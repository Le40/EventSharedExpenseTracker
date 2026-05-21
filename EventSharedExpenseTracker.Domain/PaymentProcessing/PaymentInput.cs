using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSharedExpenseTracker.Domain.PaymentProcessing
{
    public class PaymentInput
    {
        public int ParticipantId { get; init; }
        public decimal? Amount { get; set; }
        public bool IsOwed { get; init; }
        public bool IsEquallyShared { get; set; }
    }
}
