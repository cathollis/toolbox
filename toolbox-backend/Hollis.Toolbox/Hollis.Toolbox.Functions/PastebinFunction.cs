using Hollis.Toolbox.Functions.Entities;
using Hollis.Toolbox.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Hollis.Toolbox.Functions;

public class PastebinFunction(
    ILogger<PastebinFunction> logger,
    ToolboxDbContext dbContext,
    AccessCodeGenerator accessCodeGenerator)
{
    [Function(nameof(GetPastebin))]
    public async Task<IActionResult> GetPastebin(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{code}")] HttpRequest req, string code
        )
    {
        var pastebinItem = await dbContext.PastebinItems
            .FirstOrDefaultAsync(x => x.AccessCode == code);

        if (pastebinItem is null || pastebinItem.IsExpired())
        {
            logger.LogWarning("Pastebin Item not found with code: {code}", code);
            return new NotFoundResult();
        }

        return new OkObjectResult(pastebinItem);
    }

    [Function(nameof(CreatePastebin))]
    public async Task<IActionResult> CreatePastebin([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        PastebinItemCreateRequest? createReq = null;
        try
        {
            createReq = await JsonSerializer.DeserializeAsync<PastebinItemCreateRequest>(req.Body);
        }
        catch (Exception)
        {
            return new BadRequestResult();
        }

        if (createReq is null)
        {
            return new BadRequestResult();
        }

        var accessCodeLength = await GetAccessCodeDefaultLength();
        var accessCode = await accessCodeGenerator.GenerateAsync(accessCodeLength, AccessCodeExists);

        var newPastebinItem = new PastebinItem(accessCode)
        {
            ContentStorageType = PastebinItem.StorageType.Database
        };

        await dbContext.PastebinItems.AddAsync(newPastebinItem);
        await dbContext.SaveChangesAsync();

        return new OkObjectResult(newPastebinItem);
    }

    public Task<bool> AccessCodeExists(string code)
        => dbContext.PastebinItems.AnyAsync(x => x.AccessCode == code);

    public async Task<uint> GetAccessCodeDefaultLength()
    {
        const uint INIT_LENGTH = 4;
        var currentLength = await dbContext.PastebinItems.MaxAsync(x => x.AccessCode.Length);
        var defaultLength = currentLength - 1;
        if (defaultLength < INIT_LENGTH)
        {
            return INIT_LENGTH;
        }

        return (uint)defaultLength;
    }
}
