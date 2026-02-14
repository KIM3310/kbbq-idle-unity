using UnityEngine;

public class AnalyticsService : MonoBehaviour
{
    [SerializeField] private bool enableLogs = true;

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
        if (!enableLogs)
        {
            return;
        }

        if (kv == null || kv.Length == 0)
        {
            Debug.Log("[Analytics] " + eventName);
            return;
        }

        var payload = string.Join(",", kv);
        Debug.Log("[Analytics] " + eventName + " {" + payload + "}");
    }
}
