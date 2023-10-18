using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TRTA.WorkflowServices.ApplicationCore.Services;
using TRTA.WorkflowServices.Contracts.Dto;
using Microsoft.AspNetCore.Http;
using TRTA.OSP.OrganizationManagement.API.Extensions;
using TRTA.OSP.Authentication.Service.Constants;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using System.Data;

namespace TRTA.WorkflowServices.API.Controllers
{
    [ApiController]
    [Route("[controller]")]    
    public class UsersController : ControllerBase
    { 
        private readonly ILogger<UsersController> logger;
        private readonly IUserService userService;
        public UsersController(ILogger<UsersController> logger, IUserService UserService)
        {
            this.logger = logger;
            this.userService = UserService;
        }       

        [HttpGet("v1/users")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(typeof(ProducesErrorResponseTypeAttribute), 400)]
        [ProducesResponseType(typeof(ProducesErrorResponseTypeAttribute), 401)]
        [ProducesResponseType(typeof(ProducesErrorResponseTypeAttribute), 403)]
        [ProducesResponseType(typeof(ProducesErrorResponseTypeAttribute), 500)]
        public async Task<IActionResult> GetUser([FromQuery]string universalId)
        {
            try
            {                
                string tenant = HttpContext.GetTenantCode();

                return Ok(await userService.GetUserByIdAsync(tenant, universalId));
            }
            catch(ArgumentException ex)
            {
                logger.LogError(ex,ex.Message,ex.StackTrace);
                throw new Exception(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogError(ex, ex.Message);
                return Unauthorized("Invalid authorization.");
            }
        }
    }
}