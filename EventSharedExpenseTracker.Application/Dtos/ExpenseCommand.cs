using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSharedExpenseTracker.Application.Dtos
{
    public class ExpenseCommand
    {

        public int Id { get; set; }
        public int TripId { get; set; }
        public bool CanEdit { get; set; }

        public string Name { get; set; } = "";
        public DateTime Date { get; set; }
        public string Category { get; set; } = "";
        public string? Description { get; set; }

        public List<PaymentCommand> Payments { get; set; } = [];
    }

    public class PaymentCommand
    {
        public int Id { get; set; } // for updates, not needed for creates
        public int ParticipantId { get; set; }
        public decimal? Amount { get; set; }
        public bool IsOwed { get; set; }
        public bool IsEquallyShared { get; set; }
    }

    /*public int Id { get; set; }
    public int TripId { get; set; }
    public bool CanEdit { get; set; }

    public string Name { get; set; } = "";
    public DateTime Date { get; set; }
    public string Category { get; set; } = "";
    public string? Description { get; set; }

    public List<PayerInput> PaidBy { get; set; } = new();
    public List<OwerInput> OwedBy { get; set; } = new();
}

public class PayerInput
{
    public int PaymentId { get; set; } // for updates, not needed for creates
    public int ParticipantId { get; set; }
    public decimal Amount { get; set; }
}

public class OwerInput
{
    public int PaymentId { get; set; }
    public int ParticipantId { get; set; }
    public decimal? Amount { get; set; } = null;

    // optional: if null, calculate equal split
    public bool IsEquallyShared { get; set; } = true;*/
}

