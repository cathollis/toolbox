using Hollis.Toolbox.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Hollis.Toolbox.Functions;

public class PastebinFunction(ILogger<PastebinFunction> logger)
{
    const string HTTP_GET = "GET";
    const string HTTP_POST = "POST";
    const string QUERY_SQL = "select * from Toolbox_Pastebin_PastebinItems where AccessCode = @Code and Expired = false";

    [Function(nameof(GetPastebin))]
    public IActionResult GetPastebin(
        [HttpTrigger(AuthorizationLevel.Anonymous, HTTP_GET, Route = "{code}")] HttpRequest _,
        [SqlInput(QUERY_SQL, "SqlConnectionString", parameters: "@Code={code}")] IEnumerable<PastebinItem> pastebinItemList
        )
    {
        if (!pastebinItemList.Any())
        {
            return new NotFoundResult();
        }

        var result = pastebinItemList.FirstOrDefault(x => !x.IsExpired());
        if(result is null)
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(result);
    }

    [Function(nameof(CreatePastebin))]
    public IActionResult CreatePastebin([HttpTrigger(AuthorizationLevel.Anonymous, HTTP_POST)] HttpRequest req)
    {
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}
