using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeModalView : MonoBehaviour
{
    private GameManager gameManager;
    private RectTransform root;
    private RectTransform card;
    private Text titleText;
    private Text bodyText;
    private Text hintText;
    private Image previewImage;
    private Button buyButton;
    private Button closeButton;
    private UpgradeUiEntry currentEntry;
    private bool hasEntry;
    private Action onPurchased;

    private Sprite tier0Sprite;
    private Sprite tier1Sprite;
    private Sprite tier2Sprite;
    private Sprite tier3Sprite;

    private void Awake()
    {
        BuildSprites();
        BuildUi();
        Hide();
    }

    public void Bind(GameManager manager)
    {
        gameManager = manager;
    }

    public void Show(UpgradeUiEntry entry, Action purchasedCallback)
    {
        currentEntry = entry;
        hasEntry = true;
        onPurchased = purchasedCallback;
        ApplyEntry();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        hasEntry = false;
        onPurchased = null;
        gameObject.SetActive(false);
    }

    private void HandleBuy()
    {
        if (!hasEntry || gameManager == null || string.IsNullOrEmpty(currentEntry.id))
        {
            return;
        }

        if (!gameManager.PurchaseUpgrade(currentEntry.id))
        {
            if (hintText != null)
            {
                hintText.text = "Not enough funds. Keep serving and try again.";
                hintText.color = new Color(0.92f, 0.48f, 0.42f, 1f);
            }
            return;
        }

        onPurchased?.Invoke();
        Hide();
    }

    private void ApplyEntry()
    {
        if (!hasEntry)
        {
            return;
        }

        if (titleText != null)
        {
            titleText.text = currentEntry.displayName;
        }

        if (bodyText != null)
        {
            var category = string.IsNullOrEmpty(currentEntry.category) ? "General" : currentEntry.category;
            bodyText.text =
                "Level " + currentEntry.level + "\n" +
                "Cost " + FormatUtil.FormatCurrency(currentEntry.cost) + "\n" +
                "Category " + category + "\n" +
                "Efficiency " + currentEntry.score.ToString("0.000");
        }

        if (hintText != null)
        {
            hintText.text = currentEntry.affordable
                ? "Purchase applies instantly and upgrades grill visuals."
                : "Insufficient funds. Build cash flow first.";
            hintText.color = currentEntry.affordable
                ? new Color(0.90f, 0.86f, 0.74f, 0.96f)
                : new Color(0.94f, 0.68f, 0.58f, 1f);
        }

        if (buyButton != null)
        {
            buyButton.interactable = currentEntry.affordable;
        }

        if (previewImage != null)
        {
            var tier = 0;
            if (currentEntry.level >= 14)
            {
                tier = 3;
            }
            else if (currentEntry.level >= 8)
            {
                tier = 2;
            }
            else if (currentEntry.level >= 3)
            {
                tier = 1;
            }

            if (tier >= 3)
            {
                previewImage.sprite = tier3Sprite;
            }
            else if (tier == 2)
            {
                previewImage.sprite = tier2Sprite;
            }
            else if (tier == 1)
            {
                previewImage.sprite = tier1Sprite;
            }
            else
            {
                previewImage.sprite = tier0Sprite;
            }
            previewImage.color = Color.white;
        }
    }

    private void BuildUi()
    {
        root = transform as RectTransform;
        if (root == null)
        {
            return;
        }

        var dim = GetOrAdd<Image>(root.gameObject);
        dim.color = new Color(0f, 0f, 0f, 0.62f);

        card = new GameObject("ModalCard", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        card.SetParent(root, false);
        card.anchorMin = new Vector2(0.5f, 0.5f);
        card.anchorMax = new Vector2(0.5f, 0.5f);
        card.pivot = new Vector2(0.5f, 0.5f);
        card.sizeDelta = new Vector2(420f, 360f);
        card.anchoredPosition = Vector2.zero;
        var cardImage = card.GetComponent<Image>();
        cardImage.color = new Color(0.20f, 0.12f, 0.09f, 0.98f);

        titleText = CreateText("Title", card, 24, TextAnchor.UpperCenter, new Color(0.98f, 0.90f, 0.70f, 1f));
        SetRect(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(14f, -60f), new Vector2(-14f, -12f));

        previewImage = CreateImage("Preview", card, tier0Sprite, Color.white, false);
        SetRect(previewImage.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-52f, -162f), new Vector2(52f, -58f));

        bodyText = CreateText("Body", card, 14, TextAnchor.UpperLeft, new Color(0.95f, 0.90f, 0.82f, 1f));
        SetRect(bodyText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(20f, 110f), new Vector2(-20f, -164f));

        hintText = CreateText("Hint", card, 12, TextAnchor.MiddleCenter, new Color(0.90f, 0.86f, 0.74f, 0.96f));
        SetRect(hintText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(20f, 72f), new Vector2(-20f, 104f));

        buyButton = CreateButton("BuyButton", card, "BUY UPGRADE", new Color(0.72f, 0.28f, 0.18f, 1f));
        SetRect(buyButton.transform as RectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(20f, 20f), new Vector2(-20f, 62f));
        buyButton.onClick.AddListener(HandleBuy);

        closeButton = CreateButton("CloseButton", card, "CLOSE", new Color(0.40f, 0.22f, 0.16f, 1f));
        SetRect(closeButton.transform as RectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-84f, -44f), new Vector2(-20f, -10f));
        closeButton.onClick.AddListener(Hide);
    }

    private void BuildSprites()
    {
        var palette = new Dictionary<char, Color32>
        {
            ['.'] = new Color32(0, 0, 0, 0),
            ['b'] = new Color32(132, 88, 56, 255),
            ['c'] = new Color32(172, 118, 72, 255),
            ['g'] = new Color32(212, 156, 68, 255),
            ['y'] = new Color32(244, 214, 92, 255),
            ['w'] = new Color32(250, 236, 180, 255),
            ['k'] = new Color32(52, 40, 34, 255)
        };

        tier0Sprite = BuildPixelSprite("up_t0", new[]
        {
            "................",
            "....bbbbbbbb....",
            "...bbccccccbb...",
            "..bbccccccccbb..",
            "..bbccccccccbb..",
            "..bbccccccccbb..",
            "...bbccccccbb...",
            "....bbbbbbbb....",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................"
        }, palette);

        tier1Sprite = BuildPixelSprite("up_t1", new[]
        {
            "................",
            "....bbbbbbbb....",
            "...bbccggccbb...",
            "..bbccggggccbb..",
            "..bbccggggccbb..",
            "..bbccggggccbb..",
            "...bbccggccbb...",
            "....bbbbbbbb....",
            "......gggg......",
            ".....ggyygg.....",
            "......gggg......",
            "................",
            "................",
            "................",
            "................",
            "................"
        }, palette);

        tier2Sprite = BuildPixelSprite("up_t2", new[]
        {
            "................",
            "....bbbbbbbb....",
            "...bbggggggbb...",
            "..bbggyyyyggbb..",
            "..bbggyyyyggbb..",
            "..bbggyyyyggbb..",
            "...bbggggggbb...",
            "....bbbbbbbb....",
            "......yyyy......",
            ".....yywwyy.....",
            "......yyyy......",
            "................",
            "................",
            "................",
            "................",
            "................"
        }, palette);

        tier3Sprite = BuildPixelSprite("up_t3", new[]
        {
            "................",
            "....kkkkkkkk....",
            "...kkggggggkk...",
            "..kkggyyyyggkk..",
            "..kkggyyyyggkk..",
            "..kkggyyyyggkk..",
            "...kkggggggkk...",
            "....kkkkkkkk....",
            ".....yyyyyy.....",
            "....yywwwwyy....",
            ".....yyyyyy.....",
            "......wwww......",
            ".......ww.......",
            "................",
            "................",
            "................"
        }, palette);
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        var existing = go.GetComponent<T>();
        if (existing != null)
        {
            return existing;
        }
        return go.AddComponent<T>();
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private static Text CreateText(string name, RectTransform parent, int fontSize, TextAnchor anchor, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Shadow));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontStyle = FontStyle.Bold;
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.color = color;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 10;
        text.resizeTextMaxSize = fontSize + 2;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        var shadow = go.GetComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.35f);
        shadow.effectDistance = new Vector2(1f, -1f);

        return text;
    }

    private static Image CreateImage(string name, RectTransform parent, Sprite sprite, Color color, bool raycastTarget)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = raycastTarget;
        image.preserveAspect = sprite != null;
        return image;
    }

    private static Button CreateButton(string name, RectTransform parent, string label, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        var image = go.GetComponent<Image>();
        image.color = color;

        var button = go.GetComponent<Button>();
        var colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.2f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.2f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.82f);
        button.colors = colors;

        var text = CreateText("Label", rect, 14, TextAnchor.MiddleCenter, new Color(0.99f, 0.95f, 0.88f, 1f));
        text.text = label;
        SetRect(text.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(6f, 6f), new Vector2(-6f, -6f));

        return button;
    }

    private static Sprite BuildPixelSprite(string name, string[] pattern, Dictionary<char, Color32> palette)
    {
        var height = pattern != null ? pattern.Length : 1;
        var width = 1;
        if (pattern != null)
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                width = Mathf.Max(width, pattern[i] != null ? pattern[i].Length : 0);
            }
        }

        var tex = new Texture2D(width, Mathf.Max(1, height), TextureFormat.RGBA32, false);
        tex.name = name;
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < height; y++)
        {
            var row = pattern[height - 1 - y] ?? string.Empty;
            for (int x = 0; x < width; x++)
            {
                var key = x < row.Length ? row[x] : '.';
                Color32 color;
                if (!palette.TryGetValue(key, out color))
                {
                    color = new Color32(0, 0, 0, 0);
                }
                tex.SetPixel(x, y, color);
            }
        }

        tex.Apply(false, false);
        return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 16f);
    }
}
