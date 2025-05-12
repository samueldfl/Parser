namespace Domain.Types;

public class AlgebraGraphDto
{
    public required string Type { get; set; }
    public required string Label { get; set; }
    public List<AlgebraGraphDto> Children { get; set; } = [];
}
