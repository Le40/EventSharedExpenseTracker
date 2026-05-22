using AngleSharp.Html.Parser;
using Azure;
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Infrastructure.Data.DbContexts;
using EventSharedExpenseTracker.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;

namespace EventSharedExpenseTracker.Tests.Integration;

public class BaseAppFlowTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public BaseAppFlowTests(ITestOutputHelper output)
    {
        _output = output;
        _factory = new CustomWebApplicationFactory(true);

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Trip_Index_Works()
    {
        var response = await _client.GetAsync("/Trips", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Trip_Create_Get_Works()
    {
        var response = await _client.GetAsync("/Trips/Create", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Trip_Details_Works()
    {
        var tripId = await SeedTripAsync();

        var response = await _client.GetAsync($"/Trips/Details/{tripId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Trip_Edit_Get_Works()
    {
        var tripId = await SeedTripAsync();

        var response = await _client.GetAsync($"/Trips/Edit/{tripId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Expense_Create_Update_Delete_Flow_Works()
    {
        var tripId = await SeedTripWithParticipantsAsync();

        var createResponse = await PostFormAsync($"/Trips/{tripId}/Expenses/Add/", new Dictionary<string, string>
        {
            ["Name"] = "Dinner",
            ["Date"] = DateTime.Today.ToString("yyyy-MM-dd"),
            ["Category"] = "Food",

            ["Participants[0].ParticipantId"] = "1",
            ["Participants[0].ParticipantName"] = "testuser",
            ["Participants[0].PaidAmount"] = "20",
            ["Participants[0].IsOwedSelected"] = "true",
            ["Participants[0].OwedAmount"] = "10",

            ["Participants[1].ParticipantId"] = "2",
            ["Participants[1].ParticipantName"] = "dummy",
            ["Participants[1].PaidAmount"] = "",
            ["Participants[1].IsOwedSelected"] = "true",
            ["Participants[1].OwedAmount"] = "10"
        });
        await PrintResponseAsync("CREATE", createResponse);
        createResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.OK);

        var expenseId = GetFirstExpenseId();

        var updateResponse = await PostFormAsync($"/Expenses/Edit/{expenseId}", new Dictionary<string, string>
        {
            ["Id"] = expenseId.ToString(),
            ["TripId"] = tripId.ToString(),
            ["CanUserEdit"] = "true",
            ["Name"] = "Updated Dinner",
            ["Date"] = DateTime.Today.ToString("yyyy-MM-dd"),
            ["Category"] = "Food",

            ["Participants[0].ParticipantId"] = "1",
            ["Participants[0].PaidAmount"] = "30",
            ["Participants[0].IsOwedSelected"] = "true",
            ["Participants[0].OwedAmount"] = "15",

            ["Participants[1].ParticipantId"] = "2",
            ["Participants[1].PaidAmount"] = "",
            ["Participants[1].IsOwedSelected"] = "true",
            ["Participants[1].OwedAmount"] = "15"
        });
        await PrintResponseAsync("UPDATE", updateResponse);
        updateResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.OK);

        var deleteResponse = await PostFormAsync($"/Expenses/Delete/{expenseId}", new()
            {
            ["id"] = expenseId.ToString(),
            ["tripId"] = tripId.ToString()
        });

        await PrintResponseAsync("DELETE", deleteResponse);
        deleteResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task Trip_Delete_Works()
    {
        var id = await SeedTripAsync();

        var response = await PostFormAsync($"/Trips/Delete/{id}", new()
        {
            ["id"] = id.ToString(),
            ["Id"] = id.ToString()
        });

        await PrintResponseAsync("DELETE", response);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task Expense_Create_WhenValidationFails_AfterRealGet_RetainsParticipants()
    {
        var seed = await SeedTripWithParticipantsAsync();

        var form = await GetFormFieldsAsync($"/Trips/{seed}/Expenses/Add");

        form["Name"] = ""; // force validation error
        form["Date"] = DateTime.Today.ToString("yyyy-MM-dd");
        form["Category"] = "Food";

        var response = await PostFormAsync(
            $"/Trips/{seed}/Expenses/Add",
            form);

        var html = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        html.Should().Contain("hx-post");
        html.Should().Contain("dummy");
    }


    private async Task<Dictionary<string, string>> GetFormFieldsAsync(string url)
    {
        var response = await _client.GetAsync(url);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();

        var parser = new HtmlParser();
        var document = await parser.ParseDocumentAsync(html);

        var fields = new Dictionary<string, string>();

        foreach (var input in document.QuerySelectorAll("input, select, textarea"))
        {
            var name = input.GetAttribute("name");

            if (string.IsNullOrWhiteSpace(name))
                continue;

            var value = input.GetAttribute("value") ?? "";

            if (input.NodeName.Equals("select", StringComparison.OrdinalIgnoreCase))
            {
                var selected = input.QuerySelector("option[selected]");
                value = selected?.GetAttribute("value") ?? "";
            }

            fields[name] = value;
        }

        return fields;
    }

    private int GetFirstExpenseId()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return db.Expenses
            .OrderByDescending(e => e.Id)
            .Select(e => e.Id)
            .First();
    }

    private async Task<HttpResponseMessage> PostFormAsync(string url, Dictionary<string, string> formData)
    {
        using var content = new FormUrlEncodedContent(formData);

        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        return await _client.PostAsync(url, content);
    }

    private async Task<int> SeedTripWithParticipantsAsync()
    {
        await SeedUserAsync();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var trip = new Trip
        {
            Name = "Test Trip",
            DateFrom = DateTime.Today,
            DateTo = DateTime.Today.AddDays(1),
            CreatorId = 1,
        };
        trip.Participants.Add(new TripParticipant
        {
            Id = 1,
            UserId = 1,
            DisplayName = "testuser"
        });
        trip.Participants.Add(new TripParticipant
        {
            Id = 2,
            DisplayName = "dummy"
        });

        db.Trips.Add(trip);
        await db.SaveChangesAsync();

        return trip.Id;
    }

    private async Task SeedUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (await db.CustomUsers.AnyAsync(u => u.Id == 1))
            return;

        db.CustomUsers.Add(new CustomUser
        {
            Id = 1,
            CustomUserName = "testuser"
        });

        await db.SaveChangesAsync();
    }

    private async Task<int> SeedTripAsync()
    {
        await SeedUserAsync();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var trip = new Trip
        {
            Name = "Test Trip",
            DateFrom = DateTime.Today,
            DateTo = DateTime.Today.AddDays(1),
            CreatorId = 1
        };

        trip.Participants.Add(new TripParticipant
        {
            UserId = 1,
            DisplayName  = "testuser"
        });

        db.Trips.Add(trip);

        await db.SaveChangesAsync();

        return trip.Id;
    }
    private async Task PrintResponseAsync(string label, HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"--- {label} ---");
        _output.WriteLine($"STATUS: {response.StatusCode}");
        _output.WriteLine($"LOCATION: {response.Headers.Location}");
        _output.WriteLine("BODY:");
        _output.WriteLine(body);
    }

    private async Task<(int ExpenseId, int TripId, int UserParticipantId, int DummyParticipantId)>
    SeedExpenseAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var trip = new Trip
        {
            Name = "Test Trip",
            DateFrom = DateTime.Today,
            DateTo = DateTime.Today.AddDays(1),
            CreatorId = 1
        };

        var userParticipant = new TripParticipant
        {
            UserId = 1,
            DisplayName = "testuser"
        };

        var dummyParticipant = new TripParticipant
        {
            DisplayName = "dummy"
        };

        trip.Participants.Add(userParticipant);
        trip.Participants.Add(dummyParticipant);

        var expense = new Expense
        {
            Name = "Dinner",
            Date = DateTime.Today,
            Category = ExpenseCategory.Food,
            CreatorId = 1
        };

        expense.Payments.Add(new Payment
        {
            Participant = userParticipant,
            Amount = 20
        });

        expense.Payments.Add(new Payment
        {
            Participant = userParticipant,
            Amount = -10
        });

        expense.Payments.Add(new Payment
        {
            Participant = dummyParticipant,
            Amount = -10
        });

        trip.Expenses.Add(expense);

        db.Trips.Add(trip);

        await db.SaveChangesAsync();

        return (
            expense.Id,
            trip.Id,
            userParticipant.Id,
            dummyParticipant.Id
        );
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}