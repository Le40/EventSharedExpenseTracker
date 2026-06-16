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
            [Display(Name = "Road Trip")]
            RoadTrip,
            [Display(Name = "Ski Trip")]
            SkiTrip,
            [Display(Name = "Business Trip")]
            BusinessTrip,
            [Display(Name = "Festival / Event")]
            FestivalEvent,
            [Display(Name = "Action Packed")]
            ActionPacked,
            Backpacking,
            Other
        }

}
