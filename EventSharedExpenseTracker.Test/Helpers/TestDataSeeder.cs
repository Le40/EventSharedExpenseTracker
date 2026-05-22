using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Infrastructure.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSharedExpenseTracker.Tests.Helpers
{
    public class TestDataSeeder
    {
        private readonly ApplicationDbContext _db;

        public TestDataSeeder(ApplicationDbContext db)
        {
            _db = db;
        }

        public record SeededUser(int Id, string CustomUserName);

        public record SeededTrip(
            int TripId,
            int CreatorId,
            int UserParticipantId,
            int DummyParticipantId);

        public record SeededExpense(
            int TripId,
            int ExpenseId,
            int UserParticipantId,
            int DummyParticipantId);

        public async Task<SeededUser> SeedAuthenticatedUserAsync(
            CancellationToken cancellationToken = default)
        {
            var user = new CustomUser
            {
                Id = 1,
                CustomUserName = "testuser"
            };

            _db.CustomUsers.Add(user);
            await _db.SaveChangesAsync(cancellationToken);

            return new SeededUser(user.Id, user.CustomUserName);
        }

        public async Task<SeededUser> SeedUserAsync(
            string? userName = null,
            CancellationToken cancellationToken = default)
        {
            var uniqueName = userName ?? $"testuser-{Guid.NewGuid():N}";

            var user = new CustomUser
            {
                CustomUserName = uniqueName
            };

            _db.CustomUsers.Add(user);
            await _db.SaveChangesAsync(cancellationToken);

            return new SeededUser(user.Id, user.CustomUserName);
        }

        public async Task<SeededTrip> SeedTripWithParticipantsAsync(
            CancellationToken cancellationToken = default)
        {
            var user1 =  await SeedAuthenticatedUserAsync();

            var trip = new Trip
            {
                Name = "Test Trip",
                DateFrom = DateTime.Today,
                DateTo = DateTime.Today.AddDays(1),
                CreatorId = user1.Id
            };

            var userParticipant = new TripParticipant
            {
                UserId = user1.Id,
                DisplayName = user1.CustomUserName
            };

            var dummyParticipant = new TripParticipant
            {
                DisplayName = "dummy"
            };

            trip.Participants.Add(userParticipant);
            trip.Participants.Add(dummyParticipant);

            _db.Trips.Add(trip);
            await _db.SaveChangesAsync(cancellationToken);

            return new SeededTrip(
                trip.Id,
                user1.Id,
                userParticipant.Id,
                dummyParticipant.Id);
        }

        public async Task<SeededExpense> SeedExpenseAsync(
            CancellationToken cancellationToken = default)
        {
            var trip = await SeedTripWithParticipantsAsync(cancellationToken);

            var dbTrip = await _db.Trips
                .Include(t => t.Participants)
                .FirstAsync(t => t.Id == trip.TripId, cancellationToken);

            var userParticipant = dbTrip.Participants
                .First(p => p.Id == trip.UserParticipantId);

            var dummyParticipant = dbTrip.Participants
                .First(p => p.Id == trip.DummyParticipantId);

            var expense = new Expense
            {
                Name = "Dinner",
                Date = DateTime.Today,
                Category = ExpenseCategory.Food,
                CreatorId = trip.CreatorId
            };

            expense.Payments.Add(new Payment
            {
                ParticipantId = userParticipant.Id,
                Amount = 20
            });

            expense.Payments.Add(new Payment
            {
                ParticipantId = userParticipant.Id,
                Amount = -10
            });

            expense.Payments.Add(new Payment
            {
                ParticipantId = dummyParticipant.Id,
                Amount = -10
            });

            dbTrip.Expenses.Add(expense);

            await _db.SaveChangesAsync(cancellationToken);

            return new SeededExpense(
                dbTrip.Id,
                expense.Id,
                userParticipant.Id,
                dummyParticipant.Id);
        }
    }
}
