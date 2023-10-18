namespace TRTA.OSP.Authentication.Service.Constants
{
    public static class HttpHeaderNames
    {
        /// <summary>
        /// The http header name for the LoneStar correlation id
        /// </summary>
        public const string LoneStarCorrelationIdHeader = "X-LoneStar-CorrelationGuid";

        /// <summary>
        /// Http header used to indicate the currently selected ONESOURCE 3-character firm account ID.
        /// The account could be either the user's home firm ID, or if the "switch account" option is
        /// in effect it holds the account ID of the new (switched to) account.
        /// </summary>
        public const string LoneStarAccountId = "X-LoneStar-AccountId";

        /// <summary>
        /// Value is set to "true" if the currently selected firm account has Client Manager enabled.
        /// </summary>
        public const string LoneStarIsClientManagerEnabled = "X-LoneStar-IsCMEnabled";

        /// <summary>
        /// If Client Manager is enabled for the currently selected firm account, this header holds the 
        /// internal Client Manager ID for the selected client in Client Manager. 
        /// Only valid if the <see cref="LoneStarIsClientManagerEnabled"/> header value is "true".
        /// </summary>
        public const string LoneStarClientManagerId = "X-LoneStar-CMId";
        
        /// <summary>
        /// This header holds the Product Firm ID as known in the target product. 
        /// The value would be there for both CM-ON and CM-OFF scenarios.
        /// </summary>
        public const string LoneStarProductFirmId = "X-LoneStar-Product-FirmId";

        /// <summary>
        /// Display culture the user has selected for ONESOURCE (e.g. “en” or “en-US” or “fr-CA” or …)
        /// </summary>
        public const string LoneStarCulture = "X-LoneStar-Culture";

        /// <summary>
        /// TRMR's internal root correlation ID
        /// </summary>
        public const string TrmrRootGuid = "X-TRMR-rootguid";

        /// <summary>
        /// Zuul router header indicating the original host the browser used.  Be careful - Zuul may tell you
        /// it was on port 80 in this string when the client actually used SSL.  Verify that via
        /// the <see cref="SslClientCipher"/> header.
        /// </summary>
        public const string ForwardedHost = "x-forwarded-host";
        /// <summary>
        /// Zuul router header indicating the original protocol (http, https) the browser used.  Be careful - Zuul may tell you
        /// it was http in this string when the client actually used SSL/https.  Verify that via
        /// the <see cref="SslClientCipher"/> header.
        /// </summary>
        public const string ForwardedProtocol = "x-forwarded-proto";
        /// <summary>
        /// The Zuul route prefix from the original URL the client called (which was stripped from the final URL actually 
        /// called on the target server).  Used to help recreate a self-referencing link.
        /// </summary>
        public const string ForwardedPrefix = "x-forwarded-prefix";

        /// <summary>
        /// Original IP address of the client before it went through a proxy server
        /// </summary>
        public const string ForwardedFor = "X-Forwarded-For";

        /// <summary>
        /// SSL cipher used by browser.  The value isn't very useful, but the presence of this header
        /// means the client was using SSL for the request.
        /// </summary>
        public const string SslClientCipher = "SSLClientCipher";

        /// <summary>
        /// One header TR F5s store remote client IP address
        /// </summary>
        public const string RemoteAddress = "REMOTEADDRESS";
        /// <summary>
        /// Another header TR F5s store remote client IP address
        /// </summary>
        public const string RiaSourceIp = "RIA-SOURCEIP";

    }
}
