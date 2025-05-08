using System.ComponentModel.DataAnnotations;

namespace EonWatchesAPI.Dtos
{
    public class AdFilterDto
    {
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? ReferenceNumber { get; set; }
        public int? DaysAgo { get; set; }

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if (Brand == null && Model == null && ReferenceNumber == null)
        //        yield return new ValidationResult(
        //            "You must supply at least one of Brand, Model or ReferenceNumber.",
        //            new[] { nameof(Brand), nameof(Model), nameof(ReferenceNumber) }
        //        );
        //}
    }


}
