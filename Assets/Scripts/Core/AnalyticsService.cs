using System.Threading.Tasks;
using UnityEngine;

public class AnalyticsService : MonoBehaviour
{
    [SerializeField] private bool enableLogs = true;
    [SerializeField] private bool enableNetwork = true;
    [SerializeField] private NetworkService networkService;

    public void BindNetwork(NetworkService network)
    {
        networkService = network;
    }

    public void LogBoost()
    {
        LogEvent("boost_used");
    }

    public void LogUpgrade(string upgradeId, int level)
    {
        LogEvent("upgrade_purchased", "upgrade", upgradeId, "level", level.ToString());
    }

    public void LogPrestige(int points)
    {
        LogEvent("prestige", "points", points.ToString());
    }

    private void LogEvent(string eventName, params string[] kv)
    {
        if (enableLogs)
        {
            if (kv == null || kv.Length == 0)
            {
                Debug.Log("[Analytics] " + eventName);
            }
            else
            {
                var payload = string.Join(",", kv);
                Debug.Log("[Analytics] " + eventName + " {" + payload + "}");
            }
        }

        _ = TrySendNetworkEventAsync(eventName, kv);
    }

    private async Task TrySendNetworkEventAsync(string eventName, string[] kv)
    {
        if (!enableNetwork)
        {
            return;
        }

        if (networkService == null || !networkService.IsNetworkEnabled() || networkService.Analytics == null)
        {
            return;
        }

        try
        {
            await networkService.Analytics.Track(eventName, kv);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[Analytics] network track failed: " + ex.Message);
        }
    }
}
