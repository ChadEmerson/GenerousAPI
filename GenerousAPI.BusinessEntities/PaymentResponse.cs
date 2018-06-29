using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerousAPI.BusinessEntities
{
    public class PaymentResponse
    {
        public PaymentResponse()
        {

        }

        public bool IsSuccess { get; set; }
        public string AuthToken { get; set; }
        public string Message { get; set; }
        public decimal Amount { get; set; }
    }
}
