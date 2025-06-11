using EonWatchesAPI.DbContext.I_Repositories;
using EonWatchesAPI.Domain;
using EonWatchesAPI.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EonWatchesAPI.DbContext.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly AppDbContext _context;
        private readonly IAdRepository _adRepository;

        public GroupRepository(AppDbContext context, IAdRepository adRepository)
        {
            _context = context;
            _adRepository = adRepository;
        }

        public async Task DeleteWhitelistedGroup(int userId, string groupId)
        {
            // Find the trader-group whitelist entry
            var whitelistEntry = await _context.TraderWhitelistedGroups
                .FirstOrDefaultAsync(twg => twg.TraderId == userId && twg.GroupId == groupId);

            if (whitelistEntry == null)
            {
                throw new KeyNotFoundException("Group not found or not whitelisted by this user.");
            }

            // Remove the trader's whitelist entry
            _context.TraderWhitelistedGroups.Remove(whitelistEntry);
            await _context.SaveChangesAsync();

            // Check if any other traders have this group whitelisted
            var remainingWhitelists = await _context.TraderWhitelistedGroups
                .AnyAsync(twg => twg.GroupId == groupId);

            if (!remainingWhitelists)
            {
                // No other traders whitelist this group, so remove related ads and the group
                await _adRepository.RemoveAdByGroupId(groupId);
                var group = await _context.WhitelistedGroups
                    .FirstOrDefaultAsync(g => g.Id == groupId);
                if (group != null)
                {
                    _context.WhitelistedGroups.Remove(group);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task WhitelistGroup(int userId, string groupId, string groupName)
        {
            // Check if the trader already whitelisted this group
            var alreadyWhitelistedByUser = await _context.TraderWhitelistedGroups
                .AnyAsync(twg => twg.TraderId == userId && twg.GroupId == groupId);

            if (alreadyWhitelistedByUser)
            {
                throw new Exception("This group is already whitelisted by the user.");
            }

            // Check if the group exists in WhitelistedGroups
            var groupExists = await _context.WhitelistedGroups
                .AnyAsync(g => g.Id == groupId);

            if (!groupExists)
            {
                // Add the group if it doesn't exist
                var newGroup = new WhitelistedGroup
                {
                    Id = groupId,
                    GroupName = groupName
                };
                _context.WhitelistedGroups.Add(newGroup);
            }

            // Add the trader-group whitelist entry
            var newWhitelistEntry = new TraderWhitelistedGroup
            {
                TraderId = userId,
                GroupId = groupId
            };
            _context.TraderWhitelistedGroups.Add(newWhitelistEntry);

            await _context.SaveChangesAsync();
        }

        public async Task<List<WhitelistedGroup>> GetWhitelistedGroups(int traderId)
        {
            return await _context.TraderWhitelistedGroups
                .Where(twg => twg.TraderId == traderId)
                .Include(twg => twg.Group)
                .Select(twg => twg.Group)
                .ToListAsync();
        }
    }
}