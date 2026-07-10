using UnityEngine;
using UnityEngine.UI;

public class PerfOverlayView : MonoBehaviour
{
    [SerializeField] private Text overlayText;
    [SerializeField] private float updateInterval = 0.5f;

    private GameManager gameManager;
    private float elapsed;
    private int frames;

    public void Bind(GameManager manager)
    {
        gameManager = manager;
    }

    private void Update()
    {
        frames++;
        elapsed += Time.unscaledDeltaTime;
        if (elapsed < updateInterval)
        {
            return;
        }

        var fps = frames / Mathf.Max(0.0001f, elapsed);
        frames = 0;
        elapsed = 0f;

        if (overlayText == null)
        {
            return;
        }

        var queueCount = 0;
        var avgWait = 0f;
        var servedPerMin = 0f;
        var presetLabel = "1.0x";
        var architectureContract = "kbbq-idle-architecture-pack-v1";
        var storeTier = "Alley";
        var incomePerSec = 0d;
        var optionalEconomyMode = "Optional economy off / Packs off / Packs 0";
        var architectureStep = "Check grill flow, queue pressure, then optional economy posture.";
        var focusedRoute = "Review Pack -> preset 2.0x rush -> grill loop -> perf overlay";
        var architectureSnapshot = "Tier Alley / Queue 0 / Economy off / Packs off / Packs 0";
        var focusedOpsSnapshot = "Preset 1.0x / Queue 0 / Wait 0.0s / Served 0/min";
        var twoMinuteArchitecture = "Health/meta -> architecture-pack -> grill loop -> perf overlay";
        var architectureRoutes = "Health, Meta, Review Pack, Rush Preset, Perf Overlay";
        var proofAssets = "Health, Meta, Review Pack, Perf Overlay";
        if (gameManager != null)
        {
            var metrics = gameManager.GetQueueMetrics();
            var architecturePack = gameManager.GetGameplayArchitecturePack();
            queueCount = metrics.queueCount;
            avgWait = metrics.avgWaitSeconds;
            servedPerMin = metrics.servedPerMinute;
            presetLabel = GetPresetLabel(gameManager.GetDebugPresetIndex());
            architectureContract = architecturePack.contract;
            storeTier = architecturePack.storeTier;
            incomePerSec = architecturePack.incomePerSecond;
            optionalEconomyMode = architecturePack.optionalEconomyMode;
            architectureStep = architecturePack.architectureStep;
            focusedRoute = architecturePack.focusedRoute;
            architectureSnapshot = architecturePack.architectureSnapshot;
            focusedOpsSnapshot = architecturePack.focusedOpsSnapshot;
            twoMinuteArchitecture = architecturePack.twoMinuteArchitecture;
            architectureRoutes = architecturePack.architectureRoutes;
            proofAssets = architecturePack.proofAssets;
        }

        overlayText.text = "FPS " + fps.ToString("0") +
                           "\nContract " + architectureContract +
                           "\nTier " + storeTier +
                           "\nQueue " + queueCount +
                           "\nServed/min " + servedPerMin.ToString("0") +
                           "\nAvg wait " + avgWait.ToString("0.0") + "s" +
                           "\nIncome/s " + FormatUtil.FormatCurrency(incomePerSec) +
                           "\nEconomy " + optionalEconomyMode +
                           "\nPreset " + presetLabel +
                           "\nReview " + architectureStep +
                           "\nRoute " + focusedRoute +
                           "\nSnapshot " + architectureSnapshot +
                           "\nOps " + focusedOpsSnapshot +
                           "\n2m " + twoMinuteArchitecture +
                           "\nPaths " + architectureRoutes +
                           "\nProof " + proofAssets;
    }

    private string GetPresetLabel(int index)
    {
        switch (index)
        {
            case 0:
                return "0.5x";
            case 1:
                return "1.0x";
            case 2:
                return "2.0x";
            case 3:
                return "Custom";
            default:
                return "1.0x";
        }
    }
}
