using EonWatchesAPI.DbContext.I_Repositories;
using EonWatchesAPI.Dtos;
using Microsoft.EntityFrameworkCore;

namespace EonWatchesAPI.DbContext.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly AppDbContext _context;
        public GroupRepository(AppDbContext context) 
        {
            _context = context;
        }

        public async Task DeleteWhitelistedGroup(int userId, string groupId)
        {
            var group = await _context.WhitelistedGroups
                .FirstOrDefaultAsync(g => g.Id == groupId && g.TraderId == userId);

            if (group == null)
            {
                throw new KeyNotFoundException("Group not found or not whitelisted by this user.");
            }

            _context.WhitelistedGroups.Remove(group);
            await _context.SaveChangesAsync();
            
        }


        public async Task WhitelistGroup(int userId, string groupId, string groupName)
        {
            var alreadyWhitelisted = await _context.WhitelistedGroups
                .AnyAsync(g => g.Id == groupId && g.TraderId == userId);

            if (alreadyWhitelisted)
            {
                throw new Exception("Group allready exists");
            }
                var newEntry = new WhitelistedGroups
                {
                    Id = groupId,
                    TraderId = userId,
                    GroupName = groupName
                };

                _context.WhitelistedGroups.Add(newEntry);
                await _context.SaveChangesAsync();
        }

        public async Task<List<WhitelistedGroups>> GetWhitelistedGroups(int traderId)
        {
            return await _context.WhitelistedGroups
                .Where(g => g.TraderId == traderId)                
                .ToListAsync();
        }

    }
}
