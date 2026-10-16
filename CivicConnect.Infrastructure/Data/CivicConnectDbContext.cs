using CivicConnect.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CivicConnect.Infrastructure.Data;

public class CivicConnectDbContext : DbContext
{
    public CivicConnectDbContext(DbContextOptions<CivicConnectDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Describes exactly how entities map to tables, columns, constraints and relationships
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.Property(user => user.FullName).IsRequired().HasMaxLength(150);
            entity.Property(user => user.EmailAddress).IsRequired().HasMaxLength(254);
            entity.HasIndex(user => user.EmailAddress).IsUnique();   // no duplicate accounts
            entity.Property(user => user.Role).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(category => category.Id);
            entity.Property(category => category.Name).IsRequired().HasMaxLength(100);
            entity.Property(category => category.Description).HasMaxLength(500);
            entity.HasIndex(category => category.Name).IsUnique();
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(ticket => ticket.Id);
            entity.Property(ticket => ticket.Title).IsRequired().HasMaxLength(200);
            entity.Property(ticket => ticket.Description).IsRequired().HasMaxLength(2000);
            entity.Property(ticket => ticket.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(ticket => ticket.Priority).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(ticket => ticket.Status);                // common filter, so index it

            // Restrict = you cannot delete a category or user that still has tickets
            entity.HasOne(ticket => ticket.Category)
                  .WithMany(category => category.Tickets)
                  .HasForeignKey(ticket => ticket.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ticket => ticket.SubmittedBy)
                  .WithMany(user => user.SubmittedTickets)
                  .HasForeignKey(ticket => ticket.SubmittedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(log => log.Id);
            entity.Property(log => log.ActionName).IsRequired().HasMaxLength(100);
            entity.Property(log => log.Details).IsRequired().HasMaxLength(1000);

            // If a ticket is ever deleted, its audit history stays (TicketId becomes null)
            entity.HasOne(log => log.Ticket)
                  .WithMany()
                  .HasForeignKey(log => log.TicketId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        SeedDevelopmentData(modelBuilder);
    }

    // SUGGESTION: starter rows so you can test immediately without creating categories/users by hand
    private static void SeedDevelopmentData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new { Id = 1, Name = "Water and sanitation", Description = "Leaks, burst pipes, sewage", BaseUrgencyScore = 8 },
            new { Id = 2, Name = "Roads and potholes", Description = "Road damage and signage", BaseUrgencyScore = 5 },
            new { Id = 3, Name = "Street lighting", Description = "Broken or missing streetlights", BaseUrgencyScore = 4 },
            new { Id = 4, Name = "Waste collection", Description = "Missed refuse collections", BaseUrgencyScore = 3 });

        modelBuilder.Entity<User>().HasData(
            new
            {
                Id = 1,
                FullName = "Demo Citizen",
                EmailAddress = "demo.citizen@example.com",
                Role = UserRole.Citizen,
                RegisteredAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
    }
}
