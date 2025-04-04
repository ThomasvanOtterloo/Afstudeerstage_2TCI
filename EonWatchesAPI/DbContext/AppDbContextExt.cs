using Microsoft.EntityFrameworkCore;

namespace EonWatchesAPI.DbContext;

public partial class AppDbContext
{
    public override int SaveChanges()
    {
        
        var test = ChangeTracker.Entries<Ad>()
            .Where(Ad => Ad.State == EntityState.Added || Ad.State == EntityState.Modified)
            .ToList();
        var saveResult = base.SaveChanges();
        
        // todo FILTER
        
        
        return saveResult;
    }
    
    
}