namespace TRTA.OSP.Authentication.Service.Enums
{
    /// <summary>
    /// Session status.
    /// </summary>
    /// <remarks>
    /// IMPORTANT!  This enum values here _MUST_ stay exactly the same as the corresponding enum in the 
    /// UdsSessionModel.cs file of UDSServiceSupport project.  MAX 20 CHARS for names!!
    /// </remarks>
    public enum SessionStatus
    {
        /// <summary>
        /// Not Set.
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// Online Status.
        /// </summary>
        Online,

        /// <summary>
        /// Offline Status.
        /// </summary>
        Offline,

        /// <summary>
        /// Killed Status.
        /// </summary>
        Killed,

        /// <summary>
        /// Authenticated Status.
        /// </summary>
        Authenticated,

        /// <summary>
        /// Offline Stale Status.
        /// </summary>
        OfflineStale,
    }
}
