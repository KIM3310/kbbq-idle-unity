using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Sizzle Loops")]
    [SerializeField] private AudioSource sizzleLayerA;
    [SerializeField] private AudioSource sizzleLayerB;
    [SerializeField] private AudioSource sizzleLayerC;

    [Header("UI One-Shots")]
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioClip boostClip;
    [SerializeField] private AudioClip upgradeClip;
    [SerializeField] private AudioClip coinClip;
    [SerializeField] private AudioClip buttonClip;
    [SerializeField] private AudioClip happyClip;
    [SerializeField] private AudioClip sadClip;
    [SerializeField] private AudioClip adRewardClip;
    [SerializeField] private AudioClip purchaseClip;

    private void Start()
    {
        PlaySizzleLayers();
    }

    public void PlayBoost()
    {
        if (uiSource != null && boostClip != null)
        {
            uiSource.PlayOneShot(boostClip);
        }
    }

    public void PlayUpgrade()
    {
        if (uiSource != null && upgradeClip != null)
        {
            uiSource.PlayOneShot(upgradeClip);
        }
    }

    public void PlayCoin()
    {
        if (uiSource != null && coinClip != null)
        {
            uiSource.PlayOneShot(coinClip);
        }
    }

    public void PlayButton()
    {
        if (uiSource != null && buttonClip != null)
        {
            uiSource.PlayOneShot(buttonClip);
        }
    }

    public void PlayCustomerReaction(bool happy)
    {
        if (uiSource == null)
        {
            return;
        }

        var clip = happy ? happyClip : sadClip;
        if (clip != null)
        {
            uiSource.PlayOneShot(clip);
        }
    }

    public void PlayAdReward()
    {
        if (uiSource != null && adRewardClip != null)
        {
            uiSource.PlayOneShot(adRewardClip);
        }
    }

    public void PlayPurchase()
    {
        if (uiSource != null && purchaseClip != null)
        {
            uiSource.PlayOneShot(purchaseClip);
        }
    }

    private void PlaySizzleLayers()
    {
        if (sizzleLayerA != null && !sizzleLayerA.isPlaying)
        {
            sizzleLayerA.loop = true;
            sizzleLayerA.Play();
        }

        if (sizzleLayerB != null && !sizzleLayerB.isPlaying)
        {
            sizzleLayerB.loop = true;
            sizzleLayerB.Play();
        }

        if (sizzleLayerC != null && !sizzleLayerC.isPlaying)
        {
            sizzleLayerC.loop = true;
            sizzleLayerC.Play();
        }
    }
}
