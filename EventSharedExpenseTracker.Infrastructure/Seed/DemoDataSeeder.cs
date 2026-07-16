using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Infrastructure.Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace EventSharedExpenseTracker.Infrastructure.Seed;

public class DemoDataOptions
{
    public int UserCount { get; set; } = 30;
    public int TripCount { get; set; } = 50;
    public int YearsBack { get; set; } = 3;

    // Number of ordinary expenses generated for every trip day.
    public int MinOtherExpensesPerDay { get; set; } = 2;
    public int MaxOtherExpensesPerDay { get; set; } = 4;

    // Longer trips can have their total accommodation split into two expenses.
    public int SplitAccommodationFromDays { get; set; } = 9;
    public double SplitAccommodationChance { get; set; } = 0.40;

    /// <summary>
    /// True:
    /// - all seeded trips use EUR as their base currency
    /// - all seeded expenses use EUR
    /// - template amounts are converted into approximate EUR values
    ///
    /// False:
    /// - each trip and expense uses its original template currency
    /// </summary>
    public bool UseEurOnlySeedData { get; set; } = true;
}

public class DemoDataSeeder
{
    private const string EurCurrencyCode = "EUR";

    private readonly ApplicationDbContext _context;
    private readonly Random _random = new();

    public DemoDataSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedBiDataAsync(DemoDataOptions? options = null)
    {
        options ??= new DemoDataOptions();

        ValidateOptions(options);

        // Uncomment this after testing if repeated demo seeding
        // should be prevented.
        //
        // if (await _context.Trips.AnyAsync(
        //         trip => trip.Name.StartsWith("Demo")))
        // {
        //     return;
        // }

        var users = await CreateCustomUsersAsync(options.UserCount);
        var templates = GetTripTemplates();

        for (var tripIndex = 0;
             tripIndex < options.TripCount;
             tripIndex++)
        {
            var template = Pick(templates);
            var creator = Pick(users);

            var trip = CreateTrip(
                template,
                creator.Id,
                tripIndex,
                options);

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            var participants = await AddParticipantsAsync(
                trip,
                users,
                creator.Id);

            AddAccommodationExpenses(
                template,
                trip,
                creator.Id,
                participants,
                options);

            AddOrdinaryExpenses(
                template,
                trip,
                creator.Id,
                participants,
                options);

            await _context.SaveChangesAsync();
        }
    }

    private static void ValidateOptions(DemoDataOptions options)
    {
        if (options.UserCount < 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options.UserCount),
                "At least two demo users are required.");
        }

        if (options.TripCount < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options.TripCount),
                "At least one demo trip is required.");
        }

        if (options.YearsBack < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options.YearsBack),
                "YearsBack must be at least one.");
        }

        if (options.MinOtherExpensesPerDay < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options.MinOtherExpensesPerDay));
        }

        if (options.MaxOtherExpensesPerDay <
            options.MinOtherExpensesPerDay)
        {
            throw new ArgumentException(
                "MaxOtherExpensesPerDay cannot be smaller " +
                "than MinOtherExpensesPerDay.");
        }

        if (options.SplitAccommodationChance is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options.SplitAccommodationChance),
                "The split chance must be between 0 and 1.");
        }
    }

    private async Task<List<CustomUser>> CreateCustomUsersAsync(
        int count)
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
            var existingUser = await _context.CustomUsers
                .FirstOrDefaultAsync(
                    user => user.CustomUserName == name);

            if (existingUser is not null)
            {
                users.Add(existingUser);
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
        var startDate = DateOnly
            .FromDateTime(DateTime.Today)
            .AddDays(
                -_random.Next(
                    1,
                    options.YearsBack * 365 + 1));

        var tripLengthDays = _random.Next(
            template.MinDays,
            template.MaxDays + 1);

        return new Trip
        {
            Name = $"Demo {template.Name} {index + 1}",

            DateFrom = startDate,

            // A three-day trip ends two days after its starting day.
            DateTo = startDate.AddDays(tripLengthDays - 1),

            CreatorId = creatorId,

            Category = template.Category,
            Country = template.CountryCode,
            City = template.City,

            BaseCurrencyCode = options.UseEurOnlySeedData
                ? EurCurrencyCode
                : template.CurrencyCode
        };
    }

    private async Task<List<TripParticipant>> AddParticipantsAsync(
        Trip trip,
        List<CustomUser> users,
        int creatorId)
    {
        var maximumParticipantCount = Math.Min(
            7,
            users.Count);

        var participantCount = _random.Next(
            2,
            maximumParticipantCount + 1);

        var creator = users.Single(
            user => user.Id == creatorId);

        var otherUsers = users
            .Where(user => user.Id != creatorId)
            .OrderBy(_ => _random.Next())
            .Take(participantCount - 1);

        var selectedUsers = new[]
            {
                creator
            }
            .Concat(otherUsers)
            .ToList();

        var participants = selectedUsers
            .Select(user => new TripParticipant
            {
                DisplayName = user.CustomUserName,
                TripId = trip.Id,
                UserId = user.Id
            })
            .ToList();

        _context.TripParticipants.AddRange(participants);
        await _context.SaveChangesAsync();

        return participants;
    }

    private void AddAccommodationExpenses(
        TripTemplate template,
        Trip trip,
        int creatorId,
        List<TripParticipant> participants,
        DemoDataOptions options)
    {
        var accommodationRule = template.ExpenseRules
            .Single(
                rule =>
                    rule.Category ==
                    ExpenseCategory.Accommodation);

        var nightCount = GetNightCount(trip);

        // Accommodation ranges in the templates represent
        // a nightly price in the template's native currency.
        var nightlyAmount = RandomAmount(
            accommodationRule.MinAmount,
            accommodationRule.MaxAmount);

        var totalAccommodationAmount = Math.Round(
            nightlyAmount * nightCount,
            2);

        var tripDayCount = GetTripDayCount(trip);

        var accommodationExpenseCount =
            tripDayCount >= options.SplitAccommodationFromDays &&
            _random.NextDouble() <
            options.SplitAccommodationChance
                ? 2
                : 1;

        var accommodationAmounts = SplitAmount(
            totalAccommodationAmount,
            accommodationExpenseCount);

        for (var index = 0;
             index < accommodationAmounts.Count;
             index++)
        {
            var accommodationDate = index == 0
                ? trip.DateFrom
                : trip.DateFrom.AddDays(tripDayCount / 2);

            var expense = CreateExpense(
                template,
                trip,
                creatorId,
                participants,
                options,
                accommodationRule,
                accommodationAmounts[index],
                accommodationDate);

            _context.Expenses.Add(expense);
        }
    }

    private void AddOrdinaryExpenses(
        TripTemplate template,
        Trip trip,
        int creatorId,
        List<TripParticipant> participants,
        DemoDataOptions options)
    {
        var tripDayCount = GetTripDayCount(trip);

        var minimumExpenseCount =
            tripDayCount *
            options.MinOtherExpensesPerDay;

        var maximumExpenseCount =
            tripDayCount *
            options.MaxOtherExpensesPerDay;

        var expenseCount = _random.Next(
            minimumExpenseCount,
            maximumExpenseCount + 1);

        // Accommodation is generated separately, so it cannot
        // appear repeatedly among ordinary expenses.
        var ordinaryRules = template.ExpenseRules
            .Where(
                rule =>
                    rule.Category !=
                    ExpenseCategory.Accommodation)
            .ToList();

        for (var expenseIndex = 0;
             expenseIndex < expenseCount;
             expenseIndex++)
        {
            var rule = PickWeighted(ordinaryRules);

            var expense = CreateExpense(
                template,
                trip,
                creatorId,
                participants,
                options,
                rule);

            _context.Expenses.Add(expense);
        }
    }

    private Expense CreateExpense(
        TripTemplate template,
        Trip trip,
        int creatorId,
        List<TripParticipant> participants,
        DemoDataOptions options,
        ExpenseRule rule,
        decimal? amountOverride = null,
        DateOnly? dateOverride = null)
    {
        var amountOriginal =
            amountOverride ??
            RandomAmount(
                rule.MinAmount,
                rule.MaxAmount);

        var expenseCurrencyCode = template.CurrencyCode;

        if (options.UseEurOnlySeedData)
        {
            amountOriginal = ConvertTemplateAmountToEur(
                amountOriginal,
                template);

            expenseCurrencyCode = EurCurrencyCode;
        }

        var expense = new Expense
        {
            TripId = trip.Id,
            CreatorId = creatorId,

            Name = GenerateExpenseName(rule.Category),

            Date = dateOverride ??
                   RandomDate(
                       trip.DateFrom,
                       trip.DateTo),

            Category = rule.Category,
            Description = null,

            CurrencyCode = expenseCurrencyCode,

            // In either seeding mode, the expense currency
            // equals the trip's base currency.
            ExchangeRateToBase = 1m
        };

        var payments = CreatePayments(
            amountOriginal,
            expense.ExchangeRateToBase,
            participants);

        var setPaymentsResult = expense.SetPayments(payments);

        if (!setPaymentsResult.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Could not attach generated payments to expense " +
                $"'{expense.Name}' with amount {amountOriginal} " +
                $"{expense.CurrencyCode}.");
        }

        return expense;
    }

    private static decimal ConvertTemplateAmountToEur(
        decimal amount,
        TripTemplate template)
    {
        return Math.Round(
            amount * template.AmountToEurMultiplier,
            2);
    }

    private List<Payment> CreatePayments(
        decimal amountOriginal,
        decimal exchangeRateToBase,
        List<TripParticipant> participants)
    {
        var payer = Pick(participants);

        var owedParticipants = participants
            .Where(
                _ => _random.NextDouble() > 0.15)
            .ToList();

        if (owedParticipants.Count == 0)
        {
            owedParticipants.Add(payer);
        }

        var amountBase = Math.Round(
            amountOriginal * exchangeRateToBase,
            2);

        var payments = new List<Payment>
        {
            new()
            {
                ParticipantId = payer.Id,

                AmountOriginal = amountOriginal,
                AmountBase = amountBase,

                IsOwed = false,
                IsEquallyShared = false
            }
        };

        var regularOriginalShare = Math.Round(
            amountOriginal / owedParticipants.Count,
            2);

        var regularBaseShare = Math.Round(
            amountBase / owedParticipants.Count,
            2);

        var allocatedOriginal = 0m;
        var allocatedBase = 0m;

        for (var index = 0;
             index < owedParticipants.Count;
             index++)
        {
            var isLastParticipant =
                index == owedParticipants.Count - 1;

            // The final participant receives any rounding remainder.
            // This guarantees that paid and owed totals balance exactly.
            var originalShare = isLastParticipant
                ? amountOriginal - allocatedOriginal
                : regularOriginalShare;

            var baseShare = isLastParticipant
                ? amountBase - allocatedBase
                : regularBaseShare;

            allocatedOriginal += originalShare;
            allocatedBase += baseShare;

            payments.Add(new Payment
            {
                ParticipantId =
                    owedParticipants[index].Id,

                AmountOriginal = -originalShare,
                AmountBase = -baseShare,

                IsOwed = true,
                IsEquallyShared = true
            });
        }

        return payments;
    }

    private ExpenseRule PickWeighted(
        List<ExpenseRule> rules)
    {
        var totalWeight = rules.Sum(
            rule => rule.Weight);

        var roll = _random.Next(
            1,
            totalWeight + 1);

        var runningWeight = 0;

        foreach (var rule in rules)
        {
            runningWeight += rule.Weight;

            if (roll <= runningWeight)
            {
                return rule;
            }
        }

        return rules.Last();
    }

    private decimal RandomAmount(
        decimal minimum,
        decimal maximum)
    {
        var value =
            minimum +
            (decimal)_random.NextDouble() *
            (maximum - minimum);

        return Math.Round(value, 2);
    }

    private DateOnly RandomDate(
        DateOnly from,
        DateOnly to)
    {
        var dayOffset = _random.Next(
            0,
            to.DayNumber - from.DayNumber + 1);

        return from.AddDays(dayOffset);
    }

    private static int GetTripDayCount(Trip trip)
    {
        return trip.DateTo.DayNumber
               - trip.DateFrom.DayNumber
               + 1;
    }

    private static int GetNightCount(Trip trip)
    {
        return Math.Max(
            1,
            trip.DateTo.DayNumber -
            trip.DateFrom.DayNumber);
    }

    private static List<decimal> SplitAmount(
        decimal totalAmount,
        int partCount)
    {
        if (partCount <= 1)
        {
            return [totalAmount];
        }

        var regularPart = Math.Round(
            totalAmount / partCount,
            2);

        var parts = new List<decimal>();
        var allocatedAmount = 0m;

        for (var index = 0;
             index < partCount;
             index++)
        {
            var isLastPart =
                index == partCount - 1;

            var amount = isLastPart
                ? totalAmount - allocatedAmount
                : regularPart;

            parts.Add(amount);
            allocatedAmount += amount;
        }

        return parts;
    }

    private T Pick<T>(
        IReadOnlyList<T> values)
    {
        return values[_random.Next(values.Count)];
    }

    private string GenerateExpenseName(
        ExpenseCategory category)
    {
        return category switch
        {
            ExpenseCategory.Restaurant =>
                Pick(
                [
                    "Dinner",
                    "Lunch",
                    "Local restaurant",
                    "Evening meal"
                ]),

            ExpenseCategory.Groceries =>
                Pick(
                [
                    "Groceries",
                    "Supermarket",
                    "Breakfast supplies"
                ]),

            ExpenseCategory.CoffeeSnacks =>
                Pick(
                [
                    "Coffee",
                    "Snacks",
                    "Bakery",
                    "Ice cream"
                ]),

            ExpenseCategory.Accommodation =>
                Pick(
                [
                    "Hotel",
                    "Apartment",
                    "Hostel",
                    "Guesthouse"
                ]),

            ExpenseCategory.PublicTransport =>
                Pick(
                [
                    "Metro tickets",
                    "Bus tickets",
                    "Train tickets"
                ]),

            ExpenseCategory.Fuel =>
                Pick(
                [
                    "Fuel",
                    "Gas station"
                ]),

            ExpenseCategory.TaxiRides =>
                Pick(
                [
                    "Taxi",
                    "Uber",
                    "Airport transfer"
                ]),

            ExpenseCategory.ParkingTolls =>
                Pick(
                [
                    "Parking",
                    "Highway tolls"
                ]),

            ExpenseCategory.Activities =>
                Pick(
                [
                    "Activity",
                    "Tour",
                    "Bike rental"
                ]),

            ExpenseCategory.Tickets =>
                Pick(
                [
                    "Museum tickets",
                    "Entrance tickets",
                    "Concert tickets"
                ]),

            ExpenseCategory.Shopping =>
                Pick(
                [
                    "Shopping",
                    "Souvenirs"
                ]),

            ExpenseCategory.HealthPharmacy =>
                Pick(
                [
                    "Pharmacy",
                    "Medicine"
                ]),

            ExpenseCategory.Fees =>
                Pick(
                [
                    "Service fee",
                    "ATM fee",
                    "Booking fee"
                ]),

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
                AmountToEurMultiplier = 0.04m,

                MinDays = 2,
                MaxDays = 4,

                ExpenseRules =
                [
                    // Accommodation amount is a nightly price.
                    new(
                        ExpenseCategory.Accommodation,
                        1500,
                        4000,
                        1),

                    new(
                        ExpenseCategory.Restaurant,
                        250,
                        1500,
                        5),

                    new(
                        ExpenseCategory.CoffeeSnacks,
                        60,
                        350,
                        4),

                    new(
                        ExpenseCategory.PublicTransport,
                        30,
                        500,
                        2),

                    new(
                        ExpenseCategory.Tickets,
                        150,
                        1000,
                        2),

                    new(
                        ExpenseCategory.Shopping,
                        200,
                        2500,
                        1)
                ]
            },

            new()
            {
                Name = "Vienna City Trip",
                Category = TripCategory.CityTrip,
                CountryCode = "AT",
                City = "Vienna",
                CurrencyCode = "EUR",
                AmountToEurMultiplier = 1m,

                MinDays = 2,
                MaxDays = 5,

                ExpenseRules =
                [
                    new(
                        ExpenseCategory.Accommodation,
                        70,
                        180,
                        1),

                    new(
                        ExpenseCategory.Restaurant,
                        15,
                        90,
                        5),

                    new(
                        ExpenseCategory.CoffeeSnacks,
                        3,
                        18,
                        4),

                    new(
                        ExpenseCategory.PublicTransport,
                        3,
                        30,
                        2),

                    new(
                        ExpenseCategory.Tickets,
                        10,
                        80,
                        2),

                    new(
                        ExpenseCategory.Shopping,
                        10,
                        150,
                        1)
                ]
            },

            new()
            {
                Name = "Tatras Hiking",
                Category = TripCategory.HikingNature,
                CountryCode = "SK",
                City = "Vysoké Tatry",
                CurrencyCode = "EUR",
                AmountToEurMultiplier = 1m,

                MinDays = 3,
                MaxDays = 7,

                ExpenseRules =
                [
                    new(
                        ExpenseCategory.Accommodation,
                        45,
                        120,
                        1),

                    new(
                        ExpenseCategory.Groceries,
                        10,
                        80,
                        4),

                    new(
                        ExpenseCategory.Restaurant,
                        12,
                        70,
                        3),

                    new(
                        ExpenseCategory.Fuel,
                        20,
                        100,
                        2),

                    new(
                        ExpenseCategory.ParkingTolls,
                        3,
                        25,
                        1),

                    new(
                        ExpenseCategory.Activities,
                        8,
                        60,
                        2)
                ]
            },

            new()
            {
                Name = "Croatia Beach Holiday",
                Category = TripCategory.BeachHoliday,
                CountryCode = "HR",
                City = "Split",
                CurrencyCode = "EUR",
                AmountToEurMultiplier = 1m,

                MinDays = 5,
                MaxDays = 12,

                ExpenseRules =
                [
                    new(
                        ExpenseCategory.Accommodation,
                        70,
                        200,
                        1),

                    new(
                        ExpenseCategory.Restaurant,
                        15,
                        100,
                        5),

                    new(
                        ExpenseCategory.Groceries,
                        10,
                        100,
                        3),

                    new(
                        ExpenseCategory.Fuel,
                        30,
                        160,
                        2),

                    new(
                        ExpenseCategory.Activities,
                        15,
                        120,
                        2),

                    new(
                        ExpenseCategory.CoffeeSnacks,
                        3,
                        20,
                        4)
                ]
            },

            new()
            {
                Name = "London Weekend",
                Category = TripCategory.CityTrip,
                CountryCode = "GB",
                City = "London",
                CurrencyCode = "GBP",
                AmountToEurMultiplier = 1.17m,

                MinDays = 3,
                MaxDays = 5,

                ExpenseRules =
                [
                    new(
                        ExpenseCategory.Accommodation,
                        100,
                        250,
                        1),

                    new(
                        ExpenseCategory.Restaurant,
                        15,
                        90,
                        5),

                    new(
                        ExpenseCategory.CoffeeSnacks,
                        3,
                        15,
                        4),

                    new(
                        ExpenseCategory.PublicTransport,
                        3,
                        30,
                        3),

                    new(
                        ExpenseCategory.Tickets,
                        10,
                        80,
                        2)
                ]
            },

            new()
            {
                Name = "Morocco Trip",
                Category = TripCategory.Backpacking,
                CountryCode = "MA",
                City = "Marrakesh",
                CurrencyCode = "MAD",
                AmountToEurMultiplier = 0.092m,

                MinDays = 5,
                MaxDays = 12,

                ExpenseRules =
                [
                    new(
                        ExpenseCategory.Accommodation,
                        300,
                        1000,
                        1),

                    new(
                        ExpenseCategory.Restaurant,
                        40,
                        300,
                        5),

                    new(
                        ExpenseCategory.CoffeeSnacks,
                        10,
                        80,
                        4),

                    new(
                        ExpenseCategory.TaxiRides,
                        20,
                        150,
                        2),

                    new(
                        ExpenseCategory.Activities,
                        80,
                        600,
                        2),

                    new(
                        ExpenseCategory.Shopping,
                        50,
                        500,
                        1)
                ]
            },

            new()
            {
                Name = "Thailand Holiday",
                Category = TripCategory.BeachHoliday,
                CountryCode = "TH",
                City = "Bangkok",
                CurrencyCode = "THB",
                AmountToEurMultiplier = 0.026m,

                MinDays = 7,
                MaxDays = 18,

                ExpenseRules =
                [
                    new(
                        ExpenseCategory.Accommodation,
                        900,
                        3000,
                        1),

                    new(
                        ExpenseCategory.Restaurant,
                        80,
                        600,
                        5),

                    new(
                        ExpenseCategory.CoffeeSnacks,
                        30,
                        180,
                        4),

                    new(
                        ExpenseCategory.TaxiRides,
                        60,
                        500,
                        2),

                    new(
                        ExpenseCategory.Activities,
                        200,
                        2000,
                        2),

                    new(
                        ExpenseCategory.Shopping,
                        150,
                        1500,
                        1)
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

        public string CurrencyCode { get; init; } =
            EurCurrencyCode;

        /// <summary>
        /// Fixed approximate conversion used only when producing
        /// EUR-only demo data.
        /// </summary>
        public decimal AmountToEurMultiplier { get; init; } = 1m;

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