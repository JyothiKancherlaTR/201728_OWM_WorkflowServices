using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TRTA.OSP.Authentication.Service.Constants;
using TRTA.OSP.Authentication.Service.DTO;
using TRTA.OSP.Authentication.Service.Enums;
using TRTA.OSP.Authentication.Service.Interfaces;

namespace TRTA.OSP.Authentication.Service.Services
{

    /// <summary>
    /// Proxy for Lonestar security service
    /// </summary>
    public class LoneStarSecurityService : ILonestarSecurityService
    {
        private readonly string _lsSecurityBaseUrl = null;
        private readonly HttpClient _httpClient = null;
        private const string SessionsPath = @"/api/security/v1/sessions";
        private const string CheckResource = @"/api/security/v1/resourcecheck";
        private const string ServiceRunning = "OK";
        private const string ServiceNotRunning = "Failed";
        private readonly TimeSpan SecurityServiceTimeout = new TimeSpan(0, 0, 0, 45);
        private ILogger<ILonestarSecurityService> _logger;
        private IHttpContextAccessor _httpContextAccessor;
        private readonly Dictionary<string, IEnumerable<string>> _correlationHeader = null;
        private readonly string _correlationId = null;

        public LoneStarSecurityService(HttpClient httpClient, IOptions<JwtAuthentication> jwtConfiguration, ILogger<ILonestarSecurityService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _lsSecurityBaseUrl = jwtConfiguration.Value.SecurityServiceUrl;
            _httpContextAccessor = httpContextAccessor;
            _correlationHeader = new Dictionary<string, IEnumerable<string>>();
            if (!String.IsNullOrEmpty(_correlationId))
            {
                _correlationHeader.Add(HttpHeaderNames.LoneStarCorrelationIdHeader, new string[] { _correlationId } );
            }
        }
        /// <summary>
        /// Retrieve udslongtoken from the provided JWT
        /// </summary>
        /// <param name="jwToken">JWT from which udslongtoken is derived</param>
        /// <returns></returns>
        public async Task<string> GetUDSLongTokenFromJWTAsync(string jwToken)
        {
            var udsLongTokenSvc = new Uri(_lsSecurityBaseUrl + SessionsPath);

            //find out way to get the security key.
            ClaimsPrincipal user = _httpContextAccessor.HttpContext.User as ClaimsPrincipal;

            var sessionModel = new LoneStarSessionModel
            {
                CreatedDateTime = DateTime.Now.ToUniversalTime(),
                EmailAddress = user.Claims.ToList().Where(c => c.Type == LoneStarClaim.Email).FirstOrDefault()?.Value,
                EventManagerId = string.Empty,
                ExpiresReason = SessionExpireReason.NotSet,
                FirstName = user.Claims.ToList().Where(c => c.Type == LoneStarClaim.FirstName).FirstOrDefault()?.Value,

                // IpAddress = Utility.GetClientIp(),
                //Lastname property is not present in the lonestar claim
                LastName = user.Claims.ToList().Where(c => c.Type == LoneStarClaim.FirstName).FirstOrDefault()?.Value,
                FullName = user.Claims.ToList().Where(c => c.Type == LoneStarClaim.FirstName).FirstOrDefault()?.Value,
                OneSourceUserX500 = user.Claims.ToList().Where(c => c.Type == LoneStarClaim.X500).FirstOrDefault()?.Value,

                // Gets or sets the absolute expires date time.Should NOT be extended once set. This optional date field identifies when the session should expire --
                // the latest time at which the session is allowed to be active. If this field or the CreatedDateTime is not set by the caller on the Create
                // request, UDS will set this field to 24 hours after the created time.
                ExpiresDateTime = DateTime.Now.ToUniversalTime().AddHours(24),

                // This date field is required on the Session Create, and should be set to a time in the future at which the session could be ended due to inactivity.
                // If no update to the Session object extends the timestamp in this field, the SessionReaper could end the session behind-the-scenes. Typical inactivity
                // timeout values range from 15 to 90 minutes.
                OrphanExpiresDateTime = DateTime.Now.ToUniversalTime().AddMinutes(30),

                // The date and time when the session will end, if it has not been ended prior to that time. There is a maximum duration for every session, and this date
                // represents the max duration of this session.
                SessionExpiresDateTime = DateTime.Now.ToUniversalTime().AddHours(24),

                SessionId = Guid.NewGuid().ToString().Replace("-", ""),
                SessionSource = SessionSourceTypes.Web,
                Site = "B", // TODO: Need to figure out what default site to use
                Status = SessionStatus.Authenticated,
                Tier = 1,
                UserCategory = ((user.Claims.ToList().Where(c => c.Type == LoneStarClaim.IsAdmin).FirstOrDefault()?.Value != null) ? SessionUserCategories.PortalAdmin : SessionUserCategories.NormalUser)
            };

            var udsLongToken = await this.SendAysnc<string>(sessionModel,
                udsLongTokenSvc,
                AuthConstants.JWTBearerAuthenticationScheme, jwToken, HttpMethod.Post, SecurityServiceTimeout,
                null, null);

            return udsLongToken;
        }

        /// <summary>
        /// Make this as a extension method
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="value"></param>
        /// <param name="fullUrl"></param>
        /// <param name="authorizationScheme"></param>
        /// <param name="authorizationValue"></param>
        /// <param name="method"></param>
        /// <param name="timeout"></param>
        /// <param name="requestHeaders"></param>
        /// <param name="responseHeaders"></param>
        /// <returns></returns>
        private async Task<TOutput> SendAysnc<TOutput>(object value, Uri fullUrl, string authorizationScheme, string authorizationValue,
            HttpMethod method = null, TimeSpan timeout = default(TimeSpan),
            Dictionary<string, IEnumerable<string>> requestHeaders = null,
            Dictionary<string, IEnumerable<string>> responseHeaders = null)
        {
            HttpResponseMessage response = null;
            StringBuilder logDetails = new StringBuilder();
            Dictionary<string, object> lonestarHeaders = new Dictionary<string, object>();
            logDetails.AppendLine("Request to: " + fullUrl);
            Stopwatch requestTimer = new Stopwatch();
            TOutput result = default(TOutput);
            var cts = new CancellationTokenSource();

            try
            {
                string jsonValue = JsonConvert.SerializeObject(value);
                var content = new StringContent(jsonValue);
                var header = new MediaTypeHeaderValue("application/json");
                content.Headers.ContentType = header;

                method = method == null ? method = HttpMethod.Post : method;
                
                logDetails.AppendLine("HttpMethod is: " + method);
                HttpRequestMessage msg;
                msg = method == HttpMethod.Get ? new HttpRequestMessage(method, fullUrl) : new HttpRequestMessage(method, fullUrl) { Content = content };

                if (authorizationScheme != null)
                {
                    msg.Headers.Authorization = new AuthenticationHeaderValue(authorizationScheme, authorizationValue);
                }

                msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                requestHeaders?.ToList().ForEach(h => { if (h.Key.ToLower().Contains("lonestar")) { lonestarHeaders[h.Key] = h.Value; } });

                requestTimer.Start();
                response = await _httpClient.SendAsync(msg, cts.Token);
                requestTimer.Stop();
                logDetails.AppendLine("ElapsedTimeInMilliSecs: " + requestTimer.ElapsedMilliseconds);
                logDetails.AppendLine("Response Status Code: " + response.StatusCode);
                requestTimer = null;

                responseHeaders?.ToList().ForEach(h => { if (h.Key.ToLower().Contains("lonestar")) { lonestarHeaders[h.Key] = h.Value; } });

                var responseContent = await response.Content.ReadAsStringAsync();

                // if the response is an error, throw an exception with the response
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Security service failed to reutrn UDSLongToken for given jwt. Status code :" + response.StatusCode.ToString());
                    throw new Exception(AuthErrorMessages.FailToGetUDSLongToken + " Status code: " + response.StatusCode.ToString());
                }

                result = JsonConvert.DeserializeObject<TOutput>(responseContent);
                response.Dispose();

                logDetails.AppendLine("Done with the external service call. Returning to the caller.");
               
            }
            catch (TaskCanceledException exception)
            {
                _logger.LogError(exception, message: "Exception in LoneStarSecurityServiceProxy. Task timeout.", MethodBase.GetCurrentMethod(), lonestarHeaders);
                if(exception.InnerException != null)
                {
                    _logger.LogError(exception.InnerException, message: "Inner Exception in LoneStarSecurityServiceProxy. Task timeout.", MethodBase.GetCurrentMethod(), lonestarHeaders);
                }

                if(exception.CancellationToken == cts.Token)
                {
                    _logger.LogError(exception, message: "Security servie HTTP Send operation has been timed out.", MethodBase.GetCurrentMethod(), lonestarHeaders);
                }

                throw exception;
            }
            catch (Exception exception)
            {
                if (requestTimer != null && requestTimer.IsRunning)
                {
                    requestTimer.Stop();
                    var responseElapsedTime = requestTimer.ElapsedMilliseconds;
                    logDetails.AppendLine("Total Request Elapsed time with error: " + responseElapsedTime);
                    requestTimer = null;
                }
              
                _logger.LogError(exception, message: "Exception occured in LoneStarSecurityServiceProxy", MethodBase.GetCurrentMethod(), lonestarHeaders);
                throw exception;
            }
            finally
            {
                _logger.LogInformation("LoneStarSecurityServiceProxy.SendAysnc ", MethodBase.GetCurrentMethod(),
                           logDetails.ToString(), lonestarHeaders);
                logDetails.Clear();
                logDetails = null;
                lonestarHeaders = null;
            }

            return result;
        }


        public async Task<LoneStarSessionModel> GetUDSLongTokenSessionAsync(string sLongToken)
        {
            var sessionsSvc = new Uri(_lsSecurityBaseUrl + SessionsPath);
            _correlationHeader.Add("Authorization", new string[] { "UDSLongToken " + sLongToken });
            var sessionAsync = await SendAysnc<LoneStarSessionModel>(new EmptyEntity(),
                sessionsSvc,
                AuthConstants.UDSLongTokenAuthenticationScheme, sLongToken, HttpMethod.Get, SecurityServiceTimeout,
                _correlationHeader, null);

            return sessionAsync;
        }

        /// <summary>
        /// Return security service resource health
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<string,int, long>> GetResourceHealthAsync()
        {
            var stopwatch = new Stopwatch();
            long responseTime = 0;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_lsSecurityBaseUrl + CheckResource));
                stopwatch.Start();
                _logger.LogInformation(_lsSecurityBaseUrl + CheckResource);
                var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                stopwatch.Stop();
                responseTime = stopwatch.ElapsedMilliseconds;

                if (response.IsSuccessStatusCode)
                {
                    return new Tuple<string, int,long>(ServiceRunning, (int)System.Net.HttpStatusCode.OK, responseTime);
                }
                else
                {
                    return new Tuple<string, int, long>(ServiceNotRunning, (int)response.StatusCode, responseTime);
                }
            }
            catch(Exception ex)
            {
                if (stopwatch != null && stopwatch.IsRunning)
                {
                    stopwatch.Stop();
                    responseTime = stopwatch.ElapsedMilliseconds;
                }

                return new Tuple<string, int, long>("Exception Occured. " + ex.ToString(), (int)System.Net.HttpStatusCode.InternalServerError, responseTime);
            }          
            
        }

        /// <summary>
        /// Validate UDSLongToken
        /// </summary>
        /// <param name="sToken"></param>
        /// <returns></returns>
        public async Task<ValidateTokenResult> ValidateUDSTokenAsync(string sToken)
        {
            var output = new ValidateTokenResult();

            try
            {
                // see if the token is in transient token cache; pull from cache if found.
                // note the token cache only holds tokens for a few seconds, this is mainly to help with burst queries
                // using the same token. 
                IDictionary<string, string> attributes = null;
                string subject = null;

                // not in cache, call Security Service 
                LoneStarSessionModel session = await GetUDSLongTokenSessionAsync(sToken);

                if (session == null)
                {
                    output.Succeeded = false;
                    return output;
                }

                if (!session.IsValid())
                {
                    output.Succeeded = false;
                    return output;
                }

                subject = session.OneSourceUserX500;
                attributes = new Dictionary<string, string>(16);

                PopulateTokenAttributes(attributes, session);

                // while we are here, save a copy of the original UDS object so it can be rehydrated if desired
                string jsonValue = JsonConvert.SerializeObject(session,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                attributes.Add(session.GetType().Name, jsonValue);

                output.Succeeded = true;
                output.Subject = subject;
                output.Attributes = attributes;
                return output;
            }
            catch (HttpRequestException hre)
            {              
                output.Succeeded = false;
                output.Exception = hre;
                return output;
            }
            catch (Exception ex)
            {
               
                output.Succeeded = false;
                output.Exception = ex;
                return output;
            }
        }

        /// <summary>
        /// Create claim attributes
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="session"></param>
        private void PopulateTokenAttributes(IDictionary<string, string> attributes, LoneStarSessionModel session)
        {
            if (session == null)
                throw new Exception("UDSLongToken Session cannot be null");

            try
            {
                AddIfNotNull(attributes, "SessionId", session.SessionId);
                AddIfNotNull(attributes, "EventManagerId", session.EventManagerId);
                AddIfNotNull(attributes, "Site", session.Site);
                AddIfNotNull(attributes, "Status", session.Status.ToString());
                AddIfNotNull(attributes, "LongToken", session.LongToken);
                AddIfNotNull(attributes, "ExpiresReason", session.ExpiresReason.ToString());
                AddIfNotNull(attributes, "SessionEndedReason", session.SessionEndedReason.ToString());
                AddIfNotNull(attributes,  LoneStarClaim.FirstName, session.FirstName + " " + session.LastName);
                AddIfNotNull(attributes, "emailaddress", session.EmailAddress);
                AddIfNotNull(attributes, "OneSourceUserX500", session.OneSourceUserX500);

                if (!string.IsNullOrEmpty(session.OneSourceUserX500))
                {
                    var userInfo = session.OneSourceUserX500.Split(",").ToList();
                    AddIfNotNull(attributes, LoneStarClaim.UniversalId, userInfo.Find(x=>x.Contains("uid=")).Split("=")[1]);
                    AddIfNotNull(attributes, LoneStarClaim.Tenant, userInfo.First(x => x.Contains("ou=")).Split("=")[1]);
                }

                AddIfNotNull(attributes, "CreatedDateTime", session.CreatedDateTime.ToString());
                AddIfNotNull(attributes, "ExpiresDateTime", session.ExpiresDateTime.ToString());
                AddIfNotNull(attributes, "OrphanExpiresDateTime", session.OrphanExpiresDateTime.ToString());
                AddIfNotNull(attributes, "SessionEndedDateTime", session.SessionEndedDateTime.ToString());
                AddIfNotNull(attributes, "SessionExpiresDateTime", session.SessionExpiresDateTime.ToString());
                AddIfNotNull(attributes, "Tier", session.Tier.ToString(CultureInfo.InvariantCulture));
                AddIfNotNull(attributes, "SessionSource", session.SessionSource.ToString());
                AddIfNotNull(attributes, "IpAddress", session.IpAddress);
                AddIfNotNull(attributes, "UserCategory", session.UserCategory.ToString());
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, message: "Failed to parse UDSLongToken claim attributes.", MethodBase.GetCurrentMethod(), session);
                throw ex;              
            }
        }

        /// <summary>
        /// Helper method to add if attribute is not null
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="name"></param>
        /// <param name="s"></param>
        private void AddIfNotNull(IDictionary<string, string> attr, string name, string s)
        {
            if (!String.IsNullOrEmpty(s))
                attr.Add(name, s);
        }

    }
}
