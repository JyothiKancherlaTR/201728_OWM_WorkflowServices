using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRTA.WorkflowServices.Contracts.Dto
{
    public class UserAdminServiceUser
    {
        public string universalId { get; set; }
        public string password { get; set; }
        public string fullName { get; set; }
        public string emailAddress { get; set; }
        public List<UserLocation> locations { get; set; }
        public string dataProvider { get; set; }
        public bool userMustChangePassword { get; set; }
        public bool accountExpires { get; set; }
        public DateTime accountExpirationDate { get; set; }
        public bool isDisabled { get; set; }
        public bool isSharedUser { get; set; }
        public int passwordChangedInterval { get; set; }
        public bool isSysAdmin { get; set; }
        public object lockedOutDateTime { get; set; }
        public string userType { get; set; }
        public object userTypeString { get; set; }
        public bool isDirty { get; set; }
        public bool isDataFlowExternalUser { get; set; }
        public bool isSelfRegisterUser { get; set; }
        public bool isShortLoginRequired { get; set; }
        public DateTime lastPasswordChangeDate { get; set; }
        public bool isLoginUpdateRequired { get; set; }
        public string namedIdentifier { get; set; }
        public bool wasUserDisabledBeforeThisUpdate { get; set; }
        public bool hasPasswordUpdatedAsPartOfThisUpdate { get; set; }
        public string comments { get; set; }
        public object notes { get; set; }
        public string userId { get; set; }
        public bool isConcertOriginated { get; set; }
        public string userStatus { get; set; }
    }

    public class UserLocation
    {
        public int Id { get; set; }

        public string Name { get; set; }

    }

}
