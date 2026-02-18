using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class GrillStationView : MonoBehaviour
{
    private const float RefreshInterval = 0.15f;
    private const float SlotFxDuration = 0.35f;
    private const float SlotPixelSize = 152f;
    private const float SlotGrillWidth = 188f;
    private const float SlotGrillHeight = 158f;

    private GameManager gameManager;
    private RectTransform root;
    private Text selectionText;
    private Text inventoryText;
    private Text statusText;
    private Button cycleButton;
    private Button buyButton;
    private readonly Button[] slotButtons = new Button[2];
    private readonly Text[] slotTexts = new Text[2];
    private readonly Image[] slotGrillImages = new Image[2];
    private readonly Image[] slotPixelImages = new Image[2];
    private readonly Image[] slotSmokeImages = new Image[2];
    private readonly Image[] slotFxImages = new Image[2];
    private readonly Image[] slotProgressBacks = new Image[2];
    private readonly Image[] slotProgressFills = new Image[2];
    private readonly float[] slotFxTimers = new float[2];
    private readonly List<MeatInventoryUiEntry> meats = new List<MeatInventoryUiEntry>();
    private Sprite pixelEmptySprite;
    private Sprite pixelRawSprite;
    private Sprite pixelFlippedSprite;
    private Sprite pixelReadySprite;
    private Sprite pixelBurnedSprite;
    private Sprite pixelSparkSprite;
    private Sprite pixelGrillSprite;
    private Sprite pixelSmokeSprite;

    private int selectedIndex;
    private float refreshTimer;
    private float messageTimer;
    private string transientMessage;
    private bool uiBuilt;

    private void Awake()
    {
        BuildRuntimeUi();
    }

    private void Update()
    {
        if (!uiBuilt)
        {
            return;
        }

        refreshTimer -= Time.unscaledDeltaTime;
        if (refreshTimer <= 0f)
        {
            Refresh();
            refreshTimer = RefreshInterval;
        }

        if (messageTimer > 0f)
        {
            messageTimer -= Time.unscaledDeltaTime;
            if (messageTimer <= 0f)
            {
                transientMessage = null;
                UpdateStatusText();
            }
        }

        UpdateSlotFx(Time.unscaledDeltaTime);
    }

    public void Bind(GameManager manager)
    {
        gameManager = manager;
        selectedIndex = 0;
        Refresh();
    }

    public void Refresh()
    {
        if (!uiBuilt || gameManager == null)
        {
            return;
        }

        meats.Clear();
        meats.AddRange(gameManager.GetMeatInventoryUiEntries());
        if (meats.Count == 0)
        {
            selectedIndex = 0;
        }
        else
        {
            selectedIndex = Mathf.Clamp(selectedIndex, 0, meats.Count - 1);
        }

        UpdateSelectionText();
        UpdateInventoryText();
        UpdateSlotVisual(0);
        UpdateSlotVisual(1);
        UpdateStatusText();
    }

    public void ShowMessage(string message, float duration = 2f)
    {
        transientMessage = string.IsNullOrEmpty(message) ? null : message;
        messageTimer = Mathf.Max(0.2f, duration);
        UpdateStatusText();
    }

    private void BuildRuntimeUi()
    {
        if (uiBuilt)
        {
            return;
        }

        root = transform as RectTransform;
        if (root == null)
        {
            return;
        }

        HideLegacyText("GrillLabel");
        HideLegacyText("GrillHint");
        BuildPixelSprites();

        var hud = new GameObject("RuntimeGrillHud", typeof(RectTransform), typeof(Image));
        var hudRect = hud.GetComponent<RectTransform>();
        hudRect.SetParent(root, false);
        SetRect(hudRect, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(18f, 18f), new Vector2(-18f, -18f));
        var hudImage = hud.GetComponent<Image>();
        hudImage.color = new Color(0.10f, 0.06f, 0.05f, 0.40f);

        selectionText = CreateText("SelectionText", hudRect, 18, TextAnchor.MiddleLeft, new Color(0.98f, 0.93f, 0.82f, 1f));
        SetRect(selectionText.rectTransform, new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(8f, -72f), new Vector2(-6f, -12f));

        cycleButton = CreateButton("CycleMeatButton", hudRect, "NEXT CUT", new Color(0.58f, 0.25f, 0.16f, 1f));
        SetRect(cycleButton.transform as RectTransform, new Vector2(0.5f, 1f), new Vector2(0.74f, 1f), new Vector2(6f, -72f), new Vector2(-6f, -12f));
        cycleButton.onClick.AddListener(CycleSelectedMeat);

        buyButton = CreateButton("BuyMeatButton", hudRect, "BUY +1", new Color(0.70f, 0.30f, 0.18f, 1f));
        SetRect(buyButton.transform as RectTransform, new Vector2(0.74f, 1f), new Vector2(1f, 1f), new Vector2(6f, -72f), new Vector2(-8f, -12f));
        buyButton.onClick.AddListener(BuySelectedMeat);

        inventoryText = CreateText("InventoryText", hudRect, 14, TextAnchor.UpperLeft, new Color(0.94f, 0.90f, 0.78f, 1f));
        SetRect(inventoryText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(8f, -150f), new Vector2(-8f, -76f));

        BuildSlotUi(0, hudRect, "GrillSlotAButton", new Vector2(0f, 0f), new Vector2(0.5f, 1f), new Vector2(8f, 120f), new Vector2(-6f, -162f));
        BuildSlotUi(1, hudRect, "GrillSlotBButton", new Vector2(0.5f, 0f), new Vector2(1f, 1f), new Vector2(6f, 120f), new Vector2(-8f, -162f));

        statusText = CreateText("StatusText", hudRect, 14, TextAnchor.MiddleCenter, new Color(0.98f, 0.93f, 0.82f, 1f));
        SetRect(statusText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(8f, 12f), new Vector2(-8f, 104f));

        uiBuilt = true;
    }

    private void BuildSlotUi(int slotIndex, RectTransform parent, string buttonName, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        if (slotIndex < 0 || slotIndex >= slotButtons.Length || parent == null)
        {
            return;
        }

        slotButtons[slotIndex] = CreateButton(buttonName, parent, string.Empty, new Color(0.42f, 0.18f, 0.12f, 0.95f));
        var slotRect = slotButtons[slotIndex].transform as RectTransform;
        SetRect(slotRect, anchorMin, anchorMax, offsetMin, offsetMax);
        slotButtons[slotIndex].onClick.AddListener(() => HandleSlotAction(slotIndex));

        slotGrillImages[slotIndex] = CreatePixelImage("GrillSlot" + slotIndex + "Base", slotRect, pixelGrillSprite);
        SetPixelRect(slotGrillImages[slotIndex], 0.5f, 0.56f, SlotGrillWidth, SlotGrillHeight);
        slotGrillImages[slotIndex].color = new Color(0.82f, 0.73f, 0.63f, 0.80f);

        slotPixelImages[slotIndex] = CreatePixelImage("GrillSlot" + slotIndex + "Meat", slotRect, pixelEmptySprite);
        SetPixelRect(slotPixelImages[slotIndex], 0.5f, 0.56f, SlotPixelSize, SlotPixelSize);

        slotSmokeImages[slotIndex] = CreatePixelImage("GrillSlot" + slotIndex + "Smoke", slotRect, pixelSmokeSprite);
        SetPixelRect(slotSmokeImages[slotIndex], 0.5f, 0.74f, 138f, 100f);
        slotSmokeImages[slotIndex].color = new Color(0.93f, 0.86f, 0.71f, 0f);
        slotSmokeImages[slotIndex].gameObject.SetActive(false);

        slotFxImages[slotIndex] = CreatePixelImage("GrillSlot" + slotIndex + "Fx", slotRect, pixelSparkSprite);
        SetPixelRect(slotFxImages[slotIndex], 0.5f, 0.58f, 126f, 126f);
        slotFxImages[slotIndex].gameObject.SetActive(false);

        slotProgressBacks[slotIndex] = CreateImage("GrillSlot" + slotIndex + "ProgressBack", slotRect, new Color(0.19f, 0.10f, 0.08f, 0.88f), false);
        SetRect(slotProgressBacks[slotIndex].rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -38f), new Vector2(-18f, -24f));

        slotProgressFills[slotIndex] = CreateImage("GrillSlot" + slotIndex + "ProgressFill", slotRect, new Color(0.78f, 0.25f, 0.18f, 0.96f), false);
        SetRect(slotProgressFills[slotIndex].rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -38f), new Vector2(-18f, -24f));
        slotProgressFills[slotIndex].type = Image.Type.Filled;
        slotProgressFills[slotIndex].fillMethod = Image.FillMethod.Horizontal;
        slotProgressFills[slotIndex].fillOrigin = (int)Image.OriginHorizontal.Left;
        slotProgressFills[slotIndex].fillAmount = 0f;

        slotTexts[slotIndex] = slotButtons[slotIndex].GetComponentInChildren<Text>(true);
        if (slotTexts[slotIndex] != null)
        {
            slotTexts[slotIndex].fontSize = 14;
            slotTexts[slotIndex].resizeTextForBestFit = true;
            slotTexts[slotIndex].resizeTextMinSize = 10;
            slotTexts[slotIndex].resizeTextMaxSize = 14;
            slotTexts[slotIndex].alignment = TextAnchor.UpperCenter;
            slotTexts[slotIndex].lineSpacing = 1.06f;
            slotTexts[slotIndex].color = new Color(0.99f, 0.94f, 0.86f, 1f);
            SetRect(slotTexts[slotIndex].rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(8f, 8f), new Vector2(-8f, 66f));
        }
    }

    private void HideLegacyText(string name)
    {
        var rect = FindByName(root, name);
        if (rect != null)
        {
            rect.gameObject.SetActive(false);
        }
    }

    private void CycleSelectedMeat()
    {
        if (meats.Count <= 1)
        {
            return;
        }

        selectedIndex = (selectedIndex + 1) % meats.Count;
        ShowMessage("Selected " + meats[selectedIndex].displayName + ".");
        Refresh();
    }

    private void BuySelectedMeat()
    {
        if (gameManager == null || meats.Count == 0)
        {
            return;
        }

        var selected = meats[selectedIndex];
        gameManager.BuyRawMeat(selected.menuId, 1);
        Refresh();
    }

    private void HandleSlotAction(int slotIndex)
    {
        if (gameManager == null)
        {
            return;
        }

        var slot = gameManager.GetGrillSlotUiState(slotIndex);
        if (!slot.occupied)
        {
            if (meats.Count == 0)
            {
                ShowMessage("No unlocked cut to grill yet.");
                return;
            }

            var selected = meats[selectedIndex];
            if (gameManager.PlaceRawMeatOnGrill(slotIndex, selected.menuId))
            {
                TriggerSlotFx(slotIndex, new Color(1f, 0.70f, 0.32f, 0.75f));
            }
            Refresh();
            return;
        }

        if (slot.readyToCollect || slot.burned)
        {
            if (gameManager.CollectFromGrill(slotIndex))
            {
                var fxColor = slot.burned ? new Color(0.24f, 0.10f, 0.10f, 0.75f) : new Color(0.98f, 0.82f, 0.42f, 0.75f);
                TriggerSlotFx(slotIndex, fxColor);
            }
            Refresh();
            return;
        }

        if (slot.canFlip)
        {
            if (gameManager.FlipMeat(slotIndex))
            {
                TriggerSlotFx(slotIndex, new Color(1f, 0.65f, 0.30f, 0.72f));
            }
            Refresh();
            return;
        }

        ShowMessage("Keep grilling " + slot.displayName + ".");
    }

    private void UpdateSelectionText()
    {
        if (selectionText == null)
        {
            return;
        }

        if (meats.Count == 0)
        {
            selectionText.text = "CUT: NONE";
            return;
        }

        var selected = meats[selectedIndex];
        selectionText.text = "CUT: " + selected.displayName;
    }

    private void UpdateInventoryText()
    {
        if (inventoryText == null)
        {
            return;
        }

        if (meats.Count == 0)
        {
            inventoryText.text = "Unlock menu items to start meat trading.";
            return;
        }

        var lines = new StringBuilder();
        var max = Mathf.Min(3, meats.Count);
        for (int i = 0; i < max; i++)
        {
            var entry = meats[(selectedIndex + i) % meats.Count];
            if (i > 0)
            {
                lines.Append('\n');
            }

            lines.Append(i == 0 ? "> " : "  ");
            lines.Append(entry.displayName);
            lines.Append("  R");
            lines.Append(entry.rawCount);
            lines.Append("  C");
            lines.Append(entry.cookedCount);
            lines.Append("  BUY ");
            lines.Append(FormatUtil.FormatCurrency(entry.buyCost));
        }

        inventoryText.text = lines.ToString();
    }

    private void UpdateSlotVisual(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotButtons.Length)
        {
            return;
        }

        var button = slotButtons[slotIndex];
        var label = slotTexts[slotIndex];
        if (button == null || label == null || gameManager == null)
        {
            return;
        }

        var slot = gameManager.GetGrillSlotUiState(slotIndex);
        if (!slot.occupied)
        {
            label.text = "GRILL " + (slotIndex + 1) + "\nLOAD CUT";
            TintButton(button, new Color(0.29f, 0.12f, 0.09f, 0.92f));
            SetSlotGrillTint(slotIndex, new Color(0.72f, 0.66f, 0.58f, 0.70f));
            SetSlotPixelVisual(slotIndex, pixelEmptySprite, new Color(0.96f, 0.96f, 0.96f, 0.95f), 1.02f);
            UpdateSlotProgress(slotIndex, 0f, new Color(0.34f, 0.18f, 0.12f, 0.9f));
            UpdateSlotSmoke(slotIndex, 0f, false);
            return;
        }

        var progress = Mathf.Clamp01(slot.cookProgress01);

        if (slot.burned)
        {
            label.text = slot.displayName + "\nBURNT\nDISCARD";
            TintButton(button, new Color(0.20f, 0.08f, 0.07f, 0.96f));
            SetSlotGrillTint(slotIndex, new Color(0.48f, 0.41f, 0.36f, 0.85f));
            SetSlotPixelVisual(slotIndex, pixelBurnedSprite, new Color(0.82f, 0.74f, 0.74f, 1f), 1.06f);
            UpdateSlotProgress(slotIndex, 1f, new Color(0.34f, 0.18f, 0.16f, 0.96f));
            UpdateSlotSmoke(slotIndex, 1f, true);
            return;
        }

        if (slot.readyToCollect)
        {
            label.text = slot.displayName + "\nREADY\nCOLLECT";
            TintButton(button, new Color(0.64f, 0.31f, 0.18f, 0.98f));
            SetSlotGrillTint(slotIndex, new Color(0.82f, 0.74f, 0.58f, 0.86f));
            var pulse = 0.95f + Mathf.Sin(Time.unscaledTime * 8.5f) * 0.09f;
            SetSlotPixelVisual(slotIndex, pixelReadySprite, new Color(1f, 0.96f, 0.86f, 1f), pulse);
            UpdateSlotProgress(slotIndex, 1f, new Color(0.95f, 0.60f, 0.18f, 1f));
            UpdateSlotSmoke(slotIndex, 0.75f, false);
            return;
        }

        var percent = Mathf.RoundToInt(progress * 100f);
        if (slot.canFlip)
        {
            label.text = slot.displayName + "\n" + percent + "%\nFLIP NOW";
            TintButton(button, new Color(0.54f, 0.22f, 0.14f, 0.96f));
            SetSlotGrillTint(slotIndex, new Color(0.80f, 0.70f, 0.56f, 0.84f));
            var pulse = 0.96f + Mathf.Sin(Time.unscaledTime * 7.5f) * 0.06f;
            SetSlotPixelVisual(slotIndex, pixelRawSprite, new Color(1f, 0.92f, 0.84f, 1f), pulse);
            UpdateSlotProgress(slotIndex, progress, new Color(0.95f, 0.50f, 0.20f, 0.98f));
            UpdateSlotSmoke(slotIndex, 0.48f, false);
            return;
        }

        var phase = slot.flipped ? "BACK SIDE" : "FRONT SIDE";
        label.text = slot.displayName + "\n" + phase + "\n" + percent + "%";
        TintButton(button, new Color(0.42f, 0.18f, 0.12f, 0.95f));
        SetSlotGrillTint(slotIndex, new Color(0.76f, 0.67f, 0.54f, 0.82f));
        SetSlotPixelVisual(
            slotIndex,
            slot.flipped ? pixelFlippedSprite : pixelRawSprite,
            slot.flipped ? new Color(0.97f, 0.89f, 0.80f, 1f) : new Color(1f, 0.92f, 0.88f, 1f),
            1f);
        UpdateSlotProgress(slotIndex, progress, new Color(0.84f, 0.34f, 0.18f, 0.97f));
        UpdateSlotSmoke(slotIndex, 0.25f + (progress * 0.45f), false);
    }

    private void UpdateStatusText()
    {
        if (statusText == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(transientMessage))
        {
            statusText.text = transientMessage;
            return;
        }

        statusText.text = "Flow: Buy raw -> Grill -> Flip -> Collect -> Serve";
    }

    private void SetSlotPixelVisual(int slotIndex, Sprite sprite, Color color, float scale)
    {
        if (slotIndex < 0 || slotIndex >= slotPixelImages.Length)
        {
            return;
        }

        var image = slotPixelImages[slotIndex];
        if (image == null)
        {
            return;
        }

        if (sprite != null)
        {
            image.sprite = sprite;
        }

        image.color = color;
        image.rectTransform.localScale = new Vector3(scale, scale, 1f);
        var bob = Mathf.Sin(Time.unscaledTime * 2.5f + slotIndex) * 2f;
        image.rectTransform.anchoredPosition = new Vector2(0f, bob);
    }

    private void SetSlotGrillTint(int slotIndex, Color color)
    {
        if (slotIndex < 0 || slotIndex >= slotGrillImages.Length)
        {
            return;
        }

        var image = slotGrillImages[slotIndex];
        if (image == null)
        {
            return;
        }

        image.color = color;
    }

    private void UpdateSlotProgress(int slotIndex, float progress01, Color fillColor)
    {
        if (slotIndex < 0 || slotIndex >= slotProgressFills.Length)
        {
            return;
        }

        var fill = slotProgressFills[slotIndex];
        if (fill != null)
        {
            fill.fillAmount = Mathf.Clamp01(progress01);
            fill.color = fillColor;
        }

        var back = slotProgressBacks[slotIndex];
        if (back != null)
        {
            back.color = new Color(0.18f, 0.10f, 0.08f, 0.88f);
        }
    }

    private void UpdateSlotSmoke(int slotIndex, float intensity, bool burned)
    {
        if (slotIndex < 0 || slotIndex >= slotSmokeImages.Length)
        {
            return;
        }

        var smoke = slotSmokeImages[slotIndex];
        if (smoke == null)
        {
            return;
        }

        var n = Mathf.Clamp01(intensity);
        if (n <= 0.01f)
        {
            smoke.gameObject.SetActive(false);
            return;
        }

        smoke.gameObject.SetActive(true);
        var pulse = 0.90f + Mathf.Sin((Time.unscaledTime * 2.2f) + (slotIndex * 0.4f)) * 0.08f;
        smoke.rectTransform.localScale = new Vector3(pulse, pulse, 1f);
        smoke.rectTransform.anchoredPosition = new Vector2(0f, Mathf.Lerp(8f, 18f, n));
        smoke.color = burned
            ? new Color(0.44f, 0.40f, 0.38f, Mathf.Lerp(0.12f, 0.58f, n))
            : new Color(0.94f, 0.86f, 0.70f, Mathf.Lerp(0.06f, 0.44f, n));
    }

    private void TriggerSlotFx(int slotIndex, Color color)
    {
        if (slotIndex < 0 || slotIndex >= slotFxImages.Length)
        {
            return;
        }

        var fx = slotFxImages[slotIndex];
        if (fx == null)
        {
            return;
        }

        fx.color = color;
        fx.rectTransform.localScale = new Vector3(0.65f, 0.65f, 1f);
        fx.gameObject.SetActive(true);
        slotFxTimers[slotIndex] = SlotFxDuration;
    }

    private void UpdateSlotFx(float dt)
    {
        if (dt <= 0f)
        {
            return;
        }

        for (int i = 0; i < slotFxTimers.Length; i++)
        {
            if (slotFxTimers[i] <= 0f)
            {
                continue;
            }

            var fx = slotFxImages[i];
            if (fx == null)
            {
                slotFxTimers[i] = 0f;
                continue;
            }

            slotFxTimers[i] -= dt;
            var t = Mathf.Clamp01(1f - (slotFxTimers[i] / SlotFxDuration));
            var alpha = Mathf.Lerp(0.82f, 0f, t);
            var scale = Mathf.Lerp(0.65f, 1.25f, t);

            var c = fx.color;
            c.a = alpha;
            fx.color = c;
            fx.rectTransform.localScale = new Vector3(scale, scale, 1f);

            if (slotFxTimers[i] <= 0f)
            {
                fx.gameObject.SetActive(false);
            }
        }
    }

    private void BuildPixelSprites()
    {
        if (pixelRawSprite != null)
        {
            return;
        }

        var palette = new Dictionary<char, Color32>
        {
            ['.'] = new Color32(0, 0, 0, 0),
            ['r'] = new Color32(192, 58, 54, 255),
            ['p'] = new Color32(242, 160, 150, 255),
            ['b'] = new Color32(135, 80, 52, 255),
            ['c'] = new Color32(174, 102, 60, 255),
            ['g'] = new Color32(210, 136, 44, 255),
            ['y'] = new Color32(247, 204, 88, 255),
            ['k'] = new Color32(38, 28, 24, 255),
            ['d'] = new Color32(76, 55, 45, 255),
            ['='] = new Color32(136, 118, 105, 255),
            ['#'] = new Color32(70, 58, 52, 255),
            ['w'] = new Color32(250, 236, 177, 255),
            ['m'] = new Color32(216, 201, 174, 255),
            ['s'] = new Color32(176, 156, 130, 255),
        };

        pixelEmptySprite = BuildPixelSprite("pixel_empty", new[]
        {
            "................",
            "..============..",
            "..=..==..==..=..",
            "..=..==..==..=..",
            "..=..........=..",
            "..=..==..==..=..",
            "..=..==..==..=..",
            "..=..........=..",
            "..=..==..==..=..",
            "..=..==..==..=..",
            "..=..........=..",
            "..============..",
            "................",
            "................",
            "................",
            "................"
        }, palette);

        pixelRawSprite = BuildPixelSprite("pixel_raw", new[]
        {
            "................",
            "....rrrrrrrr....",
            "...rrpppppprr...",
            "..rrpppppppprr..",
            "..rrpppppppprr..",
            "..rrpppppppprr..",
            "..rrrpppppprrr..",
            "...rrrpppprrr...",
            "...rrrrrrrrrr...",
            "....rrrpprrr....",
            ".....rrrrrr.....",
            "......yyyy......",
            ".....yyyyyy.....",
            "......yyyy......",
            "................",
            "................"
        }, palette);

        pixelFlippedSprite = BuildPixelSprite("pixel_flipped", new[]
        {
            "................",
            "....bbbbbbbb....",
            "...bbccccccbb...",
            "..bbccccccccbb..",
            "..bbccccccccbb..",
            "..bbccccccccbb..",
            "..bbccccccccbb..",
            "...bbccccccbb...",
            "...bbbccccbbb...",
            "....bbbbbbbb....",
            ".....bbbbbb.....",
            "......yyyy......",
            ".....yyyyyy.....",
            "......yyyy......",
            "................",
            "................"
        }, palette);

        pixelReadySprite = BuildPixelSprite("pixel_ready", new[]
        {
            "................",
            "....gggggggg....",
            "...gggyyyyggg...",
            "..ggyyyyyyyygg..",
            "..ggyyyyyyyygg..",
            "..ggyyyyyyyygg..",
            "..ggyyyyyyyygg..",
            "...ggyyyyyygg...",
            "...gggyyyyggg...",
            "....gggggggg....",
            ".....ggyygg.....",
            "......wwww......",
            ".....wwwwww.....",
            "......wwww......",
            "................",
            "................"
        }, palette);

        pixelBurnedSprite = BuildPixelSprite("pixel_burned", new[]
        {
            "................",
            "....kkkkkkkk....",
            "...kkkddddkkk...",
            "..kkddddddddkk..",
            "..kkddddddddkk..",
            "..kkddddddddkk..",
            "..kkddddddddkk..",
            "...kkddkkddkk...",
            "...kkkddddkkk...",
            "....kkkkkkkk....",
            ".....kkkkkk.....",
            "......kkkk......",
            ".....kkkkkk.....",
            "......kkkk......",
            "................",
            "................"
        }, palette);

        pixelSparkSprite = BuildPixelSprite("pixel_spark", new[]
        {
            "................",
            "......w.........",
            ".....www........",
            "....wwwww.......",
            "...wwwwwww......",
            "..wwwwwwwww.....",
            "...wwwwwww......",
            "....wwwww.......",
            ".....www........",
            "......w.........",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................"
        }, palette);

        pixelGrillSprite = BuildPixelSprite("pixel_grill", new[]
        {
            "................",
            ".##############.",
            ".#============#.",
            ".#=#=#=#=#=#=#.",
            ".#============#.",
            ".#=#=#=#=#=#=#.",
            ".#============#.",
            ".#=#=#=#=#=#=#.",
            ".#============#.",
            ".#=#=#=#=#=#=#.",
            ".#============#.",
            ".##############.",
            "................",
            "................",
            "................",
            "................"
        }, palette);

        pixelSmokeSprite = BuildPixelSprite("pixel_smoke", new[]
        {
            "................",
            ".....mssm.......",
            "...mssssssm.....",
            "..mssmmmssss....",
            "..msssssssssm...",
            "...msssssssm....",
            ".....mssm.......",
            "................",
            "......mss.......",
            ".....mssss......",
            "......mss.......",
            "................",
            "................",
            "................",
            "................",
            "................"
        }, palette);
    }

    private Sprite BuildPixelSprite(string name, string[] pattern, Dictionary<char, Color32> palette)
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

    private Image CreatePixelImage(string name, RectTransform parent, Sprite sprite)
    {
        if (parent == null)
        {
            return null;
        }

        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
        image.raycastTarget = false;
        image.color = Color.white;

        SetPixelRect(image, 0.5f, 0.5f, 92f, 92f);
        return image;
    }

    private static Image CreateImage(string name, RectTransform parent, Color color, bool raycastTarget)
    {
        if (parent == null)
        {
            return null;
        }

        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = raycastTarget;
        return image;
    }

    private void SetPixelRect(Image image, float anchorX, float anchorY, float width, float height)
    {
        if (image == null)
        {
            return;
        }

        var rect = image.rectTransform;
        rect.anchorMin = new Vector2(anchorX, anchorY);
        rect.anchorMax = new Vector2(anchorX, anchorY);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(width, height);
    }

    private static RectTransform FindByName(RectTransform rootRect, string targetName)
    {
        if (rootRect == null || string.IsNullOrEmpty(targetName))
        {
            return null;
        }

        var stack = new Stack<Transform>();
        stack.Push(rootRect);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current.name == targetName)
            {
                return current as RectTransform;
            }

            for (int i = 0; i < current.childCount; i++)
            {
                stack.Push(current.GetChild(i));
            }
        }

        return null;
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

    private static Text CreateText(string name, RectTransform parent, int fontSize, TextAnchor alignment, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Shadow));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontStyle = FontStyle.Bold;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.lineSpacing = 1.04f;

        var shadow = go.GetComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.34f);
        shadow.effectDistance = new Vector2(1f, -1f);

        return text;
    }

    private static Button CreateButton(string name, RectTransform parent, string label, Color buttonColor)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        var image = go.GetComponent<Image>();
        image.color = buttonColor;

        var button = go.GetComponent<Button>();
        TintButton(button, buttonColor);

        var labelText = CreateText("Label", rect, 14, TextAnchor.MiddleCenter, new Color(0.99f, 0.95f, 0.88f, 1f));
        labelText.text = label;
        SetRect(labelText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(8f, 6f), new Vector2(-8f, -6f));

        return button;
    }

    private static void TintButton(Button button, Color baseColor)
    {
        if (button == null)
        {
            return;
        }

        var image = button.targetGraphic as Image;
        if (image != null)
        {
            image.color = baseColor;
        }

        var colors = button.colors;
        colors.normalColor = baseColor;
        colors.highlightedColor = Color.Lerp(baseColor, Color.white, 0.18f);
        colors.pressedColor = Color.Lerp(baseColor, Color.black, 0.20f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        button.colors = colors;
    }
}
