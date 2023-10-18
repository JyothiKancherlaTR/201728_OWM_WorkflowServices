using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRTA.WorkflowServices.Repository.Contracts
{
    public class CustomerDatabase
    {
        public string CID { get; set; }

        public string DB { get; set; }

        public string Server { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string ConnectionString { get; set; }
    }
}
