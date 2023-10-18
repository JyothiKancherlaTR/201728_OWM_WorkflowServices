// Copyright 2013: Thomson Reuters Global Resources. All Rights Reserved. Proprietary and Confidential information of TRGR. Disclosure, Use or Reproduction without the written authorization of TRGR is prohibited.

using System;

namespace TRTA.OSP.Authentication.Service.Enums
{
    /// <summary>
    /// Specifies the types of source client type that may establish a session.
    /// </summary>
    /// <remarks>MAX 50 CHARS for names!</remarks>
    [Flags]
    public enum SessionSourceTypes
    {
        /// <summary>
        /// Default session source type.
        /// </summary>
        None = 0,

        /// <summary>
        /// Request comes from regular web browser.
        /// </summary>
        Web = 1,

        /// <summary>
        /// Request comes from mobile web browser.
        /// </summary>
        MobileWeb = 2,

        /// <summary>
        /// Request comes from desktop. 
        /// </summary>
        Desktop = 4,

        /// <summary>
        /// Request comes from Website when switching to Admin mode.
        /// </summary>
        Admin = 8,

        /// <summary>
        /// Request comes from an automated testing system
        /// </summary>
        Testing = 16,

        // Note: The integer value needs to be incremented to support bitwise Flags logic to be accurate.
    }
}
