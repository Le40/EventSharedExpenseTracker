using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSharedExpenseTracker.Application.Trips.DTOs
{
    public class TripQueryOptions
    {
        public string? SearchString { get; set; }
        public string? SortBy { get; set; }
        public string? Category { get; set; }
    }
}
