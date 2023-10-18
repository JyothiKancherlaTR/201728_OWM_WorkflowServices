namespace TRTA.OSP.Authentication.Service.Enums
{
    /// <summary>
    /// Session Expiration reason. 
    /// </summary>
    /// <remarks>MAX 20 CHARS for names!</remarks>
    public enum SessionExpireReason
    {
        /// <summary>
        /// Not set.
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// Concurrent Users.
        /// </summary>
        ConcurrentUsers,

        /// <summary>
        /// Customer Support.
        /// </summary>
        CustomerSupport,

        /// <summary>
        /// SSO Token Expired.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Required for contract", MessageId = "SSO")]
        SSOTokenExpired,

        /// <summary>
        /// User signed off.
        /// </summary>
        UserSignedOff,

        /// <summary>
        /// User Inactivity.
        /// </summary>
        UserInactivity,

        /// <summary>
        /// No active browser.
        /// </summary>
        NoActiveBrowser,

        /// <summary>
        /// Maintenance reason. When updating a Uds session for maintenance, use the upper case spelling.
        /// </summary>
        Maintenance,

        /// <summary>
        /// User Throttling.
        /// </summary>
        UserThrottling,

        /// <summary>
        /// Targeted User.
        /// </summary>
        TargetedUser,

        /// <summary>
        /// Incomplete Sign-on.
        /// </summary>
        IncompleteSignon,

        /// <summary>
        /// UserInactive Offline.
        /// </summary>
        UserInactiveOffline
    }
}
