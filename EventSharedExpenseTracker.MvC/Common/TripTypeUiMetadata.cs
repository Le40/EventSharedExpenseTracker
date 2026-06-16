using EventSharedExpenseTracker.Domain.Enums;

namespace EventSharedExpenseTracker.MvC.Common
{

    public record TripCategoryUiInfo(
        string Color,
        string DefaultImagePath
    );

    public static class TripCategoryUiMetadata
    {
        public static readonly Dictionary<TripCategory, TripCategoryUiInfo> Categories =
            new()
            {
                [TripCategory.CityTrip] = new("#8D6E63", "~/images/city.jpg"),                  // Stone / old buildings
                [TripCategory.BeachHoliday] = new("#2EC4B6", "~/images/beach.jpg"),             // turquoise sea
                [TripCategory.HikingNature] = new("#6A994E", "~/images/hiking.jpg"),            // forest green
                [TripCategory.RoadTrip] = new("#D4A017", "~/images/road.jpg"),                  // Mustard yellow
                [TripCategory.SkiTrip] = new("#A8DADC", "~/images/ski.jpg"),                    // icy blue
                [TripCategory.BusinessTrip] = new("#6C757D", "~/images/bussiness.jpg"),         // professional gray
                [TripCategory.FestivalEvent] = new("#C77DFF", "~/images/festival.jpg"),         // vibrant purple
                [TripCategory.ActionPacked] = new("#495057", "~/images/action-packed.jpg"),     // Soft Anthracite
                [TripCategory.Backpacking] = new("#BC6C25", "~/images/backpacking.jpg"),        // leather brown
                [TripCategory.Other] = new("#ADB5BD", "~/images/travel2.jpg")                   // neutral gray
            };
    }
}
