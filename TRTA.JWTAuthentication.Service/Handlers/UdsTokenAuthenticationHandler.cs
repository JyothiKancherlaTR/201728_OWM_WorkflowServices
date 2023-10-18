using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using TRTA.LoneStar.Base.Models;
using TRTA.OSP.Authentication.Service.Constants;
using TRTA.OSP.Authentication.Service.DTO;
using TRTA.OSP.Authentication.Service.Extensions;
using TRTA.OSP.Authentication.Service.Handlers.Contexts;
using TRTA.OSP.Authentication.Service.Interfaces;

namespace TRTA.OSP.Authentication.Service.Handlers
{
    /// <summary>
    /// Udslongtoken authentication handler
    /// </summary>
    public class UdsTokenAuthenticationHandler : AuthenticationHandler<UdsAuthenticationOptions>
    {
        private const string SubjectString = "SubjectString";
        private readonly ILonestarSecurityService _lonestarSecurityService;
        private ILogger<UdsTokenAuthenticationHandler> _logger;

        private string InvalidTokenDesc { get { return AuthErrorMessages.InvalidTokenMessage; } }

        private string InvalidTokenError { get { return AuthErrorMessages.InvalidTokenError; } }
          
        public UdsTokenAuthenticationHandler(IOptionsMonitor<UdsAuthenticationOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock, ILonestarSecurityService lonestarSecurityService, ILogger<UdsTokenAuthenticationHandler> Logger)
            : base(options, logger, encoder, clock)
        {
            _lonestarSecurityService = lonestarSecurityService;
            _logger = Logger;
        }

        //TODO: Tests on this method
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            ValidateTokenResult sessionResult = null;
            var authHeader = Context.Request.Headers.GetAuthorizationHeader();

            if (authHeader == null || authHeader.Scheme != AuthConstants.UDSLongTokenAuthenticationScheme)
                return AuthenticateResult.NoResult();

            sessionResult = Context.IsUserAuthenticated();
            bool isAuthenticated = sessionResult.Succeeded; 

            if (isAuthenticated) 
                return AuthenticateResult.NoResult(); 
            else 
                sessionResult = await _lonestarSecurityService.ValidateUDSTokenAsync(authHeader.Parameter); 

            if (sessionResult == null || !sessionResult.Succeeded)
            {
                Exception exception = null;

                exception = sessionResult?.Exception != null ? sessionResult.Exception : new Exception(InvalidTokenDesc); 

                var authenticationFailedContext = new UdsAuthenticationFailedContext(Context, Scheme, Options) { Exception = exception };
                
                return AuthenticateResult.Fail(authenticationFailedContext.Exception);
                //failed.. handle the context flow in the challenge response
            }

            // success - now we need to create the auth ticket 
            var claimsPrincipal = CreateIdentity(this.Context, sessionResult.Subject, authHeader.Scheme, sessionResult.Attributes,
                ClaimsIssuer);

            System.Threading.Thread.CurrentPrincipal = claimsPrincipal;
            Context.User = claimsPrincipal;
            var substring = claimsPrincipal.FindFirst(LoneStarClaim.X500);
            if (substring != null && !string.IsNullOrEmpty(substring.Value))
            {
                Context.Items[SubjectString] = substring.Value;
            }

            var validatedTokenContext = new UdsValidatedTokenContext(Context, Scheme, Options)
            {
                Principal = claimsPrincipal,
                SecurityToken = sessionResult
            };

            validatedTokenContext.Success();

            return validatedTokenContext.Result;
        }
              
       
        /// <summary>
        /// Creates authentication principal
        /// </summary>
        /// <param name="context"></param>
        /// <param name="subject"></param>
        /// <param name="tokenScheme"></param>
        /// <param name="attributes"></param>
        /// <param name="issuer"></param>
        /// <param name="isAnonymous"></param>
        /// <returns></returns>
        public ClaimsPrincipal CreateIdentity(HttpContext context, string subject, string tokenScheme,
            IDictionary<string, string> attributes, string issuer, bool isAnonymous = false)
        {
            ClaimsIdentity identity = null;

            try
            {
                if (context == null)
                    throw new Exception("Context cannot be null");
                // create an identity and put the attributes into the Claims collection 
                identity = new ClaimsIdentity(new GenericIdentity(subject, tokenScheme));
                identity.BootstrapContext = context.Request.Headers.GetAuthorizationHeader();

                if (attributes != null)
                {
                    foreach (string s in attributes.Keys)
                    {
                        string claimValue = attributes[s];
                        identity.AddClaim(new Claim(s, claimValue, null, issuer));
                    }
                }

                if (!isAnonymous)
                {
                    context.AddClaimToIdentity(identity, HttpHeaderNames.LoneStarAccountId);

                    context.AddClaimToIdentity(identity, HttpHeaderNames.LoneStarIsClientManagerEnabled,
                        changeValueToLowercase: true);

                    // Set the LoneStar Product Firm Id, reading it from the header.
                    context.AddClaimToIdentity(identity, HttpHeaderNames.LoneStarProductFirmId);

                    //Set LoneStar Culture, reading it from header. If there is no value in the header, it defaults it to en-US.
                    string defaultCulture = "en-US";

                    context.AddClaimToIdentity(identity, HttpHeaderNames.LoneStarCulture, defaultCulture);
                }
            }
            catch (Exception)
            {
                throw;
            }

            // set the http context current user to the hold the identity
            return new LoneStarUserPrincipal(identity, attributes, subject);
        }

        
        /// <summary>
        /// Handle token validation failures
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            var authHeader = Context.Request.Headers.GetAuthorizationHeader();
            if (authHeader != null && authHeader.Scheme != AuthConstants.UDSLongTokenAuthenticationScheme)
                return;
            var authResult = await HandleAuthenticateOnceSafeAsync();
            var eventContext = new UdsChallengeContext(Context, Scheme, Options, properties)
            {
                AuthenticateFailure = authResult?.Failure
            };

            if (authResult == AuthenticateResult.NoResult())
            {
                return;
            }

            if (authResult != null && authResult.Succeeded)
            {
                eventContext.HandleResponse();
                return;
            }

            // Avoid returning error=invalid_token if the error is not caused by an authentication failure (e.g missing token).
            if (eventContext.AuthenticateFailure != null)
            {
                eventContext.Error = InvalidTokenError;
                eventContext.ErrorDescription = InvalidTokenDesc;
            }           

            Response.StatusCode = 401;

            if (string.IsNullOrEmpty(eventContext.Error) &&
                string.IsNullOrEmpty(eventContext.ErrorDescription) &&
                string.IsNullOrEmpty(eventContext.ErrorUri))
            {
                await Response.WriteAsync(string.Empty);
            }
            else
            { 
                var builder = new StringBuilder(Options.Challenge);

                if (Options.Challenge.IndexOf(" ", StringComparison.Ordinal) > 0)
                { 
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
    }

}