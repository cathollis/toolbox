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

    /*
     CREATE TABLE Toolbox_Pastebin_PastebinItems (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        AccessCode nvarchar(100) NOT NULL,
        Content nvarchar(max) NOT NULL,
        PasswordHash nvarchar(512) NULL,
        ExpiredAfter datetimeoffset NULL,
        Expired bit NOT NULL
    );
     */
}
