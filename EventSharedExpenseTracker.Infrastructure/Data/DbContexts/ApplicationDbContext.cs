using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace EventSharedExpenseTracker.Infrastructure.Data.DbContexts
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<CustomUser> CustomUsers { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<TripParticipant> TripParticipants { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Payment> Payments { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // auto include things that gets loaded toghether every time
            modelBuilder.Entity<Expense>().Navigation(e => e.Payments).AutoInclude();
            //modelBuilder.Entity<Expense>().Navigation(e => e.Creator).AutoInclude();
            modelBuilder.Entity<Payment>().Navigation(p => p.Participant).AutoInclude();
            modelBuilder.Entity<TripParticipant>().Navigation(u => u.User).AutoInclude();
            modelBuilder.Entity<Trip>().Navigation(t => t.Participants).AutoInclude();
            //modelBuilder.Entity<CustomUser>().Navigation(t => t.Friends).AutoInclude();

            //// FRIENDSHIPS MANY : MANY
            modelBuilder.Entity<Friendship>()
                .HasKey(f => f.Id);

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.User)
                .WithMany(u => u.Friends)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Friend)
                .WithMany()
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Restrict);

            // APPLICATION USER TO CUSTOM USER 1:1
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.CustomUser)
                .WithOne()
                .HasForeignKey<ApplicationUser>(u => u.CustomUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // TRIP 1:MANY
            // if Trip.Creator(AppUser) deleted => remove him from trip, set creator to null
            // there can be more participants in the trip. No participants, handle in code.
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Creator)
                .WithMany(u => u.TripsCreated)
                .HasForeignKey(t => t.CreatorId)
                .OnDelete(DeleteBehavior.SetNull);

            // then when Trip is deleted => Expenses are deleted
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Trip)
                .WithMany(t => t.Expenses)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // then when Expense is deleted => Payments are deleted
            modelBuilder.Entity<Expense>()
                .HasMany(e => e.Payments)
                .WithOne(p => p.Expense)
                .HasForeignKey(p => p.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(p => p.AmountOriginal)
                      .HasPrecision(18, 2);

                entity.Property(p => p.AmountBase)
                      .HasPrecision(18, 2);
            });

            modelBuilder.Entity<Expense>(entity =>
            {
                entity.Property(e => e.ExchangeRateToBase)
                      .HasPrecision(18, 8);

                entity.Property(e => e.CurrencyCode)
                      .HasMaxLength(3);

                entity.Property(e => e.BaseCurrencyCode)
                      .HasMaxLength(3);
            });

            modelBuilder.Entity<TripParticipant>()
                .HasOne(p => p.Trip)
                .WithMany(t => t.Participants)
                .HasForeignKey(p => p.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Participant)
                .WithMany(p => p.Payments)
                .HasForeignKey(p => p.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);


            // if User(AppUser) deleted => SET NULL TripParticipant.User - Becomes Dummy Participant
            modelBuilder.Entity<TripParticipant>()
                .HasOne(p => p.User)
                .WithMany(u => u.TripsParticipated)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // if Expense.Creator is deleted => SET NULL Expense.Creator - so Expense is not deleted and can by edited by anyone.
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Creator)
                .WithMany(u => u.ExpensesCreated)
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.SetNull);


            base.OnModelCreating(modelBuilder);
        }
    }
}
