using Hollis.Toolbox.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

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
        var pastebinItem = await dbContext.PastebinItems.FirstOrDefaultAsync(x => x.AccessCode == code);

        if (pastebinItem is null || pastebinItem.IsExpired())
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(pastebinItem);
    }

    [Function(nameof(CreatePastebin))]
    public IActionResult CreatePastebin([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}
