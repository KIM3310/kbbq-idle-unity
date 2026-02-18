using UnityEngine;
using UnityEngine.UI;

public class UpgradeRowView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Text label;
    [SerializeField] private Image background;
    [SerializeField] private Color bestBackgroundColor = new Color(0.95f, 0.72f, 0.28f, 1f);
    [SerializeField] private Color bestGlowColor = new Color(1f, 0.85f, 0.4f, 1f);
    [SerializeField] private Color bestTextColor = new Color(0.18f, 0.11f, 0.08f, 1f);
    [SerializeField] private Color bestTextGlowColor = new Color(1f, 0.95f, 0.6f, 0.6f);
    [SerializeField] private float glowSpeed = 2f;
    [SerializeField] private float glowScale = 0.04f;

    private GameManager gameManager;
    private string upgradeId;
    private Color normalBackgroundColor;
    private Color normalTextColor;
    private bool hasCachedColors;
    private bool isBestActive;
    private Vector3 baseScale;
    private Shadow labelShadow;

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
            label.resizeTextMinSize = 11;
            label.resizeTextMaxSize = 20;
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
        baseScale = transform.localScale;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }
    }

    public void Bind(GameManager manager)
    {
        gameManager = manager;
    }

    public void SetData(UpgradeUiEntry entry)
    {
        upgradeId = entry.id;
        if (label != null)
        {
            var costText = FormatUtil.FormatCurrency(entry.cost);
            var status = entry.affordable ? "BUY NOW" : "SAVE UP";
            var bestTag = entry.isBest ? " [BEST]" : "";
            label.text = entry.displayName + " Lv." + entry.level + bestTag +
                         "\n" + costText + " · " + status;
            label.color = entry.isBest ? bestTextColor : normalTextColor;
            label.fontStyle = entry.isBest ? FontStyle.Bold : FontStyle.Normal;
        }

        if (button != null)
        {
            button.interactable = entry.affordable;
        }

        if (background != null)
        {
            background.color = entry.isBest ? bestBackgroundColor : normalBackgroundColor;
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
        transform.localScale = baseScale * (1f + glowScale * t);
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
}
