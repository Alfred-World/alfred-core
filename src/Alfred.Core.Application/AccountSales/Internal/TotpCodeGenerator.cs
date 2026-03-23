using System.Security.Cryptography;

namespace Alfred.Core.Application.AccountSales.Internal;

internal static class TotpCodeGenerator
{
    private const int DefaultStepSeconds = 30;
    private const int Digits = 6;

    public static string? GenerateCode(string? base32Secret, DateTimeOffset timestamp)
    {
        if (string.IsNullOrWhiteSpace(base32Secret))
        {
            return null;
        }

        var key = DecodeBase32(base32Secret);
        if (key.Length == 0)
        {
            return null;
        }

        var counter = timestamp.ToUnixTimeSeconds() / DefaultStepSeconds;
        Span<byte> counterBytes = stackalloc byte[8];
        for (var i = 7; i >= 0; i--)
        {
            counterBytes[i] = (byte)(counter & 0xFF);
            counter >>= 8;
        }

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(counterBytes.ToArray());
        var offset = hash[^1] & 0x0F;
        var binaryCode = ((hash[offset] & 0x7F) << 24)
                         | ((hash[offset + 1] & 0xFF) << 16)
                         | ((hash[offset + 2] & 0xFF) << 8)
                         | (hash[offset + 3] & 0xFF);

        var otp = binaryCode % (int)Math.Pow(10, Digits);
        return otp.ToString($"D{Digits}");
    }

    private static byte[] DecodeBase32(string input)
    {
        var normalized = input.Trim().TrimEnd('=').Replace(" ", string.Empty).ToUpperInvariant();
        if (normalized.Length == 0)
        {
            return Array.Empty<byte>();
        }

        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var output = new List<byte>(normalized.Length * 5 / 8);
        var buffer = 0;
        var bitsLeft = 0;

        foreach (var c in normalized)
        {
            var val = alphabet.IndexOf(c);
            if (val < 0)
            {
                continue;
            }

            buffer = (buffer << 5) | val;
            bitsLeft += 5;

            if (bitsLeft < 8)
            {
                continue;
            }

            output.Add((byte)((buffer >> (bitsLeft - 8)) & 0xFF));
            bitsLeft -= 8;
        }

        return output.ToArray();
    }
}
