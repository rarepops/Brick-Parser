using System.ComponentModel.DataAnnotations;

public record PartDto
{
    [Required(ErrorMessage = "UUID is required")]
    public required string Uuid { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "DesignId must be positive")]
    public required int DesignId { get; init; }

    [Required(ErrorMessage = "TransformMatrix is required")]
    [Length(16, 16, ErrorMessage = "TransformMatrix must have exactly 16 elements")]
    public required float[] TransformMatrix { get; init; }
}
