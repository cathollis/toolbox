using Hollis.Toolbox.Functions;
using Hollis.Toolbox.Functions.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
//builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Services.AddDbContext<ToolboxDbContext>(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("SQLCONNSTR_SHARED");
    options.UseSqlServer(connectionString);
});

builder.Services.AddSingleton<AccessCodeGenerator>();
builder.Services.AddSingleton<PasswordHasher<PastebinItem>>();

builder.Build().Run();
