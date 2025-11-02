using System.ComponentModel.DataAnnotations;

public record ModelDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(
        200,
        MinimumLength = 1,
        ErrorMessage = "Name must be between 1 and 200 characters"
    )]
    public required string Name { get; init; }

    [Range(0, int.MaxValue, ErrorMessage = "PartCount must be non-negative")]
    public required int PartCount { get; init; }

    [Required(ErrorMessage = "Parts collection is required")]
    [MinLength(1, ErrorMessage = "At least one part is required")]
    public required List<PartDto> Parts { get; init; }
}
