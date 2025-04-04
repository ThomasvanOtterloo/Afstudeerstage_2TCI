using EonWatchesAPI.Enums;
using Microsoft.EntityFrameworkCore;

namespace EonWatchesAPI.DbContext;

public partial class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<Ad> Ads { get; set; }
    public DbSet<Trader> Traders { get; set; }
    public DbSet<Trigger> Triggers { get; set; }
    

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ad>().HasData(
            new Ad
            {
                Id = 1,
                GroupId = "G001",
                Brand = "Omega",
                Model = "Speedmaster",
                ReferenceNumber = "311.30.42.30.01.005",
                Price = 5999.99m,
                Currency = "USD",
                IsAnSeller = true,
                Archived = false,
                CreatedAt = new DateTime(2025, 01, 15, 10, 0, 0, DateTimeKind.Utc)
            },
            new Ad
            {
                Id = 2,
                GroupId = "G002",
                Brand = "Rolex",
                Model = "Submariner",
                ReferenceNumber = "124060",
                Price = 8999.50m,
                Currency = "USD",
                IsAnSeller = false,
                Archived = false,
                CreatedAt = new DateTime(2025, 02, 20, 12, 30, 0, DateTimeKind.Utc)
            },
            new Ad
            {
                Id = 3,
                GroupId = "G003",
                Brand = "Seiko",
                Model = "Prospex",
                ReferenceNumber = "SRPD21",
                Price = 450.00m,
                Currency = "EUR",
                IsAnSeller = true,
                Archived = true,
                CreatedAt = new DateTime(2024, 12, 01, 8, 0, 0, DateTimeKind.Utc)
            }
        );
        
        
        modelBuilder.Entity<Trader>().HasData(
            new Trader
            {
                Id = 1,
                Name = "Thomas de Watchman",
                Email = "thomas@example.com",
                Password = "hashed_password_1", // Replace with actual hash in real app
                PhoneNumber = "+31612345678",
                Role = Roles.ADMIN
            },
            new Trader
            {
                Id = 2,
                Name = "Lana Lux",
                Email = "lana@example.com",
                Password = "hashed_password_2",
                PhoneNumber = "+31687654321",
                Role = Roles.TRADER
            },
            new Trader
            {
                Id = 3,
                Name = "Bob the Buyer",
                Email = "bob@example.com",
                Password = "hashed_password_3",
                PhoneNumber = "+31699887766",
                Role = Roles.TRADER
            }
        );
    }
}