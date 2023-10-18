namespace TRTA.OSP.Authentication.Service.Constants
{
    public static class TokenAttributeNames
    {
        /// <summary>
        /// (Standard Ping attribute) Subject name (as specified by the app's IdP, for example an x.500 formatted string)
        /// </summary>
        public const string ATTR_SUBJECT = "subject";
        /// <summary>
        /// (Standard Ping attribute) "Token not valid before" date time string in w3c GMT format yyyy-mm-ddThh:mm:ssZ
        /// </summary>
        public const string ATTR_NOT_BEFORE = "not-before";
        /// <summary>
        /// (Standard Ping attribute) "Renew Token until" date time string in w3c GMT format yyyy-mm-ddThh:mm:ssZ
        /// </summary>
        public const string ATTR_RENEW_UNTIL = "renew-until";
        /// <summary>
        /// (Standard Ping attribute) "Token not valid on or after" date time string in w3c GMT format yyyy-mm-ddThh:mm:ssZ
        /// </summary>
        public const string ATTR_NOT_ON_OR_AFTER = "not-on-or-after";
        /// <summary>
        /// (Standard Ping attribute) SAML authentication context; an internal identifier for the Ping server
        /// </summary>
        public const string ATTR_AUTHN_CONTEXT = "authnContext";
        /// <summary>
        /// (Standard TRTA-defined attribute) Source application initiating the SSO request to a target application. Not required by
        /// most scenarios, but may be used as part of an agreement between source and target apps where behavior is determined
        /// by who is launching the application. No standard value is required, though the base URL of the source application is a good
        /// choice.
        /// </summary>
        public const string ATTR_SOURCE_APPLICATION = "Thomson.TRTA.Shared.Source.Application";
        /// <summary>
        /// (Standard TRTA-defined attribute) Target application or resource being launched by the source application. Not normally
        /// needed or specified, but may be used as part of an agreement between source and target apps to specify a particular
        /// resource being requested by the source launch. No standard value is specified here since the values would depend on
        /// the agreement between the source and target apps.
        /// </summary>
        public const string ATTR_TARGET_APPLICATION = "Thomson.TRTA.Shared.Target.Application";
        /// <summary>
        /// (Standard TRTA-defined attribute) Target application launch parameters sent from the source application. Needed only
        /// when a target app documents support for particular launch parameters and you want those parameters securely stored
        /// inside the encrypted token.  If secure parameter encryption is not required, an application may also use GET parameters
        /// on the target URL as an alternative to using this attribute.
        /// </summary>
        public const string ATTR_APPLICATION_PARAMETERS = "Thomson.TRTA.Shared.Target.Application.Parameters";
        /// <summary>
        /// (Standard TRTA-defined attribute) Client machine's apparent source IP address in the source application.
        /// Not normally needed but specified here for scenarios where the source IP is needed by the target app for
        /// internal purposes and needs to verify the client is coming from the same IP address as the one the source
        /// application saw.
        /// </summary>
        public const string ATTR_CLIENT_IP_ADDRESS = "Thomson.TRTA.Shared.Client.IPAddress";
        /// <summary>
        /// (Standard TRTA-defined attribute) Federated name identifier the client used as sent from the client's own network.
        /// <para>
        /// <i>NOTE</i> any application receiving this attribute should save it in the web session state variables and
        /// pass it along to any other application that app launches. This attribute will be the only indicator to any
        /// downstream apps that the original entry was via client-network-authentication.
        /// </para>
        /// </summary>
        public const string ATTR_CLIENT_FEDERATION_NAME = "Thomson.TRTA.Shared.Client.Federation.Name";
        /// <summary>
        /// (Standard TRTA-defined attribute) The account link defederation URL to call from the target to delete the account link from
        /// the source; provided by the source app. Should be a URL into the source app that displays some GUI allowing the user
        /// to reset the SSO account link.
        /// </summary>
        public const string ATTR_SOURCE_DEFEDERATION_URL = "Thomson.TRTA.Shared.Source.Defederation.URL";
        /// <summary>
        /// (Standard TRTA-defined attribute) Unique client connection identifier the Ping server uses to identify the client's
        /// own network.  Application code can save this value so when you get a token you can look up the value in an application
        /// side table to determine who the client is.
        /// <para>
        /// <i>NOTE</i> any application receiving this attribute should save it in the web session state variables and
        /// pass it along to any other application that app launches. This attribute will allow any downstream app to know which
        /// client network the user came from.
        /// </para>
        /// </summary>
        public const string ATTR_CLIENT_FEDERATION_CONNECTION = "Thomson.TRTA.Shared.Client.Federation.Connection";
        /// <summary>
        /// (Standard TRTA-defined attribute) An optional attribute that may be set by a source (or client) application
        /// indicating a particular role for the user.  This may be used by the target application to determine what the
        /// user's authorization should be.
        /// </summary>
        public const string ATTR_USER_ROLE = "Thomson.TRTA.Shared.User.Role";
        /// <summary>
        /// (Standard TRTA-defined attribute) An optional attribute that may be set by a source (or client) application
        /// indicating the user's email address.  This may be used by the target application to set up a new user or look up
        /// an existing user.
        /// </summary>
        public const string ATTR_USER_EMAIL = "Thomson.TRTA.Shared.User.Email";
        /// <summary>
        /// (Standard TRTA-defined attribute) Target web service's account link adapter ID (provided to caller by the Ping server admin).
        /// Used only for tokens created for web services to identify the appropriate WS-Trust target application adapter inside
        /// the Ping server configuration.
        /// </summary>
        /// <remarks>
        /// This attribute MUST be provided by the source client app if you are calling a web service using account linking via
        /// a Ping server.
        /// </remarks>
        public const string ATTR_TARGET_SERVICE_ID = "Thomson.TRTA.Shared.Services.Target.Adapter";
        /// <summary>
        /// (Custom TRTA CS&amp;S-defined attribute) OneSource Tax administrator role; set to "TRUE" if the user is a
        /// firm admin in OneSource Tax
        /// </summary>
        public const string ATTR_TRTA_CSS_ONESOURCE_ADMIN = "Thomson.TRTA.CSS.OneSource.Administrator";
        /// <summary>
        /// (Custom TRTA CS&amp;S-defined attribute) Target app's account ID for the user (as mapped from the account in
        /// the OneSource portal), plus optionally the selected account if different than the user's "home" account (also the target account
        /// as mapped from the OneSource portal selected account).
        /// </summary>
        public const string ATTR_TRTA_CSS_ONESOURCE_ACCOUNT = "Thomson.TRTA.CSS.ONESOURCE.Account";
    }
}
