using EonWatchesAPI.DbContext.I_Repositories;
using EonWatchesAPI.Dtos;
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

    public async Task<IEnumerable<Ad>> GetAdsFiltered(string? brand, string? model, string? referenceNumber)
    {
        IQueryable<Ad> q = _context.Ads.AsQueryable();

        // Apply each filter only if it was provided
        if (!string.IsNullOrWhiteSpace(brand))
            q = q.Where(ad => ad.Brand == brand);

        if (!string.IsNullOrWhiteSpace(model))
            q = q.Where(ad => ad.Model == model);

        if (!string.IsNullOrWhiteSpace(referenceNumber))
            q = q.Where(ad => ad.ReferenceNumber == referenceNumber);

        // Execute and return
        return await q.ToListAsync();
    }


}