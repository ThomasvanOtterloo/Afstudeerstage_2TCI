using EonWatchesAPI.DbContext;

namespace EonWatchesAPI.Dtos.MappingExtensions
{
    public static class TraderMappingExtensions
    {
        public static Trader ToEntity(this TraderDto dto)
        {
            return new Trader
            {
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                Password = dto.Password,
                Name = dto.Name,
            };
        }
    }
}
