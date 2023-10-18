namespace TRTA.OSP.Authentication.Service.Enums
{
    /// <summary>
    /// Session User category types (admin, support, etc.)
    /// </summary>
    public enum SessionUserCategories
    {
        /// <summary>
        /// Normal user
        /// </summary>
        NormalUser = 0,

        /// <summary>
        /// Portal administrator that can launch Concert for administration
        /// </summary>
        PortalAdmin,

        /// <summary>
        /// Internal TRTA support person
        /// </summary>
        SupportUser,

        /// <summary>
        /// Internal user doing service testing
        /// </summary>
        Testing,

        /// <summary>
        /// Internal monitoring user
        /// </summary>
        Monitoring,

        /// <summary>
        /// Internal DevOps user doing system maintenance
        /// </summary>
        DevOps,


    }
}
