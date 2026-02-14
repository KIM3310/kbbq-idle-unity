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
                var label = pack.displayName + " (" + pack.priceLabel + ")\n+" + FormatUtil.FormatCurrency(pack.currencyReward);
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
    }

    private void HandleRewarded()
    {
        var ok = service != null && service.ShowRewardedAd();
        SetStatus(ok ? "보상형 광고 보상 지급" : "광고 비활성");
        gameManager?.GetAudioManager()?.PlayButton();
    }

    private void HandleInterstitial()
    {
        var ok = service != null && service.ShowInterstitialAd();
        SetStatus(ok ? "전면 광고 보상 지급" : "광고 비활성");
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
        SetStatus(ok ? pack.displayName + " 구매 완료" : "구매 실패");
        gameManager?.GetAudioManager()?.PlayButton();
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}
