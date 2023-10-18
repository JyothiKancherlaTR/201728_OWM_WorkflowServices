using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TRTA.OSP.OrganizationManagement.Contracts.Dtos
{
    /// <summary>
    /// Links for previous and next navigation urls
    /// </summary>
    public class CollectionNavigationLinks
    {
        /// <summary>
        /// Next pagination url
        /// </summary>
        [JsonProperty("next", NullValueHandling = NullValueHandling.Ignore)]
        public string Next { get; set; } = null;

        /// <summary>
        /// Previous pagination url
        /// </summary>
        [JsonProperty("prev", NullValueHandling = NullValueHandling.Ignore)]
        public string Previous { get; set; } = null;
    }
}
