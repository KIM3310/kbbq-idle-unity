using UnityEngine;
using UnityEngine.UI;
using System;

public class UpgradeRowView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Text label;
    [SerializeField] private Image background;
    [SerializeField] private Color bestBackgroundColor = new Color(0.90f, 0.62f, 0.20f, 1f);
    [SerializeField] private Color bestGlowColor = new Color(1f, 0.80f, 0.28f, 1f);
    [SerializeField] private Color bestTextColor = new Color(0.18f, 0.10f, 0.06f, 1f);
    [SerializeField] private Color bestTextGlowColor = new Color(1f, 0.93f, 0.52f, 0.65f);
    [SerializeField] private float glowSpeed = 1.8f;

    private GameManager gameManager;
    private string upgradeId;
    private Color normalBackgroundColor;
    private Color normalTextColor;
    private bool hasCachedColors;
    private bool isBestActive;
    private Shadow labelShadow;
    private UpgradeUiEntry currentEntry;
    private Action<UpgradeUiEntry> requestUpgradeAction;
    private Vector3 baseScale = Vector3.one;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (background == null)
        {
            background = GetComponent<Image>();
        }

        if (label == null)
        {
            label = GetComponentInChildren<Text>();
        }

        if (label != null)
        {
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 12;
            label.resizeTextMaxSize = 22;
        }

        CacheColors();
        if (label != null)
        {
            labelShadow = label.GetComponent<Shadow>();
            if (labelShadow == null)
            {
                labelShadow = label.gameObject.AddComponent<Shadow>();
            }
            labelShadow.enabled = false;
            labelShadow.effectDistance = new Vector2(1f, -1f);
        }
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }

        baseScale = transform.localScale;
    }

    public void Bind(GameManager manager)
    {
        gameManager = manager;
    }

    public void SetRequestUpgradeAction(Action<UpgradeUiEntry> callback)
    {
        requestUpgradeAction = callback;
    }

    public void SetData(UpgradeUiEntry entry)
    {
        currentEntry = entry;
        upgradeId = entry.id;
        if (label != null)
        {
            var costText = FormatUtil.FormatCurrency(entry.cost);
            var status = entry.affordable ? "BUY NOW" : "LOCKED";
            var bestTag = entry.isBest ? "  BEST PICK" : "";
            var badge = string.IsNullOrEmpty(entry.badgeText) ? "UPGRADE" : entry.badgeText;
            var impact = string.IsNullOrEmpty(entry.impactText) ? "Sharper kitchen flow." : entry.impactText;
            label.text = badge + " · " + entry.displayName + "  Lv." + entry.level +
                         "\n" + impact +
                         "\n" + costText + "  " + status + bestTag;
            label.color = entry.isBest ? bestTextColor : ResolveCategoryTextColor(entry.category);
            label.fontStyle = entry.isBest ? FontStyle.Bold : FontStyle.Normal;
            label.alignment = TextAnchor.MiddleCenter;
        }

        if (button != null)
        {
            button.interactable = entry.affordable;
        }

        if (background != null)
        {
            background.color = entry.isBest ? bestBackgroundColor : ResolveCategoryBackground(entry.category, entry.affordable);
        }
        if (labelShadow != null)
        {
            labelShadow.enabled = entry.isBest;
            labelShadow.effectColor = bestTextGlowColor;
        }
        isBestActive = entry.isBest;
        if (!isBestActive)
        {
            transform.localScale = baseScale;
        }
    }

    public void Clear()
    {
        currentEntry = default;
        upgradeId = null;
        if (label != null)
        {
            label.text = "";
            label.color = normalTextColor;
            label.fontStyle = FontStyle.Normal;
        }
        if (button != null)
        {
            button.interactable = false;
        }
        if (background != null)
        {
            background.color = normalBackgroundColor;
        }
        if (labelShadow != null)
        {
            labelShadow.enabled = false;
        }
        isBestActive = false;
        transform.localScale = baseScale;
    }

    private void HandleClick()
    {
        if (requestUpgradeAction != null)
        {
            requestUpgradeAction(currentEntry);
            return;
        }

        if (gameManager == null || string.IsNullOrEmpty(upgradeId))
        {
            return;
        }

        gameManager.PurchaseUpgrade(upgradeId);
    }

    private void Update()
    {
        if (!isBestActive || background == null)
        {
            return;
        }

        var t = (Mathf.Sin(Time.unscaledTime * glowSpeed) + 1f) * 0.5f;
        background.color = Color.Lerp(bestBackgroundColor, bestGlowColor, t);
        transform.localScale = baseScale * (1f + Mathf.Sin(Time.unscaledTime * (glowSpeed * 1.4f)) * 0.012f);
    }

    private void CacheColors()
    {
        if (hasCachedColors)
        {
            return;
        }

        if (background != null)
        {
            normalBackgroundColor = background.color;
        }
        else
        {
            normalBackgroundColor = Color.white;
        }

        if (label != null)
        {
            normalTextColor = label.color;
        }
        else
        {
            normalTextColor = Color.white;
        }

        hasCachedColors = true;
    }

    private Color ResolveCategoryBackground(string category, bool affordable)
    {
        var normalized = string.IsNullOrEmpty(category) ? string.Empty : category.ToLowerInvariant();
        var baseColor = normalBackgroundColor;
        switch (normalized)
        {
            case "income":
                baseColor = new Color(0.34f, 0.20f, 0.12f, 0.96f);
                break;
            case "menu":
                baseColor = new Color(0.42f, 0.18f, 0.18f, 0.96f);
                break;
            case "staff":
                baseColor = new Color(0.24f, 0.22f, 0.16f, 0.96f);
                break;
            case "service":
                baseColor = new Color(0.22f, 0.18f, 0.26f, 0.96f);
                break;
            case "sizzle":
                baseColor = new Color(0.40f, 0.16f, 0.10f, 0.96f);
                break;
        }

        return affordable ? baseColor : Color.Lerp(baseColor, new Color(0.20f, 0.20f, 0.20f, 0.92f), 0.55f);
    }

    private Color ResolveCategoryTextColor(string category)
    {
        var normalized = string.IsNullOrEmpty(category) ? string.Empty : category.ToLowerInvariant();
        switch (normalized)
        {
            case "income":
                return new Color(1f, 0.93f, 0.78f, 1f);
            case "menu":
                return new Color(1f, 0.88f, 0.84f, 1f);
            case "staff":
                return new Color(0.96f, 0.92f, 0.78f, 1f);
            case "service":
                return new Color(0.90f, 0.90f, 1f, 1f);
            case "sizzle":
                return new Color(1f, 0.86f, 0.72f, 1f);
            default:
                return normalTextColor;
        }
    }
}
