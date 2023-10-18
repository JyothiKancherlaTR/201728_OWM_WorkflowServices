using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TRTA.OSP.Authentication.Service.DTO;
using TRTA.OSP.Authentication.Service.Interfaces;
using TRTA.OSP.Authentication.Service.Constants;
using Microsoft.Extensions.Options;
using System.Threading;
using TRTA.OSP.Authentication.Service.Extensions;

namespace TRTA.OSP.Authentication.Service.Services
{


    public class TokenService : ITokenService
    {
        private JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private ILogger<TokenService> _logger;
        private IMemoryCache _memoryCache;
        private IOptions<JwtAuthentication> _jwtConfiguration;
        private string PublicKeyPath { get; set; } = @"/api/security/v1/publickey";
        private HttpClient _httpClient;
        private IHttpContextAccessor _httpContextAccessor;
        private ILonestarSecurityService _loneStarSecurityServiceProxy;
        private static SemaphoreSlim jwtCacheLock = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim publicKeyCacheLock = new SemaphoreSlim(1, 1);


        public TokenService(ILogger<TokenService> logger, IOptions<JwtAuthentication>  jwtConfiguration,
            IMemoryCache memoryCache, IHttpContextAccessor httpContextAccessor,
            ILonestarSecurityService loneStarSecurityServiceProxy)
        {
            _httpClient = new HttpClient();
            _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            _logger = logger;
            _jwtConfiguration = jwtConfiguration;
            _memoryCache = memoryCache;
            _httpContextAccessor = httpContextAccessor;
            _loneStarSecurityServiceProxy = loneStarSecurityServiceProxy;
        }


        public async Task<TokenValidationParameters> GetTokenValidationParametes()
        {
            var securityKey = await GetPublicKey();

            return new TokenValidationParameters()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = AuthConstants.Issuer,
                ValidAudience = AuthConstants.Audience,
                IssuerSigningKey = securityKey,
                TokenDecryptionKey = securityKey
            };
        }
        

    /// <summary>
    /// This method is used to generate a JWS Token using the claims, issuer and audience
    /// </summary>
    /// <param name="claims">claims to be added to the token</param>
    /// <param name="issuer">issuer of the token</param>
    /// <param name="audience">target audience of the token</param>
    /// <param name="encrypted">Flag for switching between JWE and JWS. Generates JWE if encrypted is true else a JWS is generated</param>
    /// <returns></returns>
    public string GenerateSignedJwt(SecurityKey securityKey, IEnumerable<Claim> claims, string issuer, string audience, bool certificate)
        {
            if (claims == null) throw new ArgumentNullException(AuthErrorCodes.ClaimsNotNull);
            if (issuer == null || audience == null) throw new ArgumentNullException(AuthErrorCodes.IssuerAudienceNotNull);

            try
            {
                var signingCredentials = certificate ? new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256) : new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                ClaimsIdentity subject = new ClaimsIdentity(claims);

                SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor()
                {
                    SigningCredentials = signingCredentials,
                    Audience = audience,
                    Issuer = issuer,
                    IssuedAt = DateTime.Now,
                    Expires = DateTime.Now.AddMinutes(Constants.AuthConstants.JwtExpiryMins),
                    NotBefore = DateTime.Now,
                    Subject = subject
                };

                JwtSecurityToken token = _jwtSecurityTokenHandler.CreateJwtSecurityToken(securityTokenDescriptor);

                return _jwtSecurityTokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("JWTToken", MethodBase.GetCurrentMethod(), "Exception while creating JWT Token: " + ex.ToString());
                throw new SecurityTokenException(AuthErrorCodes.JWTTokenGenerationIssue);
            }
        }

        /// <summary>
        /// This method is used to generate a JWS Token with shared secret using the claims, issuer and audience
        /// </summary>
        /// <param name="sharedSecretKey"></param>
        /// <param name="password"></param>
        /// <param name="claims"></param>
        /// <param name="issuer"></param>
        /// <param name="audience"></param>
        /// <returns></returns>
        public string GenerateSignedJwtSharedSecret(string sharedSecretKey, IEnumerable<Claim> claims, string issuer, string audience)
        {
            if (claims == null) throw new ArgumentNullException(AuthErrorCodes.ClaimsNotNull);
            if (issuer == null || audience == null) throw new ArgumentNullException(AuthErrorCodes.IssuerAudienceNotNull);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(sharedSecretKey));

            return GenerateSignedJwt(securityKey, claims, issuer, audience, false);
        }



        private ClaimsPrincipal GetClaimsPrincipal()
        {
            return _httpContextAccessor.HttpContext.User;
        }

        public string GetNamedClaim(string claimName)
        {
            try
            {
                return GetClaimsPrincipal().Claims.Where(c => c.Type == claimName).FirstOrDefault().Value;
            }
            catch
            {
                return string.Empty;
            }           
        }


        public async Task<X509SecurityKey> GetPublicKey(string securityServiceUrl = null)
        {
            string sharedKey = _jwtConfiguration.Value.SharedSecretKey;
            var claims = new List<Claim>();
            string token = GenerateSignedJwtSharedSecret(sharedKey, claims, AuthConstants.Issuer, AuthConstants.Issuer);
            X509SecurityKey securityKey;
            string publicKey = string.Empty;

            securityKey = GetObjectFromCache<X509SecurityKey>("jwtCache");

            if (securityKey != null)
            {
                return securityKey;
            }

            try
            {               
                await publicKeyCacheLock.WaitAsync();

                securityKey = GetObjectFromCache<X509SecurityKey>("jwtCache");

                if (securityKey != null)
                {
                    return securityKey;
                }

                publicKey = await QueryServiceGetPublicKey(token);
            }
            catch (Exception exceptionNoProxy)
            {
                _logger.LogError(exceptionNoProxy ,AuthErrorCodes.SecurityServicePublicKeyNoProxy );

                try
                {
                    publicKey = await QueryServiceGetPublicKey(token, true);
                }
                catch (Exception exceptionWithProxy)
                {
                    _logger.LogError(exceptionWithProxy, AuthErrorCodes.SecurityServicePublicKeyWithProxy);                   
                }
            }
            finally
            {
                if (publicKeyCacheLock.CurrentCount == 0)
                    publicKeyCacheLock.Release();
            }

            
            var certificate = new X509Certificate2(Encoding.UTF8.GetBytes(publicKey));
            securityKey = new X509SecurityKey(certificate);
            SetCacheObject<X509SecurityKey>("jwtCache", securityKey, int.Parse(_jwtConfiguration.Value.PublicKeyCacheInMinutes));
            return securityKey;
        }

        /// <summary>
        /// **** NEED TO REVISIT THIS METHOD
        /// </summary>
        /// <param name="token"></param>
        /// <param name="securityServiceUrl"></param>
        /// <param name="useProxy"></param>
        /// <returns></returns>
        private async Task<string> QueryServiceGetPublicKey(string token, string securityServiceUrl, bool useProxy = false)
        {

            string publicKey = null;

            try
            {
                HttpClient _httpClient;
                if (useProxy && _jwtConfiguration != null && !string.IsNullOrEmpty(_jwtConfiguration.Value.ProxyAddress))
                {
                    WebProxy _proxy = new WebProxy(new Uri(_jwtConfiguration.Value.ProxyAddress), true);
                    HttpClientHandler httpClientHandler = new HttpClientHandler() { UseProxy = true, Proxy = _proxy };
                    _httpClient = new HttpClient(httpClientHandler);
                    _httpClient.DefaultRequestHeaders.ConnectionClose = false;
                }
                else
                {
                    _httpClient = new HttpClient();
                }

                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, securityServiceUrl + PublicKeyPath);
                msg.Headers.Authorization = new AuthenticationHeaderValue(AuthConstants.JWTBearerAuthenticationScheme, token);
                _logger.LogInformation(_httpClient.BaseAddress + PublicKeyPath);
                var response = await _httpClient.SendAsync(msg);
                string result = await response.Content.ReadAsStringAsync();
                publicKey = JsonConvert.DeserializeObject<string>(result);
                response.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return publicKey;
        }

        private async Task<string> QueryServiceGetPublicKey(string token, bool useProxy = false)
        {

            string publicKey = null;

            try
            {
                publicKey = await QueryServiceGetPublicKey(token, _jwtConfiguration.Value.SecurityServiceUrl, useProxy);
            }
            //Todo: Find a better way to handle the exception
            catch (Exception ex)
            {
                throw ex;
            }

            return publicKey;
        }

       

        /// <summary>
        /// A generic method for getting and setting objects to the memory cache.
        /// </summary>
        /// <typeparam name="T">The type of the object to be returned.</typeparam>
        /// <param name="cacheItemName">The name to be used when storing this object in the cache.</param>
        /// <param name="cacheTimeInMinutes">How long to cache this object for.</param>
        /// <param name="objectSettingFunction">A parameterless function to call if the object isn't in the cache and you need to set it.</param>
        /// <returns>An object of the type you asked for</returns>
        private T GetObjectFromCache<T>(string cacheItemName)
        {
            var cachedObject = (T)_memoryCache.Get(cacheItemName);
            return cachedObject;
        }

        private T SetCacheObject<T>(string cacheItemName, object item, int cacheTimeInMinutes)
        {
            MemoryCacheEntryOptions cacheOption = new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = DateTime.Now.AddMinutes(cacheTimeInMinutes) - DateTime.Now
            };

            _memoryCache.Set(cacheItemName, item, cacheOption);
            return (T)item;
        }

        /// <summary>
        /// Get UDSLongToken, if it is JWT Token at header, will convert to UDSLongToken
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetUDSLongToken()
        {

            var authHeader = _httpContextAccessor.HttpContext.Request.Headers.GetAuthorizationHeader();

            if (authHeader.Scheme == AuthConstants.UDSLongTokenAuthenticationScheme) //return if UDSLongToken is scheme
                return authHeader.Parameter;
                        
            if (authHeader.Scheme == AuthConstants.JWTBearerAuthenticationScheme) //convert if jwt scheme
            {
                var  jwtToken = authHeader.Parameter;

                if (string.IsNullOrEmpty(jwtToken))
                {
                    throw new UnauthorizedAccessException("JWT Token is empty.");
                }

                var udsToken = GetObjectFromCache<string>(jwtToken);
                if (!string.IsNullOrEmpty(udsToken))
                {
                    return udsToken;
                }

                try
                {
                    await jwtCacheLock.WaitAsync();

                    udsToken = GetObjectFromCache<string>(jwtToken);
                    if (!string.IsNullOrEmpty(udsToken))
                    {
                        return udsToken;
                    }

                    udsToken = await _loneStarSecurityServiceProxy.GetUDSLongTokenFromJWTAsync(jwtToken);

                    if (!string.IsNullOrEmpty(udsToken))
                    {
                        SetCacheObject<string>(jwtToken, udsToken, int.Parse(_jwtConfiguration.Value.CacheInMinutes));
                    }
                    else
                    {
                        _logger.LogError("Invalid UDSLongtoken obtained for JWT Token : " + jwtToken);
                        throw new Exception("Failed to get UDSLongtoken token from security service.");
                    }

                    return udsToken;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, message: "Failed to get UDSLongtoken for JWT Token : " + jwtToken);
                    throw ex;
                }
                finally
                {
                    if (jwtCacheLock.CurrentCount == 0)
                        jwtCacheLock.Release();
                }
            }

            throw new UnauthorizedAccessException("Authentication token is empty.");
        }
    }
}
