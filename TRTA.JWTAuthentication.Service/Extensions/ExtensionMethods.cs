using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using TRTA.LoneStar.Base.Models;
using TRTA.OSP.Authentication.Service.Constants;
using TRTA.OSP.Authentication.Service.DTO;
using TRTA.OSP.Authentication.Service.Handlers;

namespace TRTA.OSP.Authentication.Service.Extensions
{
    /// <summary>
    /// Extension method for http context and header
    /// </summary>
    public static class AuthenticationExtensions
    {
        public static AuthenticationHeaderValue GetAuthorizationHeader(this IHeaderDictionary headers)
        {
            var rm = new HttpRequestMessage();

            if (headers.ContainsKey("Authorization"))
            {
                var udsToken = headers["Authorization"].ToString();
                rm.Headers.Add("Authorization", udsToken);
            }

            return rm.Headers.Authorization;
        }

        public static string GetHeader(this HttpContext context, string headerName)
        {
            return context.Request.Headers[headerName];
        }

        public static bool AddClaimToIdentity(this HttpContext context, ClaimsIdentity identity, string headerName, string defaultValue = null, string headerIssuer = AuthConstants.HttpHeaderIssuer, bool changeValueToLowercase = false)
        {
            string headerValue = context.GetHeader(headerName);

            return identity.AddClaimToIdentity(headerValue, headerName, defaultValue, headerIssuer,
                changeValueToLowercase);
        }

        /// <summary>
        /// Add attributes to claims
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="claimValue"></param>
        /// <param name="headerName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="headerIssuer"></param>
        /// <param name="changeValueToLowercase"></param>
        /// <returns></returns>
        public static bool AddClaimToIdentity(this ClaimsIdentity identity, string claimValue, string headerName,
            string defaultValue = null, string headerIssuer = AuthConstants.HttpHeaderIssuer,
            bool changeValueToLowercase = false)
        {
            if (string.IsNullOrEmpty(claimValue) && !string.IsNullOrEmpty(defaultValue))
            {
                claimValue = defaultValue;
            }

            if (string.IsNullOrEmpty(claimValue))
            {
                return false;
            }

            if (changeValueToLowercase)
            {
                claimValue = claimValue.ToLowerInvariant();
            }

            identity.AddClaim(new Claim(headerName, claimValue, null, AuthConstants.HttpHeaderIssuer));

            return true;
        }

        /// <summary>
        /// Check if alaready authenticated
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ValidateTokenResult IsUserAuthenticated(this HttpContext context)
        {
            var output = new ValidateTokenResult();
            if (context.User is LoneStarUserPrincipal)
            {
                output.Attributes = ((LoneStarUserPrincipal)context.User).Attributes;

                output.Subject = ((LoneStarUserPrincipal)context.User).Subject;

                output.Succeeded = true;
            }
            else
            {
                output.Succeeded = false;
            }

            return output;
        }
    }
}