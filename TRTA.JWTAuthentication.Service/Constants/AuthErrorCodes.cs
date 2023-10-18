namespace TRTA.OSP.Authentication.Service.Constants
{

    /// <summary>
    /// 
    /// </summary>
    public class AuthErrorCodes
    {
        // SLTV - SL Token Validation

        /// <summary>
        /// Invalid Audience
        /// </summary>
        public static string InvalidAudience = "SLTV1001: The audience is invalid";

        /// <summary>
        /// Invalid Issuer
        /// </summary>
        public static string InvalidIssuer = "SLTV1002: The issuer is invalid";

        /// <summary>
        /// Missing Expiration
        /// </summary>
        public static string MissingExpiration = "SLTV1003: The token has no expiration";

        /// <summary>
        /// Invalid Life time
        /// </summary>
        public static string InvalidLifetime = "SLTV1004: The token lifetime is invalid";

        /// <summary>
        /// Invalid Signature
        /// </summary>
        public static string InvalidSignature = "SLTV1004: The signature is invalid";

        /// <summary>
        /// Token not yet valid
        /// </summary>
        public static string TokenNotYetValid = "SLTV1005: The token is not valid yet";

        /// <summary>
        /// Token expired
        /// </summary>
        public static string TokenExpired = "SLTV1006: The token is expired";

        /// <summary>
        /// Signature missing in the token
        /// </summary>
        public static string TokenMissingSignature = "SLTV1007: The signature key was not found";

        /// <summary>
        /// The claims not null
        /// </summary>
        public static string ClaimsNotNull = "SLTV1008: Claims cannot be null";

        /// <summary>
        /// The issuer audience not null
        /// </summary>
        public static string IssuerAudienceNotNull = "SLTV1009: Issuer and Audience cannot be null";

        /// <summary>
        /// The token not null
        /// </summary>
        public static string TokenNotNull = "SLTV1010: token cannot be null";

        /// <summary>
        /// The security key not null
        /// </summary>
        public static string SecurityKeyNotNull = "SLTV1011: Securitykey cannot be null";

        /// <summary>
        /// The token validation failed
        /// </summary>
        public static string TokenValidationFailed = "SLTV1012: Failed to validate the token";

        /// <summary>
        /// The token validated
        /// </summary>
        public static string TokenValidated = "SLTV1013: Successfully validated the token";

        /// <summary>
        /// The process error message
        /// </summary>
        public static string ProcessErrorMessage = "SLTV1014: Error while processing the message";

        /// <summary>
        /// The JWT token generation issue
        /// </summary>
        public static string JWTTokenGenerationIssue = "SLTV1015: Unable to generate JWT token";

        /// <summary>
        /// The JWT token generation issue
        /// </summary>
        public static string SecurityServicePublicKeyNoProxy = "SLSS1001: Error retrieving public key from security service - no proxy";

        /// <summary>
        /// The JWT token generation issue
        /// </summary>
        public static string SecurityServicePublicKeyWithProxy = "SLSS1002: Error retrieving public key from security service - with proxy";

        /// <summary>
        /// The JWT token generation issue
        /// </summary>
        public static string SecurityServiceException = "SLSS1003: Error in security service.";

        ///// <summary>
        ///// The JWT token generation issue
        ///// </summary>
        //public static string SecurityServiceException = "SLSS1003: Error in security service.";
    }
}
