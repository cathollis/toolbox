namespace Hollis.Toolbox.Functions.Models;

public class PastebinItemCreateRequest
{
    public required string Content { get; init; }
    
    public DateTimeOffset? ExpiredAfter { get; init; }

    public bool ExpiredAfterRead { get; init; } = false;

    public string? Password { get; init; }
}
