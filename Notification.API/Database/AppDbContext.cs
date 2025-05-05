using Microsoft.EntityFrameworkCore;
using Notification.Api.Sagas;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Notification.Api.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationSagaData>().HasKey(s => s.CorrelationId);

        // Configure notification entity
        modelBuilder.Entity<NotificationEntity>()
        .Property(n => n.Status)
            .HasConversion<string>();
    }

    public DbSet<NotificationEntity> Notifications { get; set; }

    public DbSet<NotificationSagaData> SagaData { get; set; }
}