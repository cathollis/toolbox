using Hollis.Toolbox.Functions.Entities;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.Azure.Functions.Worker.Http;

namespace Hollis.Toolbox.Functions.Models;

public class Output
{
    [SqlOutput("dbo.Pastebin", connectionStringSetting: "SqlConnectionString")]
    public required PastebinItem Content { get; init;  }
    public required HttpResponseData HttpResponse { get; init; }
}
