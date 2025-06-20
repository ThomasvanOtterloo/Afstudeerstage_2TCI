using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EonWatchesAPI.DbContext;
using EonWatchesAPI.DbContext.I_Repositories;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Dtos.MappingExtensions;
using EonWatchesAPI.Factories;
using EonWatchesAPI.Factories.SocialPlatforms;
using EonWatchesAPI.Services.I_Services;
using Microsoft.AspNetCore.Http;

namespace EonWatchesAPI.Services.Services
{
    public class DistributeAdService : IDistributeAdService
    {
        private readonly Dictionary<ConnectionType, ISocialConnection> _strategies;
        private readonly ITraderRepository _traderRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IAdRepository _adRepository;

        public DistributeAdService(
            Dictionary<ConnectionType, ISocialConnection> strategies,
            ITraderRepository traderRepository,
            IGroupRepository groupRepository,
            IAdRepository adRepository
        )
        {
            _strategies = strategies;
            _traderRepository = traderRepository;
            _groupRepository = groupRepository;
            _adRepository = adRepository;
        }

        public async Task SendMessageToGroup(DistributeAdDto dto)
        {
            var trader = await GetValidatedTrader(dto.Token);
            Console.WriteLine(trader.WhapiBearerToken);
            await ValidateGroups(dto.GroupIds, trader.Id);
            var payloadText = BuildTextPayload(dto.AdEntities);

            await ProcessDistribution(
                adDto: dto.AdEntities,
                textPayload: payloadText,
                imgPointer: null,
                dataUrlImage: null,
                connectionTypes: dto.ConnectionType,
                groupIds: dto.GroupIds,
                trader: trader
            );
        }

        public async Task SendImageToGroup(DistributeAdDto dto)
        {
            var adEntity = dto.AdEntities ??
                throw new ArgumentNullException(
                    nameof(dto.AdEntities),
                    "Ad details are required."
                );

            var imageFile = adEntity.Image ??
                throw new ArgumentNullException(
                    nameof(adEntity.Image),
                    "Please provide a valid image."
                );

            var trader = await GetValidatedTrader(dto.Token);
            await ValidateGroups(dto.GroupIds, trader.Id);

            var imgPointer = SaveImage(imageFile);
            var dataUrlImage = BuildDataUrl(imageFile);
            var payloadText = BuildTextPayload(adEntity);

            await ProcessDistribution(
                adDto: adEntity,
                textPayload: payloadText,
                imgPointer: imgPointer,
                dataUrlImage: dataUrlImage,
                connectionTypes: dto.ConnectionType,
                groupIds: dto.GroupIds,
                trader: trader
            );
        }

        private async Task ValidateGroups(IEnumerable<string> groupIds, int traderId)
        {
            var whitelisted = await _groupRepository.GetWhitelistedGroups(traderId);
            var allowed = whitelisted.Select(g => g.Id).ToHashSet();
            var invalid = groupIds.Where(g => !allowed.Contains(g)).ToList();
            if (invalid.Any())
                throw new InvalidOperationException(
                    $"The following groups are not whitelisted: {string.Join(", ", invalid)}"
                );
        }

        private async Task<Trader> GetValidatedTrader(int traderId)
        {
            var trader = await _traderRepository.GetTraderById(traderId);
            if (trader == null)
                throw new InvalidOperationException($"Trader with ID '{traderId}' not found.");
            return trader;
        }

        private string BuildTextPayload(CreateAdDto ad) =>
            TurnAdIntoString(ad);

        private string BuildDataUrl(IFormFile file)
        {
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            var base64 = Convert.ToBase64String(ms.ToArray());
            return $"data:{file.ContentType};base64,{base64}";
        }

        private async Task ProcessDistribution(
            CreateAdDto adDto,
            string textPayload,
            string? imgPointer,
            string? dataUrlImage,
            IEnumerable<ConnectionType> connectionTypes,
            IEnumerable<string> groupIds,
            Trader trader
        )
        {
           
                foreach (var type in connectionTypes)
                {
                    if (!_strategies.TryGetValue(type, out var strategy))
                        throw new NotSupportedException($"Unsupported connection type: {type}");
                    foreach (var groupId in groupIds)
                    {
                        string messageId = dataUrlImage != null
                            ? await strategy.SendImageToGroup(
                                trader.WhapiBearerToken,
                                textPayload,
                                dataUrlImage,
                                groupId
                              )
                            : await strategy.SendTextToGroup(
                                trader.WhapiBearerToken,
                                textPayload,
                                groupId
                              );

                        await SaveAdToDatabase(
                            dto: adDto,
                            imgPointer: imgPointer,
                            trader: trader,
                            messageId: messageId,
                            groupId: groupId
                        );
                    }
                }
            
        }

        private async Task SaveAdToDatabase(
            CreateAdDto dto,
            string? imgPointer,
            Trader trader,
            string messageId,
            string groupId
        )
        {
            var entity = dto.ToEntity(
                imgPointer ?? string.Empty,
                trader,
                messageId,
                groupId
            );
            await _adRepository.CreateAd(entity);
        }

        private string SaveImage(IFormFile formFile)
        {
            var saveDir = @"D:\WatchesImages";
            Directory.CreateDirectory(saveDir);

            var ext = Path.GetExtension(formFile.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(saveDir, fileName);

            using var stream = formFile.OpenReadStream();
            using var original = Image.FromStream(stream);

            const int maxW = 1280, maxH = 720;
            int w = original.Width, h = original.Height;

            if (w > maxW || h > maxH)
            {
                double ratio = Math.Min((double)maxW / w, (double)maxH / h);
                int newW = (int)(w * ratio),
                    newH = (int)(h * ratio);

                using var thumb = new Bitmap(newW, newH);
                using var g = Graphics.FromImage(thumb);
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.DrawImage(original, 0, 0, newW, newH);
                thumb.Save(filePath, ImageFormat.Jpeg);
            }
            else
            {
                original.Save(filePath);
            }

            return $"https://localhost:7240/Media/{fileName}";
        }

        private string TurnAdIntoString(CreateAdDto ad)
        {
            var parts = new List<string>();
            // list of property‐names you want to skip entirely
            var skip = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "TraderId",
                    "Image",     // whatever your property is called
                    "Video",     // or VideoUrl / VideoPointer etc.
                };

            foreach (var prop in typeof(CreateAdDto).GetProperties())
            {
                if (skip.Contains(prop.Name))
                    continue;

                if (typeof(IFormFile).IsAssignableFrom(prop.PropertyType))
                    continue;

                var raw = prop.GetValue(ad);
                if (raw == null)
                    continue;

                if (prop.PropertyType == typeof(string) &&
                    string.IsNullOrWhiteSpace((string)raw))
                    continue;

                parts.Add($"{prop.Name}: {raw}");
            }
            // string builder.

            return string.Join(", ", parts);
        }

    }
}
