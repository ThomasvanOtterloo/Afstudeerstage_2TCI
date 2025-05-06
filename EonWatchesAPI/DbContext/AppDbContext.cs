using EonWatchesAPI.Enums;
using Microsoft.EntityFrameworkCore;

namespace EonWatchesAPI.DbContext;

public partial class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<Ad> Ads { get; set; }
    public DbSet<Trader> Traders { get; set; }
    public DbSet<WhitelistedGroups> WhitelistedGroups { get; set; }
    public DbSet<Trigger> Triggers { get; set; }
    

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Trader>()
            .HasIndex(t => t.PhoneNumber)
            .IsUnique();

        modelBuilder.Entity<Ad>()
        .Property(a => a.Price)
        .HasPrecision(18, 2);

        modelBuilder.Entity<Ad>()
         .Property(a => a.CreatedAt)
         .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<Ad>()
            .Property(a => a.Archived)
            .HasDefaultValue(false);


        modelBuilder.Entity<Ad>()
        .HasOne(ad => ad.Trader)
        .WithMany(t => t.Ads)
        .HasForeignKey(ad => ad.TraderId)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ad>()
        .HasOne(ad => ad.Group)
        .WithMany()
        .HasForeignKey(ad => ad.GroupId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WhitelistedGroups>()
            .HasOne(t => t.Trader)
            .WithMany(t => t.WhitelistedGroups)
            .HasForeignKey(t => t.TraderId);


        // 🔥 Seeding dummy Traders
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
                PhoneNumber = "31619348878"
            }
        );

        modelBuilder.Entity<WhitelistedGroups>().HasData(
        new WhitelistedGroups
        {
            Id = "120363416829988594@g.us",
            GroupName = "Marktplaats",
            TraderId = 2,
        },
        new WhitelistedGroups 
        {
            Id = "120363420163603590@g.us",
            GroupName = "CoolTraders Only! And god..",
            TraderId = 2
        }
        );

    }

    

}