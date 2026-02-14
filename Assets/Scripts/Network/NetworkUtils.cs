using System;
using System.Security.Cryptography;
using System.Text;

public struct SignedHeaders
{
    public string nonce;
    public long timestamp;
    public string signature;
}

public static class NetworkUtils
{
    public static SignedHeaders BuildSignedHeaders(string playerId, string bodyJson, string secret)
    {
        var nonce = Guid.NewGuid().ToString("N");
        var timestamp = TimeUtil.UtcNowUnix();
        var payload = string.Concat(playerId ?? string.Empty, "|", nonce, "|", timestamp, "|", bodyJson ?? string.Empty);
        var signature = ComputeHmacSha256(secret, payload);
        return new SignedHeaders { nonce = nonce, timestamp = timestamp, signature = signature };
    }

    public static string ComputeHmacSha256(string secret, string payload)
    {
        if (string.IsNullOrEmpty(secret))
        {
            return string.Empty;
        }

        var key = Encoding.UTF8.GetBytes(secret);
        var data = Encoding.UTF8.GetBytes(payload ?? string.Empty);
        using (var hmac = new HMACSHA256(key))
        {
            var hash = hmac.ComputeHash(data);
            return Convert.ToBase64String(hash);
        }
    }

    public static string NewNonce()
    {
        return Guid.NewGuid().ToString("N");
    }
}
