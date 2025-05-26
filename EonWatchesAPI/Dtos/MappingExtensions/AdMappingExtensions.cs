using EonWatchesAPI.DbContext;

namespace EonWatchesAPI.Dtos.MappingExtensions
{
    public static class AdMappingExtensions
    {
        public static Ad ToEntity(this CreateAdDto dto)
        {
            return new Ad
            {
                Brand = dto.Brand,
                Model = dto.Model,
                ReferenceNumber = dto.ReferenceNumber,
                Price = dto.Price,
                Currency = dto.Currency,
                Image = dto.Image,
                Video = dto.Video,
                TraderId = dto.TraderId,
                Trader = dto.Trader,
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
                PhoneNumber = dto.PhoneNumber,
                TraderName = dto.TraderName,
                IsAnSeller = dto.IsAnSeller,
                Archived = dto.Archived,
                CreatedAt = dto.CreatedAt,
                MessageId = dto.MessageId, 
                Group = null, 
                GroupId = dto.GroupId,
            };
        }
    }
}
