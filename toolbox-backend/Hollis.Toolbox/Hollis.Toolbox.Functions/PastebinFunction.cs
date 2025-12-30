using Hollis.Toolbox.Functions.Entities;
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
    ToolboxDbContext dbContext)
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
        var obj = await JsonSerializer.DeserializeAsync<PastebinItem>(req.Body);
        if (obj is null) {
            return new BadRequestResult();
        }
        
        await dbContext.PastebinItems.AddAsync(obj);
        await dbContext.SaveChangesAsync();

        return new OkObjectResult(obj);
    }
}
