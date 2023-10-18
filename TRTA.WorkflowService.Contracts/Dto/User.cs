using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TRTA.WorkflowServices.Contracts.Dto
{
    public class User
    {      

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
        /// UserType
        /// </summary>
        [JsonProperty("userType")]
        public string UserType { get; set; }
    }
}
