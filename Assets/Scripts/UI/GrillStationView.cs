using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class GrillStationView : MonoBehaviour
{
    private const float RefreshInterval = 0.15f;

    private GameManager gameManager;
    private RectTransform root;
    private Text selectionText;
    private Text inventoryText;
    private Text statusText;
    private Button cycleButton;
    private Button buyButton;
    private readonly Button[] slotButtons = new Button[2];
    private readonly Text[] slotTexts = new Text[2];
    private readonly List<MeatInventoryUiEntry> meats = new List<MeatInventoryUiEntry>();

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

        slotButtons[0] = CreateButton("GrillSlotAButton", hudRect, "", new Color(0.42f, 0.18f, 0.12f, 0.95f));
        SetRect(slotButtons[0].transform as RectTransform, new Vector2(0f, 0f), new Vector2(0.5f, 1f), new Vector2(8f, 120f), new Vector2(-6f, -162f));
        slotButtons[0].onClick.AddListener(() => HandleSlotAction(0));
        slotTexts[0] = slotButtons[0].GetComponentInChildren<Text>(true);
        if (slotTexts[0] != null)
        {
            slotTexts[0].fontSize = 18;
            slotTexts[0].resizeTextForBestFit = true;
            slotTexts[0].resizeTextMinSize = 11;
            slotTexts[0].resizeTextMaxSize = 18;
            slotTexts[0].color = new Color(0.99f, 0.94f, 0.86f, 1f);
        }

        slotButtons[1] = CreateButton("GrillSlotBButton", hudRect, "", new Color(0.42f, 0.18f, 0.12f, 0.95f));
        SetRect(slotButtons[1].transform as RectTransform, new Vector2(0.5f, 0f), new Vector2(1f, 1f), new Vector2(6f, 120f), new Vector2(-8f, -162f));
        slotButtons[1].onClick.AddListener(() => HandleSlotAction(1));
        slotTexts[1] = slotButtons[1].GetComponentInChildren<Text>(true);
        if (slotTexts[1] != null)
        {
            slotTexts[1].fontSize = 18;
            slotTexts[1].resizeTextForBestFit = true;
            slotTexts[1].resizeTextMinSize = 11;
            slotTexts[1].resizeTextMaxSize = 18;
            slotTexts[1].color = new Color(0.99f, 0.94f, 0.86f, 1f);
        }

        statusText = CreateText("StatusText", hudRect, 14, TextAnchor.MiddleCenter, new Color(0.98f, 0.93f, 0.82f, 1f));
        SetRect(statusText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(8f, 12f), new Vector2(-8f, 104f));

        uiBuilt = true;
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
            gameManager.PlaceRawMeatOnGrill(slotIndex, selected.menuId);
            Refresh();
            return;
        }

        if (slot.readyToCollect || slot.burned)
        {
            gameManager.CollectFromGrill(slotIndex);
            Refresh();
            return;
        }

        if (slot.canFlip)
        {
            gameManager.FlipMeat(slotIndex);
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
            label.text = "GRILL " + (slotIndex + 1) + "\n[ .... ]\nTap to load selected cut";
            TintButton(button, new Color(0.32f, 0.14f, 0.10f, 0.92f));
            return;
        }

        if (slot.burned)
        {
            label.text = slot.displayName + "\n[ XXXX ] BURNT\nTap to discard";
            TintButton(button, new Color(0.22f, 0.08f, 0.07f, 0.96f));
            return;
        }

        if (slot.readyToCollect)
        {
            label.text = slot.displayName + "\n[ ==== ] READY\nTap to collect";
            TintButton(button, new Color(0.66f, 0.32f, 0.18f, 0.98f));
            return;
        }

        var percent = Mathf.RoundToInt(slot.cookProgress01 * 100f);
        if (slot.canFlip)
        {
            label.text = slot.displayName + "\n[ >><< ] " + percent + "%\nTap to FLIP";
            TintButton(button, new Color(0.56f, 0.22f, 0.14f, 0.96f));
            return;
        }

        var phase = slot.flipped ? "Back side grilling" : "Front side grilling";
        label.text = slot.displayName + "\n[ --## ] " + phase + "\n" + percent + "%";
        TintButton(button, new Color(0.44f, 0.18f, 0.12f, 0.95f));
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
