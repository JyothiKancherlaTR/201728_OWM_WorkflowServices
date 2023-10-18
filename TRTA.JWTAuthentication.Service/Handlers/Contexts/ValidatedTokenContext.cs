using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using TRTA.OSP.Authentication.Service.DTO;

namespace TRTA.OSP.Authentication.Service.Handlers.Contexts
{
    public class ValidatedTokenContext<Options> : ResultContext<Options> where Options : AuthenticationSchemeOptions
    {
        public ValidatedTokenContext(
            HttpContext context,
            AuthenticationScheme scheme,
            Options options)
            : base(context, scheme, options)
        {


        }

        public ValidateTokenResult SecurityToken { get; set; }

        public new void Success()
        {
            base.Success();
            HandleRequestResult.Handle();
        }

    }

    public class UdsValidatedTokenContext : ValidatedTokenContext<UdsAuthenticationOptions>
    {
        public UdsValidatedTokenContext(
            HttpContext context,
            AuthenticationScheme scheme,
            UdsAuthenticationOptions options)
            : base(context, scheme, options) { }

    }

}
