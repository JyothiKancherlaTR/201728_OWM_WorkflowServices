using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TRTA.OSP.Authentication.Service.Interfaces
{
    public interface ITokenService
    {

        string GenerateSignedJwt(SecurityKey securityKey, IEnumerable<Claim> claims, string issuer, string audience, bool certificate);

        string GenerateSignedJwtSharedSecret(string sharedSecretKey, IEnumerable<Claim> claims, string issuer, string audience);

        string GetNamedClaim(string claimName);

        Task<X509SecurityKey> GetPublicKey(string securityServiceUrl = null);

        Task<string> GetUDSLongToken();

        Task<TokenValidationParameters> GetTokenValidationParametes();
    }
}
