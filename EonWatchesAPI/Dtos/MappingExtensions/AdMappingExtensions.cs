using EonWatchesAPI.DbContext;

namespace EonWatchesAPI.Dtos.MappingExtensions
{
    public static class AdMappingExtensions
    {
        public static Ad ToEntity(this CreateAdDto dto, string imgPointer, Trader trader, string messageId, string GroupId)
        {
            return new Ad
            {
                Brand = dto.Brand,
                Model = dto.Model,
                ReferenceNumber = dto.ReferenceNumber,
                Price = dto.Price,
                Currency = dto.Currency,
                Image = imgPointer,
                Video = dto.Video,
                TraderId = trader.Id,
                Color = dto.Color,
                Condition = dto.Condition,
                YearOfManufacture = dto.YearOfManufacture,
                BatchCode = dto.BatchCode,
                Location = dto.Location,
                FullSet = dto.FullSet,
                PaymentMethod = dto.PaymentMethod,
                Movement = dto.Movement,
                Caliber = dto.Caliber,
                CaseDiameter = dto.CaseDiameter,
                Other = dto.Other,
                Shipping = dto.Shipping,
                PhoneNumber = trader.PhoneNumber,
                TraderName = trader.Name,
                MessageId = messageId,
                GroupId = GroupId,
                Archived = false,
                CreatedAt = DateTime.UtcNow,
            };
        }
    }
}
