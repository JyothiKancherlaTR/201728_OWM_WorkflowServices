using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using TRTA.OSP.Authentication.Service.Constants;
using TRTA.OSP.Authentication.Service.DTO;

namespace TRTA.LoneStar.Base.Models
{
    /// <summary>
    /// All of the built-in token handlers will set the Principal for the call as an instance of LoneStarUserPrincipal.
    /// There are a number of "value-added" features available so it is generally worth your time to cast the
    /// User.Principal as a LoneStarUserPrincipal to get the benefit of these extra properties and methods.
    /// The enclosed IIdentity is constrained to be a ClaimsIdentity only and cannot be null.
    /// </summary>
    public class LoneStarUserPrincipal : ClaimsPrincipal, IPrincipal
    {
        /// <summary>
        /// If the token was a UDSLongToken, this field will hold the hydrated LoneStarSessionModel for the session.
        /// For best performance, this instance is only created if a caller actually asks for the session.
        /// </summary>
        protected LoneStarSessionModel LoneStarSession = null;

        /// <summary>
        /// The original attributes from the authenticated user
        /// </summary>
        public IDictionary<string, string> Attributes { get; set; }

        /// <summary>
        /// The original subject for the authenticated user
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Authenticated flag
        /// </summary>
        public bool IsAuthenticated { get; } = true;

        /// <summary>
        /// Custom IPrincipal for LoneStar services; exposes in direct properties some of the more useful 
        /// elements from the token
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="attributes"></param>
        /// <param name="subject"></param>
        public LoneStarUserPrincipal(IIdentity identity, IDictionary<string, string> attributes = null, string subject = null)
        {
            var claimsIdentity = identity as ClaimsIdentity;
            if (claimsIdentity == null)
                throw new InvalidOperationException("LoneStarUserPrincipal requires a ClaimsIdentity instance for the identity parameter");

            Identity = identity;
            
            // If subject is standard ONESOURCE x500 form, extract the firm and user ids into properties
            // There are a couple of token subjects possible; one with x500 syntax already, and one with just a standalone user id
            var x500 = identity.Name.Contains("=") ? new X500Name(identity.Name) : new X500Name("uid=" + identity.Name);

            UniversalId = x500.UserID;
            TenantCode = x500.OrganizationalUnit.ToUpperInvariant();
            AddIdentities(new List<ClaimsIdentity> { claimsIdentity });
            // look for any of the known email attributes and set the EmailAddress property if found
            var eMail = GetAttribute(typeof(LoneStarSessionModel).Name + ".EmailAddress");
            if (String.IsNullOrEmpty(eMail))
                eMail = GetAttribute("EmailAddress");
            if (String.IsNullOrEmpty(eMail))
                eMail = GetAttribute(TokenAttributeNames.ATTR_USER_EMAIL);
            if (!String.IsNullOrEmpty(eMail))
                EmailAddress = eMail;

            // SSO tokens support passing in roles; set the role claims if present
            var roles = GetAttribute(TokenAttributeNames.ATTR_USER_ROLE);
            if (!String.IsNullOrEmpty(roles))
            {
                string[] arRoles = roles.Split(',', ';');
                foreach (var s in arRoles)
                    claimsIdentity.AddClaim(new Claim(claimsIdentity.RoleClaimType, s));
            }

            if (attributes != null)
            {
                this.Attributes = attributes;
            }
            if (!string.IsNullOrEmpty(subject))
            {
                this.Subject = subject;
            }
        }

        /// <summary>
        /// ONESOURCE Universal ID (the value of the uid= portion of the x500 subject string)
        /// </summary>
        public string UniversalId { get; private set; }

        /// <summary>
        /// ONESOURCE home firm account ID (the value of the ou= portion of the x500 subject string)
        /// </summary>
        /// <seealso cref="SelectedTenant"/>
        public string TenantCode { get; private set; }

        /// <summary>
        /// email address associated with the user, if determined by the token handler
        /// </summary>
        public string EmailAddress { get; private set; }

        /// <summary>
        /// Retrieve a token attribute by name from the claims; null if attribute not present
        /// </summary>
        public string GetAttribute(string attributeName)
        {
            var claim = ClaimsIdentity.FindFirst(attributeName);
            if (claim != null)
                return claim.Value;
            return null;
        }

        /// <summary>
        /// Expose the identity as required by the IPrincipal interface
        /// </summary>
        public override IIdentity Identity { get; }

        /// <summary>
        /// Expose the identity as the type it actually is, a ClaimsIdentity.
        /// The enclosed IIdentity is constrained to be a ClaimsIdentity only and cannot be null, so this property is guaranteed to
        /// not be null.
        /// </summary>
        public ClaimsIdentity ClaimsIdentity
        {
            get { return Identity as ClaimsIdentity; }
        }
    }
}