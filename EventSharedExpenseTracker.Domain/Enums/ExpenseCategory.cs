
using System.ComponentModel.DataAnnotations;

namespace EventSharedExpenseTracker.Domain.Enums
{
    public enum ExpenseCategory
    {
        Restaurant,
        Groceries,
        [Display(Name = "Coffee / Snacks")]
        CoffeeSnacks,

        [Display(Name = "Public Transport")]
        PublicTransport,
        Activities,
        Tickets,
        Other,

        [Display(Name = "Car Rental")]
        CarRental,
        Fuel,
        [Display(Name = "Taxi Rides")]
        TaxiRides,
        [Display(Name = "Parking Tolls")]
        ParkingTolls,

        Flights,
        Accommodation,
        Shopping,
        [Display(Name = "Health & Pharmacy")]
        HealthPharmacy,
        Fees
    }
}

