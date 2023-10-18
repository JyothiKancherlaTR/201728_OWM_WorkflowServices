using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using TRTA.OSP.Authentication.Service.DTO;

namespace TRTA.OSP.Authentication.Service.Handlers.Contexts
{
    public class GenericChallengeContext<T> : PropertiesContext<T> where T : AuthenticationSchemeOptions
    {
        public GenericChallengeContext(
            HttpContext context,
            AuthenticationScheme scheme,
            T options,
            AuthenticationProperties properties)
            : base(context, scheme, options, properties) { }

        /// <summary>
        /// Any failures encountered during the authentication process.
        /// </summary>
        public Exception AuthenticateFailure { get; set; }

        ///// <summary>
        ///// Gets or sets the "error" value returned to the caller as part
        ///// of the WWW-Authenticate header. This property may be null when
        ///// <see cref="UdsAuthenticationOptions.IncludeErrorDetails"/> is set to <c>false</c>.
        ///// </summary>
        public string Error { get; set; }

        ///// <summary>
        ///// Gets or sets the "error_description" value returned to the caller as part
        ///// of the WWW-Authenticate header. This property may be null when
        ///// <see cref="UdsAuthenticationOptions.IncludeErrorDetails"/> is set to <c>false</c>.
        ///// </summary>
        public string ErrorDescription { get; set; }

        /// <summary>
        /// Gets or sets the "error_uri" value returned to the caller as part of the
        /// WWW-Authenticate header. This property is always null unless explicitly set.
        /// </summary>
        public string ErrorUri { get; set; }

        /// <summary>
        /// If true, will skip any default logic for this challenge.
        /// </summary>
        public bool Handled { get; private set; }

        /// <summary>
        /// Skips any default logic for this challenge.
        /// </summary>
        public void HandleResponse() => Handled = true;
    }
 

    public class UdsChallengeContext : GenericChallengeContext<UdsAuthenticationOptions>
    {
        public UdsChallengeContext(
            HttpContext context,
            AuthenticationScheme scheme,
            UdsAuthenticationOptions options,
            AuthenticationProperties properties)
            : base(context, scheme, options, properties) { }
    }

}
