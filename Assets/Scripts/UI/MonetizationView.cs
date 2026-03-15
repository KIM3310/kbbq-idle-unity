using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonetizationView : MonoBehaviour
{
    [SerializeField] private Text statusText;
    [SerializeField] private Button rewardedButton;
    [SerializeField] private Button interstitialButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button[] packButtons;
    [SerializeField] private Text[] packLabels;

    private MonetizationService service;
    private GameManager gameManager;
    private List<IapPack> packs = new List<IapPack>();

    public void Bind(GameManager manager)
    {
        gameManager = manager;
        service = manager != null ? manager.GetMonetizationService() : null;
        ApplyThemeCopy();

        if (rewardedButton != null)
        {
            rewardedButton.onClick.RemoveAllListeners();
            rewardedButton.onClick.AddListener(HandleRewarded);
        }

        if (interstitialButton != null)
        {
            interstitialButton.onClick.RemoveAllListeners();
            interstitialButton.onClick.AddListener(HandleInterstitial);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        Refresh();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void Refresh()
    {
        packs.Clear();
        if (service != null && service.Config != null)
        {
            packs.AddRange(service.Config.packs);
        }

        if (packButtons == null || packLabels == null)
        {
            return;
        }

        var count = Mathf.Min(packButtons.Length, packLabels.Length);
        for (int i = 0; i < count; i++)
        {
            if (i < packs.Count)
            {
                var pack = packs[i];
                var label = pack.displayName + "  " + pack.priceLabel + "\nFestival cash +" + FormatUtil.FormatCurrency(pack.currencyReward);
                if (packLabels[i] != null)
                {
                    packLabels[i].text = label;
                }
                if (packButtons[i] != null)
                {
                    var index = i;
                    packButtons[i].gameObject.SetActive(true);
                    packButtons[i].onClick.RemoveAllListeners();
                    packButtons[i].onClick.AddListener(() => HandlePurchase(index));
                    StyleButton(packButtons[i], i == 0
                        ? new Color(0.82f, 0.34f, 0.18f, 1f)
                        : new Color(0.58f, 0.28f, 0.16f, 0.96f));
                }
            }
            else
            {
                if (packLabels[i] != null)
                {
                    packLabels[i].text = "";
                }
                if (packButtons[i] != null)
                {
                    packButtons[i].gameObject.SetActive(false);
                }
            }
        }

        if (statusText != null)
        {
            statusText.text = "Festival Booth: optional boosts only. Core progression stays playable without paying.";
            statusText.color = new Color(0.98f, 0.92f, 0.80f, 0.98f);
        }

        StyleButton(rewardedButton, new Color(0.94f, 0.44f, 0.16f, 1f));
        StyleButton(interstitialButton, new Color(0.74f, 0.32f, 0.18f, 0.98f));
        StyleButton(closeButton, new Color(0.32f, 0.18f, 0.12f, 0.94f));
    }

    private void HandleRewarded()
    {
        var ok = service != null && service.ShowRewardedAd();
        SetStatus(ok ? "TV hype boost delivered. The crowd mood just jumped." : "Broadcast boost is offline right now.");
        gameManager?.GetAudioManager()?.PlayButton();
    }

    private void HandleInterstitial()
    {
        var ok = service != null && service.ShowInterstitialAd();
        SetStatus(ok ? "Quick promo run complete. You pocketed a small crowd bonus." : "Promo run is offline right now.");
        gameManager?.GetAudioManager()?.PlayButton();
    }

    private void HandlePurchase(int index)
    {
        if (index < 0 || index >= packs.Count)
        {
            return;
        }

        var pack = packs[index];
        var ok = service != null && service.PurchasePack(pack.id);
        SetStatus(ok ? pack.displayName + " unlocked. The kitchen has more runway now." : "Purchase failed. Try again in a moment.");
        gameManager?.GetAudioManager()?.PlayButton();
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    private void ApplyThemeCopy()
    {
        SetButtonLabel(rewardedButton, "TV HYPE x2");
        SetButtonLabel(interstitialButton, "PROMO BURST");
        SetButtonLabel(closeButton, "BACK TO SERVICE");
    }

    private void SetButtonLabel(Button button, string text)
    {
        if (button == null)
        {
            return;
        }

        var label = button.GetComponentInChildren<Text>(true);
        if (label != null)
        {
            label.text = text;
            label.fontStyle = FontStyle.Bold;
        }
    }

    private void StyleButton(Button button, Color color)
    {
        if (button == null)
        {
            return;
        }

        var image = button.targetGraphic as Image;
        if (image != null)
        {
            image.color = color;
        }

        var label = button.GetComponentInChildren<Text>(true);
        if (label != null)
        {
            label.color = new Color(1f, 0.95f, 0.88f, 1f);
            label.fontStyle = FontStyle.Bold;
        }
    }
}
