using System.Threading.Tasks;

public class AnalyticsClient : ApiClientBase
{
    private readonly RateLimiter trackLimiter = new RateLimiter(120);

    public async Task<bool> Track(string eventName, params string[] kv)
    {
        if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(playerId))
        {
            return false;
        }

        if (!trackLimiter.TryConsume())
        {
            return false;
        }

        var timestamp = TimeUtil.UtcNowUnix();
        var nonce = NetworkUtils.NewNonce();

        var body = new AnalyticsEventRequest
        {
            playerId = playerId,
            eventName = eventName,
            kv = kv != null ? kv : new string[0],
            timestamp = timestamp,
            nonce = nonce
        };

        await SendRequest<object>("POST", "/analytics/event", body, true);
        return true;
    }
}

