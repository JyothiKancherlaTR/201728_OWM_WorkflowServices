namespace TRTA.OSP.Authentication.Service.DTO
{
    public class JwtAuthentication
    {
        /// <summary>
        /// Secret key for second encryption
        /// </summary>
        public string CacheInMinutes { get; set; }

        /// <summary>
        /// Secret key
        /// </summary>
        public string SharedSecretKey { get; set; }

        /// <summary>
        /// The UDS security service base URL
        /// </summary>
        /// <value>
        /// The security service URL.
        /// </value>
        public string SecurityServiceUrl { get; set; }


        /// <summary>
        /// Proxy address
        /// </summary>
        public string ProxyAddress { get; set; }

        /// <summary>
        /// Proxy public key cache in timeout
        /// </summary>
        public string PublicKeyCacheInMinutes { get; set; }
    }
}
