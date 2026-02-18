using UnityEngine;
using UnityEngine.UI;

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
            var status = entry.affordable ? "BUY NOW" : "LOCKED";
            var bestTag = entry.isBest ? "  BEST PICK" : "";
            label.text = entry.displayName + "  Lv." + entry.level +
                         "\n" + costText + "  " + status + bestTag;
            label.color = entry.isBest ? bestTextColor : normalTextColor;
            label.fontStyle = entry.isBest ? FontStyle.Bold : FontStyle.Normal;
            label.alignment = TextAnchor.MiddleCenter;
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
            transform.localScale = Vector3.one;
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
        transform.localScale = Vector3.one;
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
        transform.localScale = Vector3.one;
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
