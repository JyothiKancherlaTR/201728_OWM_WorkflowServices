using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using TRTA.OSP.Authentication.Service.DTO;

namespace TRTA.OSP.Authentication.Service.Handlers.Contexts
{
    public abstract class AuthenticationFailedContext<Options> : ResultContext<Options> where Options : AuthenticationSchemeOptions
    {
        protected AuthenticationFailedContext(
            HttpContext context,
            AuthenticationScheme scheme,
            Options options)
            : base(context, scheme, options)
        {            
        }
        public Exception Exception { get; set; }
    }

    public class UdsAuthenticationFailedContext : AuthenticationFailedContext<UdsAuthenticationOptions>
    {
        public UdsAuthenticationFailedContext(
            HttpContext context,
            AuthenticationScheme scheme,
            UdsAuthenticationOptions options)
            : base(context, scheme, options)
        {            
        }
    } 
}
