using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerousAPI.BusinessEntities
{
    public class BankVerificationResponse
    {
        public BankVerificationResponse()
        {

        }

        public bool IsSuccess { get; set; }
        public string AuthToken { get; set; }
        public string Message { get; set; }

        public List<decimal> VerificationAmounts { get; set; }

        public Nullable<System.DateTime> BankVerificationRequestedOn { get; set; }
    }
}
