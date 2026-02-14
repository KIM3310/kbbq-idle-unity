using System;
using System.Globalization;
using System.Threading.Tasks;

public class LeaderboardClient : ApiClientBase
{
    private readonly RateLimiter submitLimiter = new RateLimiter(60);

    public async Task<bool> SubmitScore(double score)
    {
        if (score < 0 || string.IsNullOrEmpty(playerId))
        {
            return false;
        }

        if (!submitLimiter.TryConsume())
        {
            return false;
        }

        var timestamp = TimeUtil.UtcNowUnix();
        var nonce = NetworkUtils.NewNonce();
        // Deterministic signing across languages/cultures:
        // - Python may stringify floats differently (e.g., 1.0 vs "1")
        // - Some locales use "," as a decimal separator
        // For portfolio/demo purposes, we sign a rounded integer score.
        var scoreInt = (long)Math.Round(score);
        var signaturePayload = (playerId ?? string.Empty)
                               + "|"
                               + scoreInt.ToString(CultureInfo.InvariantCulture)
                               + "|"
                               + timestamp.ToString(CultureInfo.InvariantCulture);
        var signature = NetworkUtils.ComputeHmacSha256(config.hmacSecret, signaturePayload);

        var body = new ScoreSubmitRequest
        {
            playerId = playerId,
            score = score,
            signature = signature,
            timestamp = timestamp,
            nonce = nonce
        };

        await SendRequest<object>("POST", "/leaderboard/submit", body, true);
        return true;
    }

    public Task<LeaderboardResponse> FetchTop(string region, int limit)
    {
        limit = Math.Max(1, Math.Min(100, limit));
        var query = string.IsNullOrEmpty(region) ? "" : "?region=" + region + "&limit=" + limit;
        return SendRequest<LeaderboardResponse>("GET", "/leaderboard/top" + query, null, true);
    }
}
