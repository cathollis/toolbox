using Hollis.Toolbox.Functions.Entities;
using Hollis.Toolbox.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
    AccessCodeGenerator accessCodeGenerator,
    PasswordHasher<PastebinItem> hasher)
{
    private const uint PASSWORD_LENGTH = 4;

    [Function(nameof(GetPastebin))]
    public async Task<IActionResult> GetPastebin(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(PastebinItem) + "/{code}")] HttpRequest req,
        string code)
    {
        var saveDatabase = false;
        var pastebinItem = await dbContext.PastebinItems
            .FirstOrDefaultAsync(x => x.AccessCode == code);

        // verify query
        if (pastebinItem is null)
        {
            logger.LogWarning("Pastebin Item not found with code: {code}.", code);
            return new NotFoundResult();
        }

        // verify expired time
        if (pastebinItem.IsExpired())
        {
            logger.LogWarning("Pastebin Item with code: {code} has expired.", code);
            return new NotFoundResult();
        }

        // verify password
        if (pastebinItem.PasswordHash is not null)
        {
            var passsword = pastebinItem.PasswordHash.Trim();
            if (passsword.Length <= PASSWORD_LENGTH)
            {
                return new NotFoundResult();
            }

            var verify = hasher.VerifyHashedPassword(pastebinItem, pastebinItem.PasswordHash, passsword);
            if (verify == PasswordVerificationResult.Failed)
            {
                return new NotFoundResult();
            }

            if (verify == PasswordVerificationResult.SuccessRehashNeeded)
            {
                var passwordHash = hasher.HashPassword(pastebinItem, passsword);
                pastebinItem.PasswordHash = passwordHash;
                saveDatabase = true;
            }
        }

        if (pastebinItem.ConfiguredExpiredAfterRead)
        {
            logger.LogWarning("Pastebin Item with code: {code} has configured expired after read, update.", code);

            pastebinItem.Expired = true;
            saveDatabase = true;
        }

        if (saveDatabase)
        {
            await dbContext.SaveChangesAsync();
        }
        return new OkObjectResult(pastebinItem);
    }

    [Function(nameof(CreatePastebin))]
    public async Task<ActionResult<PastebinItemCreateResponse>> CreatePastebin(
        [HttpTrigger(AuthorizationLevel.User, "post", Route = nameof(PastebinItem) + "/")] HttpRequest req)
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

        if (createReq.Password is not null
            && !string.IsNullOrWhiteSpace(createReq.Password)
            && createReq.Password.Length < PASSWORD_LENGTH)
        {
            return new BadRequestResult();
        }

        if (createReq.ExpiredAfter is not null && createReq.ExpiredAfter.Value <= DateTimeOffset.Now)
        {
            return new BadRequestResult();
        }

        var accessCodeLength = await GetAccessCodeDefaultLength();
        var accessCode = await accessCodeGenerator.GenerateAsync(accessCodeLength, AccessCodeExists);

        var newPastebinItem = new PastebinItem()
        {
            AccessCode = accessCode,
            ContentStorageType = PastebinItem.StorageType.Database,
            ContentInDb = createReq.Content,
            ExpiredAfter = createReq.ExpiredAfter,
            ConfiguredExpiredAfterRead = createReq.ExpiredAfterRead,
        };

        if (createReq.Password is not null)
        {
            newPastebinItem.PasswordHash = hasher.HashPassword(newPastebinItem, createReq.Password);
        }

        await dbContext.PastebinItems.AddAsync(newPastebinItem);
        await dbContext.SaveChangesAsync();

        var createResp = new PastebinItemCreateResponse(newPastebinItem.AccessCode);
        return new OkObjectResult(createResp);
    }

    private Task<bool> AccessCodeExists(string code)
        => dbContext.PastebinItems.AnyAsync(x => x.AccessCode == code);

    private async Task<uint> GetAccessCodeDefaultLength()
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
