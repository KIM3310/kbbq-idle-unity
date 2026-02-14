using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIController : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private Text currencyText;
    [SerializeField] private Text incomeText;
    [SerializeField] private Text storeTierText;
    [SerializeField] private Text prestigeText;
    [SerializeField] private Text loginRewardText;
    [SerializeField] private Text dailyMissionsText;
    [SerializeField] private Text queueText;
    [SerializeField] private Text queueMetricsText;
    [SerializeField] private Text upgradesText;
    [SerializeField] private Text debugIndicatorText;
    [SerializeField] private Text comboText;
    [SerializeField] private Button debugToggleButton;

    [Header("Meters")]
    [SerializeField] private Slider satisfactionSlider;
    [SerializeField] private Slider comboSlider;

    [Header("Views")]
    [SerializeField] private DailyMissionView dailyMissionView;
    [SerializeField] private PrestigeView prestigeView;
    [SerializeField] private QueueControlView queueControlView;
    [SerializeField] private UpgradeListView upgradeListView;
    [SerializeField] private DebugPanelView debugPanelView;
    [SerializeField] private PerfOverlayView perfOverlayView;
    [SerializeField] private TutorialView tutorialView;
    [SerializeField] private LeaderboardView leaderboardView;
    [SerializeField] private MonetizationView monetizationView;

    private GameManager gameManager;

    public void Bind(GameManager manager)
    {
        gameManager = manager;
        dailyMissionView?.Bind(manager);
        prestigeView?.Bind(manager);
        queueControlView?.Bind(manager);
        upgradeListView?.Bind(manager);
        debugPanelView?.Bind(manager);
        perfOverlayView?.Bind(manager);
        tutorialView?.Bind(manager);
        leaderboardView?.Bind(manager);
        monetizationView?.Bind(manager);
        if (debugToggleButton != null && !Application.isEditor)
        {
            debugToggleButton.gameObject.SetActive(false);
        }
        UpdateDebugIndicator();
    }

    public void UpdateEconomy(double currency, double incomePerSec)
    {
        if (currencyText != null)
        {
            currencyText.text = FormatUtil.FormatCurrency(currency);
        }

        if (incomeText != null)
        {
            incomeText.text = FormatUtil.FormatCurrency(incomePerSec) + "/s";
        }
    }

    public void UpdateStoreTier(StoreTier tier)
    {
        if (storeTierText == null)
        {
            return;
        }

        storeTierText.text = tier != null ? tier.displayName : "";
    }

    public void UpdateSatisfaction(float value)
    {
        if (satisfactionSlider != null)
        {
            satisfactionSlider.value = value;
        }
    }

    public void UpdatePrestige(int level, int points)
    {
        if (prestigeText == null)
        {
            return;
        }

        prestigeText.text = "Prestige " + level + " (+" + points + ")";
        prestigeView?.Refresh(level, points);
    }

    public void UpdateDailyMissions(IReadOnlyList<DailyMissionState> missions)
    {
        if (dailyMissionsText == null || missions == null)
        {
            dailyMissionView?.Render(missions);
            return;
        }

        var lines = new List<string>();
        foreach (var mission in missions)
        {
            if (mission == null)
            {
                continue;
            }

            var status = mission.claimed ? "Claimed" : mission.completed ? "Complete" : "In Progress";
            lines.Add(mission.type + ": " + mission.progress.ToString("0") + "/" + mission.target.ToString("0") + " [" + status + "]");
        }

        dailyMissionsText.text = string.Join("\n", lines);
        dailyMissionView?.Render(missions);
    }

    public void UpdateQueue(IReadOnlyList<CustomerQueueEntry> queue)
    {
        if (queueText == null)
        {
            return;
        }

        if (queue == null || queue.Count == 0)
        {
            queueText.text = "No customers waiting.";
            return;
        }

        var lines = new List<string>();
        var maxLines = Mathf.Min(6, queue.Count);
        for (int i = 0; i < maxLines; i++)
        {
            var entry = queue[i];
            if (entry == null)
            {
                continue;
            }

            var remaining = Mathf.Max(0f, entry.patience - entry.waitTime);
            lines.Add(entry.customerName + " - " + entry.menuName + " (" + remaining.ToString("0") + "s)");
        }

        queueText.text = string.Join("\n", lines);
    }

    public void UpdateQueueMetrics(QueueMetrics metrics)
    {
        if (queueMetricsText == null)
        {
            return;
        }

        queueMetricsText.text = "Avg wait " + metrics.avgWaitSeconds.ToString("0.0") +
                                "s\nServed/min " + metrics.servedPerMinute.ToString("0");
    }

    public void UpdateCombo(int comboCount, float comboTimeRemaining, float comboDuration, float comboMultiplier)
    {
        if (comboText != null)
        {
            if (comboCount > 0)
            {
                var bonusPct = Mathf.RoundToInt((comboMultiplier - 1f) * 100f);
                comboText.text = "Sizzle Combo x" + comboCount + " (+" + bonusPct + "%)";
            }
            else
            {
                comboText.text = "Serve fast to build combo";
            }
        }

        if (comboSlider != null)
        {
            if (comboCount > 0 && comboDuration > 0f)
            {
                comboSlider.gameObject.SetActive(true);
                comboSlider.value = Mathf.Clamp01(comboTimeRemaining / comboDuration);
            }
            else
            {
                comboSlider.value = 0f;
                comboSlider.gameObject.SetActive(false);
            }
        }
    }

    public void SetDebugPanelVisible(bool visible)
    {
        if (debugPanelView != null)
        {
            debugPanelView.gameObject.SetActive(visible);
        }
        if (!visible)
        {
            SetPerfOverlayVisible(false);
        }
        UpdateDebugIndicator();
    }

    public void SetPerfOverlayVisible(bool visible)
    {
        if (perfOverlayView != null)
        {
            perfOverlayView.gameObject.SetActive(visible);
        }
        UpdateDebugIndicator();
    }

    public bool IsDebugPanelVisible()
    {
        return debugPanelView != null && debugPanelView.gameObject.activeSelf;
    }

    public bool IsPerfOverlayVisible()
    {
        return perfOverlayView != null && perfOverlayView.gameObject.activeSelf;
    }

    public void ToggleDebugUI()
    {
        var newState = !IsDebugPanelVisible();
        SetDebugPanelVisible(newState);
        SetPerfOverlayVisible(newState);
    }

    public void SetDebugPresetIndex(int index)
    {
        debugPanelView?.SetPresetIndex(index);
    }

    public int GetDebugPresetIndex()
    {
        return debugPanelView != null ? debugPanelView.GetPresetIndex() : 1;
    }

    public void SetDebugSliderValues(float spawnValue, float serviceValue, bool markCustom)
    {
        debugPanelView?.SetSliderValues(spawnValue, serviceValue, markCustom);
    }

    public void ShowTutorial(string message)
    {
        tutorialView?.Show(message);
    }

    public void HideTutorial()
    {
        tutorialView?.Hide();
    }

    private void UpdateDebugIndicator()
    {
        if (debugIndicatorText == null)
        {
            return;
        }

        if (!Application.isEditor)
        {
            debugIndicatorText.gameObject.SetActive(false);
            return;
        }

        var show = !IsDebugPanelVisible();
        debugIndicatorText.gameObject.SetActive(show);
        if (show)
        {
            debugIndicatorText.text = "DBG OFF";
        }
    }

    public void UpdateUpgrades(IReadOnlyList<UpgradeUiEntry> upgrades)
    {
        upgradeListView?.Render(upgrades);

        if (upgradeListView != null)
        {
            return;
        }

        if (upgradesText == null)
        {
            return;
        }

        if (upgrades == null || upgrades.Count == 0)
        {
            upgradesText.text = "No upgrades available.";
            return;
        }

        var lines = new List<string>();
        var maxLines = Mathf.Min(6, upgrades.Count);
        for (int i = 0; i < maxLines; i++)
        {
            var entry = upgrades[i];
            var costText = FormatUtil.FormatCurrency(entry.cost);
            var status = entry.affordable ? "Ready" : "Locked";
            lines.Add(entry.displayName + " Lv." + entry.level + " - " + costText + " [" + status + "]");
        }

        upgradesText.text = string.Join("\n", lines);
    }

    public void ShowLoginReward(DailyLoginReward reward)
    {
        if (loginRewardText == null || !reward.granted)
        {
            return;
        }

        loginRewardText.text = "Login Day " + reward.streakDay + " +" + FormatUtil.FormatCurrency(reward.currency);
    }

    public void OnSizzleBoostClicked()
    {
        gameManager?.TriggerSizzleBoost();
    }
}
