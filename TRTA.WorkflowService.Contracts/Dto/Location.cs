using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRTA.WorkflowServices.Contracts.Dto
{
    public class Location
    {
        /// <summary>
        /// Name (name):  String
        /// The location name as it will appear on the screen.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Id (id):  String
        /// The id of the location
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
