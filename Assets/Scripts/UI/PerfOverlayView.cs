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
        if (gameManager != null)
        {
            var metrics = gameManager.GetQueueMetrics();
            queueCount = metrics.queueCount;
            avgWait = metrics.avgWaitSeconds;
            servedPerMin = metrics.servedPerMinute;
            presetLabel = GetPresetLabel(gameManager.GetDebugPresetIndex());
        }

        overlayText.text = "FPS " + fps.ToString("0") +
                           "\nQueue " + queueCount +
                           "\nServed/min " + servedPerMin.ToString("0") +
                           "\nAvg wait " + avgWait.ToString("0.0") + "s" +
                           "\nPreset " + presetLabel;
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
