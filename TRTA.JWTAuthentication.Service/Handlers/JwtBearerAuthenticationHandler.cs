using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using TRTA.OSP.Authentication.Service.Constants;
using TRTA.OSP.Authentication.Service.DTO;
using TRTA.OSP.Authentication.Service.Extensions;
using TRTA.OSP.Authentication.Service.Interfaces;

namespace TRTA.OSP.Authentication.Service.Handlers
{
    public class JwtBearerAuthenticationHandler : AuthenticationHandler<JwtBearerOptions>
    {
        private const string SubjectString = "SubjectString";
        private ILogger<JwtBearerAuthenticationHandler> _logger;
        private ITokenService _JWTTokenService;

        public JwtBearerAuthenticationHandler(IOptionsMonitor<JwtBearerOptions> options, ILogger<JwtBearerAuthenticationHandler> logger,
            ILoggerFactory loggerFactory, UrlEncoder encoder, IDataProtectionProvider dataProtection, ISystemClock clock,
            ITokenService JWTTokenService
            )
            : base(options, loggerFactory, encoder, clock)
        {
            _logger = logger;
            _JWTTokenService = JWTTokenService;

        }

        /// <summary>
        /// The handler calls methods on the events which give the application control at certain points where processing is occurring.
        /// If it is not provided a default instance is supplied which does nothing when the methods are called.
        /// </summary>
        protected new JwtBearerEvents Events
        {
            get { return (JwtBearerEvents)base.Events; }
            set { base.Events = value; }
        }

        protected override Task<object> CreateEventsAsync() => Task.FromResult<object>(new JwtBearerEvents());

        private string BasicTokenValidation(HttpContext context)
        {
            string token = null;

            var authorizationHeader = context.Request.Headers.GetAuthorizationHeader();

            // If no authorization header found, nothing to process further
            if (authorizationHeader == null)
            {
                return null;
            }

            if (authorizationHeader.Scheme.Equals(JwtBearerDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase))
            {
                token = authorizationHeader.Parameter;
            }

            // If no token found, no further work possible
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            return token;
        }

        /// <summary>
        /// Searches the 'Authorization' header for a 'Bearer' token. If the 'Bearer' token is found, it is validated using <see cref="TokenValidationParameters"/> set in the options.
        /// </summary>
        /// <returns></returns>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string token = null;
            List<Exception> validationFailures = null;
            SecurityToken validatedToken = null;
            ClaimsPrincipal principal = null;

            try
            {
                token = BasicTokenValidation(Context);
                if (token == null) return AuthenticateResult.NoResult();

                var validationParameters = await _JWTTokenService.GetTokenValidationParametes();
                var validator = Options.SecurityTokenValidators.FirstOrDefault();

                if (validator != null && validator.CanReadToken(token))
                {
                    try
                    {
                        principal = validator.ValidateToken(token, validationParameters, out validatedToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(new EventId(211, "SLTV1012"), ex, AuthErrorCodes.TokenValidationFailed, null);

                        if (validationFailures == null)
                        {
                            validationFailures = new List<Exception>(1);
                        }

                        validationFailures.Add(ex);
                    }

                    if (validationFailures == null || !validationFailures.Any())
                    {
                        _logger.LogInformation(new EventId(205, "SLTV1013"), null, AuthErrorCodes.TokenValidated, null);

                        var tokenValidatedContext = new TokenValidatedContext(Context, Scheme, Options)
                        {
                            Principal = principal,
                            SecurityToken = validatedToken
                        };

                        await Events.TokenValidated(tokenValidatedContext);

                        if (tokenValidatedContext.Result != null)
                        {
                            return tokenValidatedContext.Result;
                        }

                        if (Options.SaveToken)
                        {
                            tokenValidatedContext.Properties.StoreTokens(new[]
                            {
                                new AuthenticationToken { Name = "access_token", Value = token }
                            });
                        }

                        Thread.CurrentPrincipal = principal;
                        Context.User = principal;
                        var substring = principal.FindFirst(LoneStarClaim.X500);
                        if( substring != null && ! string.IsNullOrEmpty(substring.Value))
                        {
                            Context.Items[SubjectString] = substring.Value;
                        }
                        else
                        {
                            AddUserTenatntDetails(principal);
                        }

                        tokenValidatedContext.Success();
                        return tokenValidatedContext.Result;
                    }
                }

                if (validationFailures != null)
                {
                    var authenticationFailedContext = new AuthenticationFailedContext(Context, Scheme, Options)
                    {
                        Exception = validationFailures.Count == 1 ? validationFailures[0] : new AggregateException(validationFailures)
                    };

                    await Events.AuthenticationFailed(authenticationFailedContext);

                    if (authenticationFailedContext.Result != null)
                    {
                        return authenticationFailedContext.Result;
                    }

                    return AuthenticateResult.Fail(authenticationFailedContext.Exception);
                }

                return AuthenticateResult.Fail("No SecurityTokenValidator available for token: " + token ?? "[null]");
            }
            catch (Exception ex)
            {
                _logger.LogError(new EventId(508, "SLTV1014"), ex, AuthErrorCodes.ProcessErrorMessage, null);

                var authenticationFailedContext = new AuthenticationFailedContext(Context, Scheme, Options)
                {
                    Exception = ex
                };

                await Events.AuthenticationFailed(authenticationFailedContext);

                if (authenticationFailedContext.Result != null)
                {
                    return authenticationFailedContext.Result;
                }

                throw;
            }
        }

        /// <summary>
        /// Addd user and tenant details if available
        /// </summary>
        /// <param name="principal"></param>
        private void AddUserTenatntDetails(ClaimsPrincipal principal)
        {
            var tenant = principal.FindFirst(LoneStarClaim.Tenant);
            var user = principal.FindFirst(LoneStarClaim.UniversalId);

            var substng = tenant != null ? $"ou={tenant.Value}," : "";
            substng += (user != null ? $"uid={user.Value}" : "");
            Context.Items[SubjectString] = substng;
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            var authHeader = Context.Request.Headers.GetAuthorizationHeader();
            if (authHeader != null && authHeader.Scheme != JwtBearerDefaults.AuthenticationScheme)
                return;
            var authResult = await HandleAuthenticateOnceSafeAsync();
            var eventContext = new JwtBearerChallengeContext(Context, Scheme, Options, properties)
            {
                AuthenticateFailure = authResult?.Failure
            };

            // Avoid returning error=invalid_token if the error is not caused by an authentication failure (e.g missing token).
            if (Options.IncludeErrorDetails && eventContext.AuthenticateFailure != null)
            {
                eventContext.Error = "invalid_token";
                eventContext.ErrorDescription = CreateErrorDescription(eventContext.AuthenticateFailure);
            }

            await Events.Challenge(eventContext);
            if (eventContext.Handled)
            {
                return;
            }

            Response.StatusCode = 401;

            if (string.IsNullOrEmpty(eventContext.Error) &&
                string.IsNullOrEmpty(eventContext.ErrorDescription) &&
                string.IsNullOrEmpty(eventContext.ErrorUri))
            {
                return;
            }
            else
            {
                var builder = new StringBuilder(Options.Challenge);
                if (Options.Challenge.IndexOf(" ", StringComparison.Ordinal) > 0)
                {
                    // Only add a comma after the first param, if any
                    builder.Append(',');
                }
                if (!string.IsNullOrEmpty(eventContext.Error))
                {
                    builder.Append(" error=\"");
                    builder.Append(eventContext.Error);
                    builder.Append("\"");
                }
                if (!string.IsNullOrEmpty(eventContext.ErrorDescription))
                {
                    if (!string.IsNullOrEmpty(eventContext.Error))
                    {
                        builder.Append(",");
                    }

                    builder.Append(" error_description=\"");
                    builder.Append(eventContext.ErrorDescription);
                    builder.Append('\"');
                }
                if (!string.IsNullOrEmpty(eventContext.ErrorUri))
                {
                    if (!string.IsNullOrEmpty(eventContext.Error) ||
                        !string.IsNullOrEmpty(eventContext.ErrorDescription))
                    {
                        builder.Append(",");
                    }

                    builder.Append(" error_uri=\"");
                    builder.Append(eventContext.ErrorUri);
                    builder.Append('\"');
                }
                await Response.WriteAsync(builder.ToString());
            }
        }

        private static string CreateErrorDescription(Exception authFailure)
        {
            IEnumerable<Exception> exceptions;
            if (authFailure is AggregateException)
            {
                var agEx = authFailure as AggregateException;
                exceptions = agEx.InnerExceptions;
            }
            else
            {
                exceptions = new[] { authFailure };
            }

            var messages = new List<string>();

            foreach (var ex in exceptions)
            {
                // Order sensitive, some of these exceptions derive from others
                // and we want to display the most specific message possible.
                if (ex is SecurityTokenInvalidAudienceException)
                {
                    messages.Add(AuthErrorCodes.InvalidAudience);
                }
                else if (ex is SecurityTokenInvalidIssuerException)
                {
                    messages.Add(AuthErrorCodes.InvalidIssuer);
                }
                else if (ex is SecurityTokenNoExpirationException)
                {
                    messages.Add(AuthErrorCodes.MissingExpiration);
                }
                else if (ex is SecurityTokenInvalidLifetimeException)
                {
                    messages.Add(AuthErrorCodes.InvalidLifetime);
                }
                else if (ex is SecurityTokenNotYetValidException)
                {
                    messages.Add(AuthErrorCodes.TokenNotYetValid);
                }
                else if (ex is SecurityTokenExpiredException)
                {
                    messages.Add(AuthErrorCodes.TokenExpired);
                }
                else if (ex is SecurityTokenSignatureKeyNotFoundException)
                {
                    messages.Add(AuthErrorCodes.TokenMissingSignature);
                }
                else if (ex is SecurityTokenInvalidSignatureException)
                {
                    messages.Add(AuthErrorCodes.InvalidSignature);
                }
            }

            return string.Join("; ", messages);
        }

    }
}