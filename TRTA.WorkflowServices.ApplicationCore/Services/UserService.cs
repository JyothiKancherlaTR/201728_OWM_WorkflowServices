using AutoMapper;
using Microsoft.Extensions.Logging;
using TRTA.OSP.Authentication.Service.Interfaces;
using TRTA.WorkflowServices.Contracts.Dto;
using TRTA.WorkflowServices.ExternalServices;
using TRTA.WorkflowServices.Repository.Repositories;


namespace TRTA.WorkflowServices.ApplicationCore.Services
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(string tenantId, string universalId);
    }
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly ITokenService tokenService;
        private readonly IUserAdminservice userAdminService;
        private readonly IMapper mapper;
        private readonly ILogger<UserAdminservice> logger;

        public UserService(IUserRepository userRepository, ITokenService tokenService,IUserAdminservice userAdminService, IMapper mapper, ILogger<UserAdminservice> logger)
        {
            this.userRepository = userRepository;
            this.tokenService = tokenService;
            this.userAdminService = userAdminService;
            this.mapper = mapper;
            this.logger = logger;

        }
        public async Task<User> GetUserByIdAsync( string tenantId, string universalId)
        {
            try
            {
                var token = await tokenService.GetUDSLongToken();

                var user = await userRepository.GetUserAsync(tenantId);
                if (user == null)
                {
                    throw new Exception($"User details not found ");
                }

                var userDetails = await userAdminService.GetUserDetailsByLoginId(universalId, tenantId, token);
                return mapper.Map<User>(userDetails);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }                       
          
        }
    }
}
