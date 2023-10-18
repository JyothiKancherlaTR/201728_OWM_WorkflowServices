using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;
using TRTA.WorkflowServices.Repository.Contracts;

namespace TRTA.WorkflowServices.Repository
{
    public class RepositoryBase
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<RepositoryBase> logger;
        private readonly IMemoryCache memoryCache;

        public RepositoryBase(IConfiguration configuration, ILogger<RepositoryBase> logger, IMemoryCache memoryCache)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.memoryCache = memoryCache;
        }

        protected List<CustomerDatabase> GetAllCustomerDataBases()
        {
            List<CustomerDatabase> customerDatabases = null;
            try
            {
                if (!memoryCache.TryGetValue("ALL_CUSTOMER_DATABASES", out customerDatabases))
                {
                    using (IDbConnection connection = new SqlConnection(GetAdminDBConnectionString()))
                    {
                        customerDatabases = connection.Query<dynamic>("gfr_sp_get_client_connections", null, commandType: CommandType.StoredProcedure)
                            .Select(item => new CustomerDatabase()
                            {
                                CID = item.cid,
                                Server = item.server,
                                DB = item.db,
                                UserName = item.username,
                                Password = item.password,
                                ConnectionString = GetConnectionString(item.server, item.db, item.username, item.password)
                            }).ToList();
                    }
                    logger.LogInformation("Fethed GetAllCustomerDataBases() count:" + customerDatabases.Count());
                    memoryCache.Set("ALL_CUSTOMER_DATABASES", customerDatabases, GetCachePolicy());
                }
                return customerDatabases;
            }
            catch (Exception ex)
            {
                logger.LogError("GetAllDataBases() failed Exception: " + ex.ToString());
            }
            return null;
        }

        protected CustomerDatabase GetCustomerDatabaseById(string cid)
        {
            return GetAllCustomerDataBases().Where(e => e.CID.Equals(cid, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        protected string GetAdminDBConnectionString()
        {
            return configuration.GetValue<string>("ConnectionStringsConfig:AdminDBConnectionString");
        }

        protected string GetConnectionString(string serverName, string dataBase, string userName, string password)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder()
            {
                DataSource = serverName,
                InitialCatalog = dataBase,
                UserID = userName,
                Password = password
            };
            return builder.ConnectionString;
        }

        protected MemoryCacheEntryOptions GetCachePolicy()
        {
            int hours = configuration.GetValue<int>("ConnectionStringsConfig:CACHE_TIME_IN_HOURS");

            return new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(hours));
        }
    }
}
