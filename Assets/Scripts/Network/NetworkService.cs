using System.Threading.Tasks;
using UnityEngine;

public class NetworkService : MonoBehaviour
{
    [SerializeField] private ApiConfig apiConfig;

    public AuthClient Auth { get; private set; }
    public LeaderboardClient Leaderboard { get; private set; }
    public FriendsClient Friends { get; private set; }

    private void Awake()
    {
        if (apiConfig == null)
        {
            apiConfig = DefaultDataFactory.CreateApiConfig();
        }

        Auth = new AuthClient();
        Leaderboard = new LeaderboardClient();
        Friends = new FriendsClient();

        Auth.Initialize(apiConfig);
        Leaderboard.Initialize(apiConfig);
        Friends.Initialize(apiConfig);
    }

    public bool IsNetworkEnabled()
    {
        if (apiConfig == null || !apiConfig.enableNetwork)
        {
            return false;
        }

        if (Application.isEditor && !apiConfig.allowInEditor)
        {
            return false;
        }

        return !string.IsNullOrEmpty(apiConfig.baseUrl);
    }

    public async Task<bool> EnsureGuestAuth()
    {
        if (!IsNetworkEnabled())
        {
            return false;
        }

        var response = await Auth.GuestLogin();
        if (response == null)
        {
            return false;
        }

        Leaderboard.SetAuth(response.playerId, response.token);
        Friends.SetAuth(response.playerId, response.token);
        return true;
    }
}
