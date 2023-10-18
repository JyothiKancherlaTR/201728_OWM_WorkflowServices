using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using TRTA.WorkflowServices.Contracts.Dto;

namespace TRTA.WorkflowServices.ExternalServices
{
    public interface IUserAdminservice
    {
        Task<UserAdminServiceUser> GetUserDetailsByLoginId(string universalId, string tenantId, string token);

    }
    public class UserAdminservice : IUserAdminservice
    {
        private readonly ILogger<UserAdminservice> logger;
        private readonly HttpClient client;
        private readonly IConfiguration configuration;

        public UserAdminservice(ILogger<UserAdminservice> logger, IConfiguration configuration, HttpClient client) 
        {
            this.logger = logger;
            this.client = client;
            this.configuration = configuration;

            client.BaseAddress = configuration.GetValue<System.Uri>("ExternalServices:UserAdminServiceUrl");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        }
        public async Task<UserAdminServiceUser> GetUserDetailsByLoginId(string universalId, string tenantId, string token)
        {
            HttpRequestMessage request = CreateRequestMessage($"{UserAdminServiceConstants.GetUserDetailsByLoginIdPath}?loginId={universalId}", HttpMethod.Get, token);
            logger.LogInformation(client.BaseAddress + UserAdminServiceConstants.GetUserDetailsByLoginIdPath);
            var userDetailResponse = await client.SendAsync(request).ConfigureAwait(false);
            var userDetailResult = userDetailResponse.Content.ReadAsStringAsync().Result;

            //User was not found, return the error
            if (!userDetailResponse.IsSuccessStatusCode)
            {
                // parse User Detail error
                var msg = JObject.Parse(userDetailResult)["Message"].ToString();
                if (!string.IsNullOrEmpty(msg) && msg.Contains("Exception Type: DataNotFound"))
                {
                    throw new Exception($"User not found - {universalId}");
                }
                
            }

            UserAdminServiceUser userDetails = JsonConvert.DeserializeObject<UserAdminServiceUser>(userDetailResult);

            if (userDetails == null)
                throw new Exception($"User not found - {universalId}");

            return userDetails;

        }

        private HttpRequestMessage CreateRequestMessage(string path, HttpMethod method, string token, object payload = null)
        {
            var request = new HttpRequestMessage(method, path);
            request.Headers.Add("Authorization", $"UDSLongToken {token}");
           // request.Headers.Add("X-LoneStar-AccountId", new List<String>() { targetTenantId });

            // map to UserAdmin SaveUser model
            var json = JsonConvert.SerializeObject(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return request;
        }
    }
}
