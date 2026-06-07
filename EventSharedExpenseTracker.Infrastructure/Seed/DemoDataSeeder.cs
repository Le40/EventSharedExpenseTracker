using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Infrastructure.Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace EventSharedExpenseTracker.Infrastructure.Seed;

public class DemoDataOptions
{
    public int UserCount { get; set; } = 30;
    public int TripCount { get; set; } = 50;
    public int MinExpensesPerTrip { get; set; } = 10;
    public int MaxExpensesPerTrip { get; set; } = 80;
    public int YearsBack { get; set; } = 3;
}

public class DemoDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly Random _random = new();

    public DemoDataSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync(DemoDataOptions? options = null)
    {
        options ??= new DemoDataOptions();

        //if (await _context.Trips.AnyAsync(t => t.Name.StartsWith("Demo")))
        //    return;

        var users = await CreateCustomUsersAsync(options.UserCount);
        var templates = GetTripTemplates();

        for (int i = 0; i < options.TripCount; i++)
        {
            var template = Pick(templates);
            var creator = Pick(users);

            var trip = CreateTrip(template, creator.Id, i, options);

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            var participants = AddParticipants(trip, users);

            var expenseCount = _random.Next(
                options.MinExpensesPerTrip,
                options.MaxExpensesPerTrip + 1);

            for (int e = 0; e < expenseCount; e++)
            {
                var expense = CreateExpense(template, trip, creator.Id, participants);
                _context.Expenses.Add(expense);
            }

            await _context.SaveChangesAsync();
        }
    }

    private async Task<List<CustomUser>> CreateCustomUsersAsync(int count)
    {
        var names = new[]
        {
            "Peter", "Martin", "Lucia", "Jana", "Tomas",
            "Marek", "Zuzana", "Michal", "Katarina", "Adam",
            "Eva", "David", "Simona", "Filip", "Nina",
            "Roman", "Veronika", "Patrik", "Michaela", "Oliver",
            "Barbora", "Samuel", "Dominika", "Jakub", "Lenka",
            "Andrej", "Natalia", "Matej", "Sofia", "Daniel"
        };

        var users = new List<CustomUser>();

        foreach (var name in names.Take(count))
        {
            var existing = await _context.CustomUsers
                .FirstOrDefaultAsync(u => u.CustomUserName == name);

            if (existing != null)
            {
                users.Add(existing);
                continue;
            }

            var user = new CustomUser
            {
                CustomUserName = name
            };

            _context.CustomUsers.Add(user);
            users.Add(user);
        }

        await _context.SaveChangesAsync();

        return users;
    }

    private Trip CreateTrip(
        TripTemplate template,
        int creatorId,
        int index,
        DemoDataOptions options)
    {
        var startDate = DateOnly.FromDateTime(DateTime.Today)
            .AddDays(-_random.Next(1, options.YearsBack * 365));

        var duration = _random.Next(template.MinDays, template.MaxDays + 1);

        return new Trip
        {
            Name = $"Demo {template.Name} {index + 1}",
            DateFrom = startDate,
            DateTo = startDate.AddDays(duration),
            CreatorId = creatorId,

            Category = template.Category,
            Country = template.CountryCode,
            City = template.City,
            BaseCurrencyCode = template.CurrencyCode
        };
    }

    private List<TripParticipant> AddParticipants(
        Trip trip,
        List<CustomUser> users)
    {
        var count = _random.Next(2, Math.Min(7, users.Count + 1));

        var selectedUsers = users
            .OrderBy(_ => _random.Next())
            .Take(count)
            .ToList();

        var participants = selectedUsers
            .Select(u => new TripParticipant
            {
                DisplayName = u.CustomUserName,
                TripId = trip.Id,
                UserId = u.Id
            })
            .ToList();

        _context.TripParticipants.AddRange(participants);
        _context.SaveChanges();

        return participants;
    }

    private Expense CreateExpense(
        TripTemplate template,
        Trip trip,
        int creatorId,
        List<TripParticipant> participants)
    {
        var rule = PickWeighted(template.ExpenseRules);

        var amountOriginal = RandomAmount(rule.MinAmount, rule.MaxAmount);

        var expense = new Expense
        {
            TripId = trip.Id,
            CreatorId = creatorId,

            Name = GenerateExpenseName(rule.Category),
            Date = RandomDate(trip.DateFrom, trip.DateTo),
            Category = rule.Category,
            Description = null,

            CurrencyCode = template.CurrencyCode,
            //BaseCurrencyCode = trip.BaseCurrencyCode,
            ExchangeRateToBase = 1m
        };

        var payments = CreatePayments(
            amountOriginal,
            expense.ExchangeRateToBase,
            participants);

        expense.SetPayments(payments);

        return expense;
    }

    private List<Payment> CreatePayments(
        decimal amountOriginal,
        decimal exchangeRateToBase,
        List<TripParticipant> participants)
    {
        var payer = Pick(participants);

        var owedParticipants = participants
            .Where(_ => _random.NextDouble() > 0.15)
            .ToList();

        if (owedParticipants.Count == 0)
            owedParticipants.Add(payer);

        var shareOriginal = Math.Round(
            amountOriginal / owedParticipants.Count,
            2);

        var payments = new List<Payment>
        {
            new()
            {
                ParticipantId = payer.Id,
                AmountOriginal = amountOriginal,
                AmountBase = Math.Round(amountOriginal * exchangeRateToBase, 2),
                IsOwed = false,
                IsEquallyShared = false
            }
        };

        foreach (var participant in owedParticipants)
        {
            payments.Add(new Payment
            {
                ParticipantId = participant.Id,
                AmountOriginal = -shareOriginal,
                AmountBase = Math.Round(-shareOriginal * exchangeRateToBase, 2),
                IsOwed = true,
                IsEquallyShared = true
            });
        }

        return payments;
    }

    private ExpenseRule PickWeighted(List<ExpenseRule> rules)
    {
        var totalWeight = rules.Sum(r => r.Weight);
        var roll = _random.Next(1, totalWeight + 1);

        var running = 0;

        foreach (var rule in rules)
        {
            running += rule.Weight;

            if (roll <= running)
                return rule;
        }

        return rules.Last();
    }

    private decimal RandomAmount(decimal min, decimal max)
    {
        var value = min + (decimal)_random.NextDouble() * (max - min);
        return Math.Round(value, 2);
    }

    private DateOnly RandomDate(DateOnly from, DateOnly to)
    {
        var dayOffset = _random.Next(0, (to.DayNumber - from.DayNumber) + 1);
        return from.AddDays(dayOffset);
    }

    private T Pick<T>(IReadOnlyList<T> values)
    {
        return values[_random.Next(values.Count)];
    }

    private string GenerateExpenseName(ExpenseCategory category)
    {
        return category switch
        {
            ExpenseCategory.Restaurant => Pick(["Dinner", "Lunch", "Local restaurant", "Evening meal"]),
            ExpenseCategory.Groceries => Pick(["Groceries", "Supermarket", "Breakfast supplies"]),
            ExpenseCategory.CoffeeSnacks => Pick(["Coffee", "Snacks", "Bakery", "Ice cream"]),

            ExpenseCategory.Accommodation => Pick(["Hotel", "Apartment", "Hostel", "Guesthouse"]),

            ExpenseCategory.PublicTransport => Pick(["Metro tickets", "Bus tickets", "Train tickets"]),
            ExpenseCategory.Fuel => Pick(["Fuel", "Gas station"]),
            ExpenseCategory.TaxiRides => Pick(["Taxi", "Uber", "Airport transfer"]),
            ExpenseCategory.ParkingTolls => Pick(["Parking", "Highway tolls"]),

            ExpenseCategory.Activities => Pick(["Activity", "Tour", "Bike rental"]),
            ExpenseCategory.Tickets => Pick(["Museum tickets", "Entrance tickets", "Concert tickets"]),

            ExpenseCategory.Shopping => Pick(["Shopping", "Souvenirs"]),
            ExpenseCategory.HealthPharmacy => Pick(["Pharmacy", "Medicine"]),
            ExpenseCategory.Fees => Pick(["Service fee", "ATM fee", "Booking fee"]),

            _ => "Other expense"
        };
    }

    private List<TripTemplate> GetTripTemplates()
    {
        return
        [
            new()
            {
                Name = "Prague Weekend",
                Category = TripCategory.CityTrip,
                CountryCode = "CZ",
                City = "Prague",
                CurrencyCode = "CZK",
                MinDays = 2,
                MaxDays = 4,
                ExpenseRules =
                [
                    new(ExpenseCategory.Accommodation, 1500, 7000, 2),
                    new(ExpenseCategory.Restaurant, 300, 1800, 5),
                    new(ExpenseCategory.CoffeeSnacks, 80, 400, 4),
                    new(ExpenseCategory.PublicTransport, 100, 700, 2),
                    new(ExpenseCategory.Tickets, 200, 1200, 2),
                    new(ExpenseCategory.Shopping, 300, 2500, 1)
                ]
            },

            new()
            {
                Name = "Vienna City Trip",
                Category = TripCategory.CityTrip,
                CountryCode = "AT",
                City = "Vienna",
                CurrencyCode = "EUR",
                MinDays = 2,
                MaxDays = 5,
                ExpenseRules =
                [
                    new(ExpenseCategory.Accommodation, 80, 500, 2),
                    new(ExpenseCategory.Restaurant, 20, 150, 5),
                    new(ExpenseCategory.CoffeeSnacks, 5, 30, 4),
                    new(ExpenseCategory.PublicTransport, 5, 60, 2),
                    new(ExpenseCategory.Tickets, 15, 120, 2),
                    new(ExpenseCategory.Shopping, 20, 200, 1)
                ]
            },

            new()
            {
                Name = "Tatras Hiking",
                Category = TripCategory.HikingNature,
                CountryCode = "SK",
                City = "Vysoké Tatry",
                CurrencyCode = "EUR",
                MinDays = 3,
                MaxDays = 7,
                ExpenseRules =
                [
                    new(ExpenseCategory.Accommodation, 60, 400, 2),
                    new(ExpenseCategory.Groceries, 20, 120, 4),
                    new(ExpenseCategory.Restaurant, 15, 100, 3),
                    new(ExpenseCategory.Fuel, 30, 160, 2),
                    new(ExpenseCategory.ParkingTolls, 5, 40, 1),
                    new(ExpenseCategory.Activities, 10, 100, 2)
                ]
            },

            new()
            {
                Name = "Croatia Beach Holiday",
                Category = TripCategory.BeachHoliday,
                CountryCode = "HR",
                City = "Split",
                CurrencyCode = "EUR",
                MinDays = 5,
                MaxDays = 12,
                ExpenseRules =
                [
                    new(ExpenseCategory.Accommodation, 300, 1400, 2),
                    new(ExpenseCategory.Restaurant, 25, 180, 5),
                    new(ExpenseCategory.Groceries, 20, 180, 3),
                    new(ExpenseCategory.Fuel, 50, 250, 2),
                    new(ExpenseCategory.Activities, 20, 200, 2),
                    new(ExpenseCategory.CoffeeSnacks, 5, 40, 4)
                ]
            },

            new()
            {
                Name = "London Weekend",
                Category = TripCategory.CityTrip,
                CountryCode = "GB",
                City = "London",
                CurrencyCode = "GBP",
                MinDays = 3,
                MaxDays = 5,
                ExpenseRules =
                [
                    new(ExpenseCategory.Accommodation, 120, 700, 2),
                    new(ExpenseCategory.Restaurant, 20, 160, 5),
                    new(ExpenseCategory.CoffeeSnacks, 4, 35, 4),
                    new(ExpenseCategory.PublicTransport, 8, 80, 3),
                    new(ExpenseCategory.Tickets, 15, 150, 2)
                ]
            },

            new()
            {
                Name = "Morocco Trip",
                Category = TripCategory.Backpacking,
                CountryCode = "MA",
                City = "Marrakesh",
                CurrencyCode = "MAD",
                MinDays = 5,
                MaxDays = 12,
                ExpenseRules =
                [
                    new(ExpenseCategory.Accommodation, 250, 1800, 2),
                    new(ExpenseCategory.Restaurant, 60, 450, 5),
                    new(ExpenseCategory.CoffeeSnacks, 15, 120, 4),
                    new(ExpenseCategory.TaxiRides, 30, 250, 2),
                    new(ExpenseCategory.Activities, 100, 900, 2),
                    new(ExpenseCategory.Shopping, 80, 700, 1)
                ]
            },

            new()
            {
                Name = "Thailand Holiday",
                Category = TripCategory.BeachHoliday,
                CountryCode = "TH",
                City = "Bangkok",
                CurrencyCode = "THB",
                MinDays = 7,
                MaxDays = 18,
                ExpenseRules =
                [
                    new(ExpenseCategory.Accommodation, 900, 8000, 2),
                    new(ExpenseCategory.Restaurant, 150, 1200, 5),
                    new(ExpenseCategory.CoffeeSnacks, 40, 300, 4),
                    new(ExpenseCategory.TaxiRides, 100, 800, 2),
                    new(ExpenseCategory.Activities, 400, 3000, 2),
                    new(ExpenseCategory.Shopping, 300, 2500, 1)
                ]
            }
        ];
    }

    private record TripTemplate
    {
        public string Name { get; init; } = "";
        public TripCategory Category { get; init; }
        public string CountryCode { get; init; } = "";
        public string City { get; init; } = "";
        public string CurrencyCode { get; init; } = "EUR";
        public int MinDays { get; init; }
        public int MaxDays { get; init; }
        public List<ExpenseRule> ExpenseRules { get; init; } = [];
    }

    private record ExpenseRule(
        ExpenseCategory Category,
        decimal MinAmount,
        decimal MaxAmount,
        int Weight);
}