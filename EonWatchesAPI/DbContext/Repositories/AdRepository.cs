using EonWatchesAPI.DbContext.I_Repositories;
using Microsoft.EntityFrameworkCore;

namespace EonWatchesAPI.DbContext;

public class AdRepository : IAdRepository
{
    private readonly AppDbContext _context;
    
    public AdRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Ad>> GetAds()
    {
        return await _context.Ads.ToListAsync();
    }
    
    public async Task<Ad> GetAdById(int id)
    {
        return await _context.Ads.FirstOrDefaultAsync(a => a.Id == id);
    }
    
    public async Task<Ad> CreateAd(Ad ad)
    {
        await _context.Ads.AddAsync(ad);
        await _context.SaveChangesAsync();
        return ad;
    }
    
}