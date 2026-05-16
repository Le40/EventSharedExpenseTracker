using EventSharedExpenseTracker.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSharedExpenseTracker.Application.Dtos
{
    public class TripQuery
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateTo { get; set; }
        public string? ImagePath { get; set; }

        public List<string> ParticipantNames { get; set; } = [];
    }

    public class TripQueryParticipant
    {
        public int Id { get; set; }
        public string UserName { get; set; }
    }
}
