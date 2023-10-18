using System.Configuration;


namespace TRTA.OSP.Authentication.Service.Constants
{
    public static class AuthConstants
    {
        /// <summary>
        /// security config
        /// </summary>
        public const string SecurityConfig = "Security";

        /// <summary>
        /// Jot config
        /// </summary>
        public const string JotConfig = "JotConfig";

        /// <summary>
        ///  proxy config
        /// </summary>
        public const string WebProxyConfig = "WebProxy";

        /// <summary>
        /// Secret key for second encryption
        /// </summary>
        public const string JwtCertCacheKey = "jwtPublicCertCache";

        /// <summary>
        /// http header
        /// </summary>
        public const string HttpHeaderIssuer = "urn:http.header";

        /// <summary>
        /// Issuer 
        /// </summary>
        public const string JwtIssuer = "http://thomsonreuters.com";

        /// <summary>
        /// The authentication scheme text for Jwt
        /// </summary>
        public static string JWTBearerAuthenticationScheme = "Bearer";

        /// <summary>
        /// UDLongtoken auth scheme
        /// </summary>
        public const string UDSLongTokenAuthenticationScheme = "UDSLongToken";

        /// <summary>
        /// Apigee token validation public key
        /// </summary>
        public static string ApigeeTokenValidationPublicKey = "ApigeeTokenValidationPublicKey";

        /// <summary>
        /// PublicKey
        /// </summary>
        public static string PublicKey = "PublicKey";

        /// <summary>
        /// Issuer for jwt token received from apigee.
        /// </summary>
        public static string ApigeeTokenIssuer
        {
            get
            {
                var result = ConfigurationManager.AppSettings["ApigeeJwtIssuer"];
                return string.IsNullOrEmpty(result) ? "apigee.api.onesourcetax.com" : result;
            }
        }

        /// <summary>
        /// Certificate Path for jwt (access_token) generation from security service
        /// </summary>
        public static string CertificatePath
        {
            get
            {
                var result = ConfigurationManager.AppSettings["JwtCeritficatePath"];
                return string.IsNullOrEmpty(result) ? "~\\trinternaljwt-dev.onesourcetax.com.p12" : result;
            }
        }

        /// <summary>
        /// Audience of the jwt (access_token) generated from security service
        /// </summary>
        public static string Audience
        {
            get
            {
                var result = ConfigurationManager.AppSettings["JwtAudience"];
                return string.IsNullOrEmpty(result) ? "api.onesourcetax.com" : result;
            }
        }

        /// <summary>
        /// Issuer of the jwt (access_token) generated from security service
        /// </summary>
        public static string Issuer
        {
            get
            {
                var result = ConfigurationManager.AppSettings["JwtIssuer"];
                return string.IsNullOrEmpty(result) ? "udservice.onesourcetax.com" : result;
            }
        }

        /// <summary>
        /// Apigee token validation certificate from configuration
        /// </summary>
        public static string APITokenRequestCertUrl
        {
            get
            {
                var result = ConfigurationManager.AppSettings["APITokenRequestCertUrl"];
                string defaultUrl = "https://s3.amazonaws.com/tr-api-pub-keys/apigeetosecurity-dev.onesourcetax.com.cert";
                return string.IsNullOrEmpty(result) ? defaultUrl : result;
            }
        }

        /// <summary>
        /// Tenant service url for tenant validation from configuration
        /// </summary>
        public static string MultiTenantUrl
        {
            get
            {
                var result = ConfigurationManager.AppSettings["LS_REST_SERVICE_URL"];
                string defaultUrl = "https://ls2rest-dev2.onesourcetax.com";
                return string.IsNullOrEmpty(result) ? defaultUrl : result;
            }
        }

        /// <summary>
        /// Product service Url from configuration
        /// </summary>
        public static string ProductServiceUrl
        {
            get
            {
                var result = ConfigurationManager.AppSettings["LS_PRODUCT_SERVICE_URL"];
                string defaultUrl = "https://productservice-dev2.onesourcetax.com";
                return string.IsNullOrEmpty(result) ? defaultUrl : result;
            }
        }

        /// <summary>
        /// User service Url from configuration
        /// </summary>
        public static string UserServiceUrl
        {
            get
            {
                var result = ConfigurationManager.AppSettings["LS_USER_SERVICE_URL"];
                string defaultUrl = "https://userservice-dev2.onesourcetax.com";
                return string.IsNullOrEmpty(result) ? defaultUrl : result;
            }
        }

        /// <summary>
        /// cache expire time
        /// </summary>
        public static int CACHE_EXPIRES
        {
            get
            {
                int cacheTime;
                return int.TryParse(ConfigurationManager.AppSettings["JwtCeritficateCacheTime"], out cacheTime) ? cacheTime : 30;
            }
        }

        /// <summary>
        /// Genrated Jwt expiry mins
        /// </summary>
        public static int JwtExpiryMins
        {
            get
            {
                int result;
                return int.TryParse(ConfigurationManager.AppSettings["JwtExpiryMins"], out result) ? result : 30;
            }
        }

        /// <summary>
        /// Flag to specify whether to validate the issuer
        /// </summary>
        public static bool ValidateIssuer
        {
            get
            {
                bool result;
                return bool.TryParse(ConfigurationManager.AppSettings["JwtValidateIssuer"], out result) ? result : true;
            }
        }

        /// <summary>
        /// Flag to specify whether to validate the audience
        /// </summary>
        public static bool ValidateAudience
        {
            get
            {
                bool result;
                return bool.TryParse(ConfigurationManager.AppSettings["JwtValidateAudience"], out result) ? result : true;
            }
        }

        /// <summary>
        /// Flag to specify whether to validate the lifetime
        /// </summary>
        public static bool ValidateLifetime
        {
            get
            {
                bool result;
                return bool.TryParse(ConfigurationManager.AppSettings["JwtValidateLifetime"], out result) ? result : true;
            }
        }
    }
}
