using System.ComponentModel.DataAnnotations;

namespace EventSharedExpenseTracker.Domain.Enums
{
        public enum TripCategory
        {
            [Display(Name = "City Trip")]
            CityTrip,
            [Display(Name = "Beach Holiday")]
            BeachHoliday,
            [Display(Name = "Nature / Hiking")]
            HikingNature,
            [Display(Name = "Read Trip")]
            RoadTrip,
            [Display(Name = "Ski Trip")]
            SkiTrip,
            [Display(Name = "Business Trip")]
            BusinessTrip,
            [Display(Name = "Festival / Event")]
            FestivalEvent,
            [Display(Name = "Family Visit")]
            FamilyVisit,
            Backpacking,
            Other
        }

}
