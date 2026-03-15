using UnityEngine;
using UnityEngine.UI;

public class PrestigeView : MonoBehaviour
{
    [SerializeField] private Text prestigeInfoText;
    [SerializeField] private Button prestigeButton;
    [SerializeField] private Text prestigeButtonText;

    private GameManager gameManager;
    private Image panelImage;

    public void Bind(GameManager manager)
    {
        gameManager = manager;
        if (panelImage == null)
        {
            panelImage = GetComponent<Image>();
        }
        if (prestigeButton != null && prestigeButtonText == null)
        {
            prestigeButtonText = prestigeButton.GetComponentInChildren<Text>(true);
        }
    }

    public void Refresh(int level, int points, string hint)
    {
        var canPrestige = gameManager != null && gameManager.CanPrestige();
        var progress = gameManager != null ? Mathf.RoundToInt(gameManager.GetPrestigeProgress01() * 100f) : 0;
        if (prestigeInfoText != null)
        {
            var suffix = string.IsNullOrEmpty(hint) ? string.Empty : "\n" + hint;
            var momentumLine = canPrestige
                ? "Season finale ready. Cash out now for a hotter relaunch."
                : progress >= 70
                    ? "The next run is almost ready. Push sales and level together."
                    : "Build district clout and total sales to unlock the next legendary restart.";
            prestigeInfoText.text =
                "SPICE LEGACY\n" +
                "Prestige " + level + " (+" + points + ")\n" +
                "Progress " + progress + "%\n" +
                momentumLine +
                suffix;
            prestigeInfoText.color = canPrestige
                ? new Color(1f, 0.90f, 0.54f, 1f)
                : new Color(0.96f, 0.92f, 0.82f, 0.98f);
            prestigeInfoText.fontStyle = FontStyle.Bold;
        }

        if (prestigeButton != null)
        {
            prestigeButton.interactable = canPrestige;
            var buttonImage = prestigeButton.targetGraphic as Image;
            if (buttonImage != null)
            {
                buttonImage.color = canPrestige
                    ? new Color(0.72f, 0.38f, 0.18f, 1f)
                    : new Color(0.34f, 0.22f, 0.16f, 0.92f);
            }
        }

        if (prestigeButtonText != null)
        {
            prestigeButtonText.text = canPrestige ? "OPEN NEW SEASON" : "BUILD LEGEND";
            prestigeButtonText.fontStyle = FontStyle.Bold;
        }

        if (panelImage != null)
        {
            panelImage.color = canPrestige
                ? new Color(0.44f, 0.22f, 0.10f, 0.94f)
                : new Color(0.24f, 0.16f, 0.10f, 0.92f);
        }
    }

    public void OnPrestigeClicked()
    {
        gameManager?.TryPrestige();
    }
}
