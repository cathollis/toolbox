using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Hollis.Toolbox.Functions.Entities;

[Index(nameof(AccessCode))]
public class PastebinItem
{
    public PastebinItem() { }

    public PastebinItem(string content, string accessCode)
    {
        ContentInDb = content;
        ContentStorageType = StorageType.Database;
        AccessCode = accessCode;
    }

    [Key]
    public Guid Id { get; init; } = Guid.NewGuid();

    [MaxLength(128)]
    public required string AccessCode { get; set; }

    public required StorageType ContentStorageType { get; set; } = StorageType.Database;

    [MaxLength(16384)]
    public required string? ContentInDb { get; set; }

    [MaxLength(128)]
    public string? PasswordHash { get; set; }

    public DateTimeOffset? ExpiredAfter { get; set; }

    public bool Expired { get; set; }

    public bool IsExpired()
    {
        if (Expired) return true;

        return ExpiredAfter <= DateTimeOffset.UtcNow;
    }

    public enum StorageType
    {
        Database,
        Blob
    }
}
