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
        var reviewContract = "kbbq-idle-review-pack-v1";
        var storeTier = "Alley";
        var incomePerSec = 0d;
        var monetizationMode = "Ads off / IAP off / Packs 0";
        var reviewStep = "Check grill flow, queue pressure, then ad/IAP posture.";
        var twoMinuteReview = "Health/meta -> review-pack -> grill loop -> perf overlay";
        var proofAssets = "Health, Meta, Review Pack, Perf Overlay";
        if (gameManager != null)
        {
            var metrics = gameManager.GetQueueMetrics();
            var reviewPack = gameManager.GetGameplayReviewPack();
            queueCount = metrics.queueCount;
            avgWait = metrics.avgWaitSeconds;
            servedPerMin = metrics.servedPerMinute;
            presetLabel = GetPresetLabel(gameManager.GetDebugPresetIndex());
            reviewContract = reviewPack.contract;
            storeTier = reviewPack.storeTier;
            incomePerSec = reviewPack.incomePerSecond;
            monetizationMode = reviewPack.monetizationMode;
            reviewStep = reviewPack.reviewStep;
            twoMinuteReview = reviewPack.twoMinuteReview;
            proofAssets = reviewPack.proofAssets;
        }

        overlayText.text = "FPS " + fps.ToString("0") +
                           "\nContract " + reviewContract +
                           "\nTier " + storeTier +
                           "\nQueue " + queueCount +
                           "\nServed/min " + servedPerMin.ToString("0") +
                           "\nAvg wait " + avgWait.ToString("0.0") + "s" +
                           "\nIncome/s " + FormatUtil.FormatCurrency(incomePerSec) +
                           "\nMonetize " + monetizationMode +
                           "\nPreset " + presetLabel +
                           "\nReview " + reviewStep +
                           "\n2m " + twoMinuteReview +
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
