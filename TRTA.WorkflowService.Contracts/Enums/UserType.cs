using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRTA.WorkflowServices.Contracts.Enums
{
    public enum UserType
    {
            /// <summary>
            /// Regular user.
            /// </summary>
            RegularUser,
            /// <summary>
            /// DataFlow data provider.
            /// </summary>
            DataFlowProvider,
            /// <summary>
            /// AuditManager data provider.
            /// </summary>
            AuditManagerProvider
    }
    
}
