using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TRTA.WorkflowServices.Contracts.Dto
{
    public class UserDetails
    {
        /// <summary>
        /// A GUID, in 8-4-4-4-12 format without parentheses, that uniquely identifies a user.
        /// </summary>
        [JsonProperty("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Unique login name that identifies a user.
        /// </summary>
        [JsonProperty("login")]
        public string Login { get; set; }

        /// <summary>
        /// Identifies the full name of the user. 
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Identifies the email address of the user. 
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// User status (Active - 1 ,Inactive - 0 and Waiting for registration - 2)
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }

        /// <summary>
        /// User type. Regular - 0, DataFlow provider - 1 and AuditManager provider - 2 
        /// </summary>
        [JsonProperty("type")]
        public int Type { get; set; }

        /// <summary>
        /// Mapped SsoId of the user.
        /// </summary>
        [JsonProperty("ssoId")]
        public string SsoId { get; set; }

        /// <summary>
        /// Comments on user.
        /// </summary>
        [JsonProperty("comments")]
        public string Comments { get; set; }

        /// <summary>
        /// Notes on user.
        /// </summary>
        [JsonProperty("notes")]
        public string Notes { get; set; }

        /// <summary>
        /// Determines whether the user is enabled or disabled.
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Lists locations of the user
        /// </summary>
        [JsonProperty("locations")]
        public IList<Location> Locations { get; set; }

        /// <summary>
        /// Expiration date indicates whether the account will expire or not.<br />
        /// ex. expirationDate:"" (empty string) - User account will not be expired.<br />
        /// ex. expirationDate:"2020-05-20" - User account will be expired after 2020-05-20.
        /// </summary>
        [JsonProperty("expirationDate")]
        public DateTime? ExpirationDate { get; set; } = null;

        /// <summary>
        /// lastLogin date indicates when the user last logged in<br />
        /// ex. lastLogin:"" (empty string) - User has not logged in yet.<br />
        /// ex. lastLogin:"2020-05-20" - User last logged in on 2020-05-20.
        /// </summary>
        [JsonProperty("lastLogin")]
        public DateTime? LastLogin { get; set; } = null;
    }
}
