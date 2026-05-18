using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSharedExpenseTracker.Application.Dtos
{
    public class TripDetailsQuery
    {
        public int Id { get; set; }

        public bool CanEdit { get; set; }

        public string Name { get; set; } = "";

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public string? ImagePath { get; set; }

        public List<TripDetailsQueryarticipant> Participants { get; set; } = [];

        public List<ExpenseQuery> Expenses { get; set; } = [];
    }

    public class TripQueryParticipant
    {
        public int Id { get; set; }
        public string UserName { get; set; }
    }

    public class TripDetailsQueryarticipant : TripQueryParticipant
    {
        public decimal PaymentSum { get; set; }

        public int PaymentCount { get; set; }
    }
}
