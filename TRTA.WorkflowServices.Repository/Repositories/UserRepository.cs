using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Dapper;
using TRTA.WorkflowServices.Contracts.Dto;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TRTA.WorkflowServices.Repository.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserAsync(string tenantId);
    }
    public class UserRepository : RepositoryBase, IUserRepository
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<UserRepository> logger;
        private readonly IMemoryCache memoryCache;

        public UserRepository(IConfiguration configuration, ILogger<UserRepository> logger, IMemoryCache memoryCache) 
            : base(configuration, logger, memoryCache)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.memoryCache = memoryCache;            
        }

        public async Task<User> GetUserAsync(string tenantId)
        {
            try
            {
                using (var connection = new SqlConnection(GetCustomerDatabaseById(tenantId).ConnectionString))
                {
                    var sql = @"select uid As Uid,
                            login As Login,
                            full_name As nmae,
                            email_address As Email
                            from usrs ";

                    var result = await connection.QueryAsync<User>(sql);
                    return result.Count() <= 0 ? null : result.First();
                }           
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
