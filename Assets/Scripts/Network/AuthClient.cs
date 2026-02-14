using System.Threading.Tasks;
using UnityEngine;

public class AuthClient : ApiClientBase
{
    public async Task<AuthResponse> GuestLogin()
    {
        var payload = new GuestAuthRequest
        {
            deviceId = SystemInfo.deviceUniqueIdentifier
        };

        var response = await SendRequest<AuthResponse>("POST", "/auth/guest", payload, false);
        if (response != null)
        {
            SetAuth(response.playerId, response.token);
        }
        return response;
    }

    [System.Serializable]
    private class GuestAuthRequest
    {
        public string deviceId;
    }
}
