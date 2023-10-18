using Microsoft.AspNetCore.Authentication;
using TRTA.OSP.Authentication.Service.Constants;

namespace TRTA.OSP.Authentication.Service.DTO
{
    public class UdsAuthenticationOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// Gets or sets the challenge to put in the "WWW-Authenticate" header.
        /// </summary>
        public string Challenge { get; set; } = AuthConstants.UDSLongTokenAuthenticationScheme;
    } 
}
