using AutoMapper;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.SqlServer.Server;
using TRTA.WorkflowServices.Contracts.Constants;
using TRTA.WorkflowServices.Contracts.Dto;
using TRTA.WorkflowServices.Contracts.Enums;

namespace TRTA.WorkflowServices.ApplicationCore.AutoMapProfiles
{
    public class UsersProfile : Profile
    {
        public UsersProfile() 
        {
            CreateMap<UserAdminServiceUser, User>()
            .ForMember(des => des.Name, opt => opt.MapFrom(src => src.fullName))
            .ForMember(des => des.Email, opt => opt.MapFrom(src => src.emailAddress))
            .ForMember(des => des.Login, opt => opt.MapFrom(src => src.universalId))
            .ForMember(dest => dest.UserType, opt => opt.MapFrom((src, dest) =>
            {
                switch (src.userType)
                {
                    case "1": return UserAdminServiceConstants.UserTypeInternalUsers ;
                    case "2":  return UserAdminServiceConstants.UserTypeDataFlow;
                    case "3": return UserAdminServiceConstants.UserTypeAuditManager;                       
                }
                return UserAdminServiceConstants.UserTypeRegularUser;
            }));
        }
    }
}
