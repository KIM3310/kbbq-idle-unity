using System.Threading.Tasks;

public class FriendsClient : ApiClientBase
{
    public async Task<bool> Invite(string code)
    {
        if (string.IsNullOrEmpty(code) || code.Length < 4)
        {
            return false;
        }

        var timestamp = TimeUtil.UtcNowUnix();
        var nonce = NetworkUtils.NewNonce();
        var signaturePayload = string.Concat(playerId ?? string.Empty, "|", code ?? string.Empty, "|", timestamp);
        var signature = NetworkUtils.ComputeHmacSha256(config.hmacSecret, signaturePayload);

        var body = new FriendInviteRequest
        {
            playerId = playerId,
            code = code,
            signature = signature,
            timestamp = timestamp,
            nonce = nonce
        };

        await SendRequest<object>("POST", "/friends/invite", body, true);
        return true;
    }

    public Task<FriendListResponse> List()
    {
        return SendRequest<FriendListResponse>("GET", "/friends/list", null, true);
    }
}
