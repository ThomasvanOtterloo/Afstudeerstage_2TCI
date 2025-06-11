using EonWatchesAPI.Domain;
using EonWatchesAPI.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace EonWatchesAPI.DbContext;

public partial class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<Ad> Ads { get; set; }
    public DbSet<Trader> Traders { get; set; }
    public DbSet<WhitelistedGroup> WhitelistedGroups { get; set; }
    public DbSet<TraderWhitelistedGroup> TraderWhitelistedGroups { get; set; }
    public DbSet<Trigger> Triggers { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Trader configuration
        modelBuilder.Entity<Trader>()
            .HasIndex(t => t.PhoneNumber)
            .IsUnique();

        // Ad configuration
        modelBuilder.Entity<Ad>()
            .Property(a => a.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Ad>()
            .Property(a => a.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<Ad>()
            .Property(a => a.Archived)
            .HasDefaultValue(false);

        // Set the relationship with Trader as optional
        modelBuilder.Entity<Ad>()
            .HasOne(ad => ad.Trader)
            .WithMany(t => t.Ads)
            .HasForeignKey(ad => ad.TraderId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull); // Allow Trader to be null without cascading delete

        // Ad links to WhitelistedGroup via GroupId
        modelBuilder.Entity<Ad>()
            .HasOne(ad => ad.Group)
            .WithMany(wg => wg.Ads)
            .HasForeignKey(ad => ad.GroupId)
            .IsRequired(true); // GroupId is required and must exist in WhitelistedGroups

        // WhitelistedGroup configuration (represents the WhatsApp group itself)
        modelBuilder.Entity<WhitelistedGroup>(wg =>
        {
            wg.HasKey(w => w.Id); // Primary key is the WhatsApp group JID
        });

        // TraderWhitelistedGroup configuration (junction table for many-to-many)
        modelBuilder.Entity<TraderWhitelistedGroup>(twg =>
        {
            // Composite key: TraderId and GroupId
            twg.HasKey(t => new { t.TraderId, t.GroupId });

            // Foreign key to Trader
            twg.HasOne(t => t.Trader)
               .WithMany(t => t.TraderWhitelistedGroups)
               .HasForeignKey(t => t.TraderId)
               .IsRequired(true)
               .OnDelete(DeleteBehavior.Cascade); // Delete if trader is deleted

            // Foreign key to WhitelistedGroup
            twg.HasOne(t => t.Group)
               .WithMany(wg => wg.TraderWhitelistedGroups)
               .HasForeignKey(t => t.GroupId)
               .IsRequired(true)
               .OnDelete(DeleteBehavior.Cascade); // Delete if group is deleted

            // Unique constraint already enforced by composite key
        });

        // Seeding dummy Traders
        modelBuilder.Entity<Trader>().HasData(
            new Trader
            {
                Id = 1,
                Name = "Thomas Van Otterloo",
                PhoneNumber = "31612231577"
            },
            new Trader
            {
                Id = 2,
                Name = "ThomasFake",
                Email = "demoaccwatches@gmail.com",
                PhoneNumber = "31619348878",
                WhapiBearerToken = "9En69NKM61lH2dnHbRgiec0gLW0mck2E"
            }
        );

        // Seeding WhitelistedGroups (the groups themselves)
        modelBuilder.Entity<WhitelistedGroup>().HasData(
            new WhitelistedGroup
            {
                Id = "120363416829988594@g.us",
                GroupName = "Marktplaats"
            },
            new WhitelistedGroup
            {
                Id = "120363420163603590@g.us",
                GroupName = "CoolTraders Only! And god.."
            }
        );

        // Seeding TraderWhitelistedGroup (traders whitelisting groups)
        modelBuilder.Entity<TraderWhitelistedGroup>().HasData(
            new TraderWhitelistedGroup
            {
                TraderId = 2,
                GroupId = "120363416829988594@g.us"
            },
            new TraderWhitelistedGroup
            {
                TraderId = 2,
                GroupId = "120363420163603590@g.us"
            },
            new TraderWhitelistedGroup
            {
                TraderId = 1,
                GroupId = "120363420163603590@g.us"
            }
        );

        // Seeding Ads (TraderId can be null, GroupId must link to WhitelistedGroup)
        modelBuilder.Entity<Ad>().HasData(
            new Ad
            {
                Id = 3,
                TraderId = 2,
                GroupId = "120363420163603590@g.us",
                ReferenceNumber = "123",
                Price = 122
            },
            new Ad
            {
                Id = 4,
                TraderId = null, // Valid: Ads from random people
                GroupId = "120363420163603590@g.us",
                ReferenceNumber = "1234",
                Price = 11
            },
            new Ad
            {
                Id = 5,
                TraderId = 1,
                GroupId = "120363416829988594@g.us",
                ReferenceNumber = "5678",
                Price = 500
            }
        );
    }
}