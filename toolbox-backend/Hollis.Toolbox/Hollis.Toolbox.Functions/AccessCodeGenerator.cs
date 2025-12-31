using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hollis.Toolbox.Functions;

public class AccessCodeGenerator(uint maxAttempts = 16)
{
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

    public async Task<string> GenerateAsync(
        uint defaultLength,
        Func<string, Task<bool>> verifyAsync)
    {
        var length = defaultLength;
        var attempts = 0;

        while (attempts <= maxAttempts)
        {
            var candidate = GenerateRandomString(length);
            if (await verifyAsync(candidate))
                return candidate;

            attempts++;
            length++;
        }

        throw new InvalidOperationException("Generate AccessCode Failed, reach max attempts.");
    }

    private static string GenerateRandomString(uint length)
    {
        var result = new StringBuilder((int)length);
        var buffer = new byte[length];

        Rng.GetBytes(buffer);

        for (int i = 0; i < length; i++)
        {
            result.Append(Chars[buffer[i] % Chars.Length]);
        }
        return result.ToString();
    }
}
