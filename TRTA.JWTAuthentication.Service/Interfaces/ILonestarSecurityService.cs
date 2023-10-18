using System;
using System.Threading.Tasks;
using TRTA.OSP.Authentication.Service.DTO;

namespace TRTA.OSP.Authentication.Service.Interfaces
{
    public interface ILonestarSecurityService
    {
        /// <summary>
        /// Retrieve udslongtoken from the provided JWT
        /// </summary>
        /// <param name="jwToken">JWT from which udslongtoken is derived</param>
        /// <returns></returns>
        Task<string> GetUDSLongTokenFromJWTAsync(string jwToken);

        /// <summary>
        /// Return resource check status
        /// </summary>
        /// <returns></returns>
        Task<Tuple<string, int, long>> GetResourceHealthAsync();


        /// <summary>
        /// Validate UDSLongToken 
        /// </summary>
        /// <param name="sToken"></param>
        /// <returns></returns>
        Task<ValidateTokenResult> ValidateUDSTokenAsync(string sToken);
    }
}
