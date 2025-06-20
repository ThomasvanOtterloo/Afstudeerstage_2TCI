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
        // 1) get all ads from the database
        var all = await _context.Ads
            .OrderByDescending(a => a.CreatedAt)   // so that First() picks “newest”
            .ToListAsync();

        // 2) dedupe in‐memory by (ReferenceNumber, PhoneNumber, Price)
        return all
            .GroupBy(a => new { a.ReferenceNumber, a.PhoneNumber, a.Price })
            .Select(g => g.First())               // already sorted, so First() is newest
            .OrderBy(a => a.CreatedAt)            // final sort oldest→newest if you like
            .ToList();
    }

    public async Task<Ad> GetAdById(int id)
        => await _context.Ads.FirstOrDefaultAsync(a => a.Id == id);
// .find

    public async Task<Ad> CreateAd(Ad ad)
    {
        await _context.Ads.AddAsync(ad);
        await _context.SaveChangesAsync();
        return ad;
    }

    public async Task<IEnumerable<Ad>> GetAdsFiltered(
        string? brand,
        string? model,
        string? referenceNumber,
        int? daysAgo)
    {
        // 1) Build the EF filter query
        IQueryable<Ad> q = _context.Ads;

        if (!string.IsNullOrWhiteSpace(brand))
            q = q.Where(ad => ad.Brand == brand);

        if (!string.IsNullOrWhiteSpace(model))
            q = q.Where(ad => ad.Model == model);

        if (!string.IsNullOrWhiteSpace(referenceNumber))
            q = q.Where(ad => ad.ReferenceNumber!.Contains(referenceNumber));

        if (daysAgo.HasValue)
        {
            var cutoff = DateTime.UtcNow.AddDays(-daysAgo.Value);
            q = q.Where(ad => ad.CreatedAt >= cutoff);
        }

        var list = await q
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
//

        return list
            .GroupBy(a => new { a.ReferenceNumber, a.PhoneNumber, a.Price })
            .Select(g => g.First())
            .OrderByDescending(a => a.CreatedAt)
            .ToList();
    }

    public async Task RemoveAdByGroupId(string groupId)
    {
        var adsToRemove = _context.Ads.Where(ad => ad.GroupId == groupId);
        _context.Ads.RemoveRange(adsToRemove);
        await _context.SaveChangesAsync();
    }
}
