using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Infrastructure.Identity;

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
            modelBuilder.Entity<Trip>().Navigation(t => t.Participants).AutoInclude();

            // FRIENDSHIPS
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


            // APPLICATION USER TO CUSTOM USER
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.CustomUser)
                .WithOne()
                .HasForeignKey<ApplicationUser>(u => u.CustomUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);


            // if Trip.Creator(AppUser) deleted => delete all Trips created by him
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Creator)
                .WithMany(u => u.TripsCreated)
                .HasForeignKey(t => t.CreatorId)
                .OnDelete(DeleteBehavior.Cascade);

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


            // if User(AppUser) deleted => SET NULL TripParticipant.User - Becomes Dummy Participant
            modelBuilder.Entity<TripParticipant>()
                .HasOne(p => p.User)
                .WithMany(u => u.TripsParticipated)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // if User(AppUser) is deleted => SET NULL Payment.User - As if Dummy Participant
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
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
