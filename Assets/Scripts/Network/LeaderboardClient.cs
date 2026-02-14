using System;
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
        var signaturePayload = string.Concat(playerId ?? string.Empty, "|", score, "|", timestamp);
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
