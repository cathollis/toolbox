namespace Hollis.Toolbox.Functions.Entities;

public class PastebinItem
{
    public PastebinItem() { }

    public PastebinItem(string content, string accessCode)
        => (Content, AccessCode) = (content, accessCode);

    public Guid Id { get; } = Guid.NewGuid();

    public required string AccessCode { get; set; }

    public required string Content { get; set; }

    public string? PasswordHash { get; set; }

    public DateTimeOffset? ExpiredAfter { get; set; }

    public bool Expired { get; set; }

    public bool IsExpired()
    {
        if (Expired) return true;

        return ExpiredAfter <= DateTimeOffset.UtcNow;
    }
}
