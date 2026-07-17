
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Infrastructure.Data.DbContexts;
using EventSharedExpenseTracker.Tests.Factories;
using EventSharedExpenseTracker.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace EventSharedExpenseTracker.Tests.Integration;

public class ExpenseTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly FormTestHelper _formHelper;
    private readonly ITestOutputHelper _output;
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    public ExpenseTests(ITestOutputHelper output)
    {
        _output = output;
        _factory = new CustomWebApplicationFactory(true);

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        _formHelper = new FormTestHelper(_client, _cancellationToken);
    }

    [Fact]
    public async Task Expense_Create_Works()
    {
        using var scope = _factory.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<TestDataSeeder>();

        var seed = await seeder.SeedTripWithParticipantsAsync(_cancellationToken);

        var response = await _formHelper.PostFormAsync($"/Trips/{seed.TripId}/Expenses/Add/", new()
        {
            ["Name"] = "Dinner",
            ["Date"] = DateTime.Today.ToString("yyyy-MM-dd"),
            ["Category"] = "Restaurant",

            ["Participants[0].ParticipantId"] = seed.UserParticipantId.ToString(),
            ["Participants[0].ParticipantName"] = "testuser",
            ["Participants[0].PaidAmount"] = "20",
            ["Participants[0].IsOwedSelected"] = "true",
            ["Participants[0].OwedAmount"] = "10",

            ["Participants[1].ParticipantId"] = seed.DummyParticipantId.ToString(),
            ["Participants[1].ParticipantName"] = "dummy",
            ["Participants[1].PaidAmount"] = "",
            ["Participants[1].IsOwedSelected"] = "true",
            ["Participants[1].OwedAmount"] = "10"
        });

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.OK);

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        db.Expenses.Any(e => e.Name == "Dinner").Should().BeTrue();
    }

    [Fact]
    public async Task Expense_Create_WhenValidationFails_ReturnsForm()
    {
        using var scope = _factory.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<TestDataSeeder>();

        var seed = await seeder.SeedTripWithParticipantsAsync(_cancellationToken);

        var form = await _formHelper.GetFormFieldsAsync($"/Trips/{seed.TripId}/Expenses/Add");

        form["Name"] = ""; // force validation error
        form["Date"] = DateTime.Today.ToString("yyyy-MM-dd");
        form["Category"] = "Food";

        var response = await _formHelper.PostFormAsync(
            $"/Trips/{seed}/Expenses/Add",
            form);

        var html = await response.Content.ReadAsStringAsync(_cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        html.Should().Contain("hx-post");
        html.Should().Contain("Name is required");
    }

    [Fact]
    public async Task Expense_Update_Works()
    {
        using var scope = _factory.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<TestDataSeeder>();

        var seed = await seeder.SeedExpenseAsync(_cancellationToken);

        var response = await _formHelper.PostFormAsync($"/Expenses/Edit/{seed.ExpenseId}", new()
        {
            ["Id"] = seed.ExpenseId.ToString(),
            ["TripId"] = seed.TripId.ToString(),
            ["CanUserEdit"] = "true",

            ["Name"] = "Updated Dinner",
            ["Date"] = DateTime.Today.ToString("yyyy-MM-dd"),
            ["Category"] = "Restaurant",

            ["Participants[0].ParticipantId"] = seed.UserParticipantId.ToString(),
            ["Participants[0].ParticipantName"] = "testuser",
            ["Participants[0].PaidAmount"] = "30",
            ["Participants[0].IsOwedSelected"] = "true",
            ["Participants[0].OwedAmount"] = "15",

            ["Participants[1].ParticipantId"] = seed.DummyParticipantId.ToString(),
            ["Participants[1].ParticipantName"] = "dummy",
            ["Participants[1].PaidAmount"] = "",
            ["Participants[1].IsOwedSelected"] = "true",
            ["Participants[1].OwedAmount"] = "15"
        });

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.OK);

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Expenses.Any(e => e.Id == seed.ExpenseId && e.Name == "Updated Dinner")
            .Should().BeTrue();
    }

    [Fact]
    public async Task Expense_Delete_Works()
    {
        using var scope = _factory.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<TestDataSeeder>();

        var seed = await seeder.SeedExpenseAsync(_cancellationToken);

        var response = await _formHelper.PostFormAsync($"/Expenses/Delete/{seed.ExpenseId}", new()
        {
            ["id"] = seed.ExpenseId.ToString(),
            ["tripId"] = seed.TripId.ToString()
        });

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.OK);

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Expenses.Any(e => e.Id == seed.ExpenseId).Should().BeFalse();
    }

    [Fact]
    public async Task Expense_Edit_WhenExpenseBelongsToAnotherUsersTrip_ReturnsForbidden()
    {
        int foreignExpenseId;

        using (var scope = _factory.Services.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<TestDataSeeder>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Test authentication always represents user ID 1.
            var currentUser = await seeder.SeedAuthenticatedUserAsync(
                _cancellationToken);

            var otherUser = await seeder.SeedUserAsync(
                "other-user",
                _cancellationToken);

            // Trip A belongs to the authenticated user.
            var tripA = new Trip
            {
                Name = "Trip A",
                DateFrom = DateOnly.FromDateTime(DateTime.Today),
                DateTo = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                CreatorId = currentUser.Id
            };

            tripA.Participants.Add(new TripParticipant
            {
                UserId = currentUser.Id,
                DisplayName = currentUser.CustomUserName
            });

            // Trip B belongs only to another user.
            var tripB = new Trip
            {
                Name = "Trip B",
                DateFrom = DateOnly.FromDateTime(DateTime.Today),
                DateTo = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                CreatorId = otherUser.Id
            };

            var otherParticipant = new TripParticipant
            {
                UserId = otherUser.Id,
                DisplayName = otherUser.CustomUserName
            };

            tripB.Participants.Add(otherParticipant);

            var foreignExpense = new Expense
            {
                Name = "Private dinner",
                Date = DateOnly.FromDateTime(DateTime.Today),
                Category = ExpenseCategory.Restaurant,
                CreatorId = otherUser.Id,
                CurrencyCode = "EUR",
                ExchangeRateToBase = 1m
            };

            foreignExpense.Payments.Add(new Payment
            {
                Participant = otherParticipant,
                AmountOriginal = 20m,
                AmountBase = 20m,
                IsOwed = false
            });

            foreignExpense.Payments.Add(new Payment
            {
                Participant = otherParticipant,
                AmountOriginal = -20m,
                AmountBase = -20m,
                IsOwed = true
            });

            tripB.Expenses.Add(foreignExpense);

            db.Trips.AddRange(tripA, tripB);
            await db.SaveChangesAsync(_cancellationToken);

            foreignExpenseId = foreignExpense.Id;
        }

        // Authenticated user from Trip A tries to open an expense from Trip B.
        var response = await _client.GetAsync(
            $"/Expenses/Edit/{foreignExpenseId}",
            _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}