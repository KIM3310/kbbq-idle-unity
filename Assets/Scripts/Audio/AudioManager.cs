using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private const int SampleRate = 44100;

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

    [Header("Kitchen SFX (optional)")]
    [SerializeField] private AudioClip grillLoadClip;
    [SerializeField] private AudioClip grillFlipClip;
    [SerializeField] private AudioClip grillCollectClip;
    [SerializeField] private AudioClip grillBurnClip;

    private AudioClip runtimeGrillLoadClip;
    private AudioClip runtimeGrillFlipClip;
    private AudioClip runtimeGrillCollectClip;
    private AudioClip runtimeGrillBurnClip;
    private AudioClip runtimeSizzleClipA;
    private AudioClip runtimeSizzleClipB;
    private AudioClip runtimeSizzleClipC;
    private AudioClip runtimeSizzleCrackleClip;
    private float sizzleIntensity;
    private float crackleTimer;

    private void Awake()
    {
        BuildRuntimeKitchenClips();
    }

    private void Start()
    {
        EnsureSizzleClip(sizzleLayerA, runtimeSizzleClipA, 0.96f);
        EnsureSizzleClip(sizzleLayerB, runtimeSizzleClipB, 1.03f);
        EnsureSizzleClip(sizzleLayerC, runtimeSizzleClipC, 1.10f);
        PlaySizzleLayers();
        SetSizzleIntensity(0f);
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

    public void PlayGrillLoad()
    {
        PlayClipWithFallback(grillLoadClip, runtimeGrillLoadClip, 0.9f);
    }

    public void PlayGrillFlip()
    {
        PlayClipWithFallback(grillFlipClip, runtimeGrillFlipClip, 1f);
    }

    public void PlayGrillCollect()
    {
        PlayClipWithFallback(grillCollectClip, runtimeGrillCollectClip, 1f);
    }

    public void PlayGrillBurn()
    {
        PlayClipWithFallback(grillBurnClip, runtimeGrillBurnClip, 1f);
    }

    public void SetSizzleIntensity(float normalized)
    {
        var n = Mathf.Clamp01(normalized);
        sizzleIntensity = n;
        SetLayerVolume(sizzleLayerA, Mathf.Lerp(0.06f, 0.42f, n));
        SetLayerVolume(sizzleLayerB, Mathf.Lerp(0.04f, 0.34f, n * 0.95f));
        SetLayerVolume(sizzleLayerC, Mathf.Lerp(0.03f, 0.28f, n * 0.9f));
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

    private void Update()
    {
        if (uiSource == null || sizzleIntensity <= 0.08f)
        {
            return;
        }

        crackleTimer -= Time.unscaledDeltaTime;
        if (crackleTimer > 0f)
        {
            return;
        }

        if (runtimeSizzleCrackleClip != null)
        {
            uiSource.pitch = Random.Range(0.88f, 1.16f);
            uiSource.PlayOneShot(runtimeSizzleCrackleClip, Mathf.Lerp(0.06f, 0.24f, sizzleIntensity));
            uiSource.pitch = 1f;
        }

        crackleTimer = Mathf.Lerp(0.48f, 0.12f, sizzleIntensity) + Random.Range(0.02f, 0.15f);
    }

    private void PlayClipWithFallback(AudioClip configured, AudioClip fallback, float volume)
    {
        if (uiSource == null)
        {
            return;
        }

        var clip = configured != null ? configured : fallback;
        if (clip == null)
        {
            return;
        }

        uiSource.pitch = Random.Range(0.96f, 1.05f);
        uiSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        uiSource.pitch = 1f;
    }

    private void SetLayerVolume(AudioSource source, float volume)
    {
        if (source == null)
        {
            return;
        }

        source.volume = Mathf.Clamp01(volume);
    }

    private void BuildRuntimeKitchenClips()
    {
        runtimeGrillLoadClip = CreateToneSweep("rt_grill_load", 120f, 90f, 0.08f, 0.22f, 0.08f);
        runtimeGrillFlipClip = CreateToneSweep("rt_grill_flip", 430f, 260f, 0.07f, 0.18f, 0.20f);
        runtimeGrillCollectClip = CreateToneSweep("rt_grill_collect", 360f, 680f, 0.14f, 0.20f, 0.03f);
        runtimeGrillBurnClip = CreateNoiseBurst("rt_grill_burn", 0.20f, 0.28f);
        runtimeSizzleClipA = CreateSizzleLoop("rt_sizzle_a", 0.32f, 250f, 1200f, 0.18f);
        runtimeSizzleClipB = CreateSizzleLoop("rt_sizzle_b", 0.32f, 210f, 980f, 0.15f);
        runtimeSizzleClipC = CreateSizzleLoop("rt_sizzle_c", 0.32f, 175f, 860f, 0.12f);
        runtimeSizzleCrackleClip = CreateNoiseBurst("rt_sizzle_crackle", 0.05f, 0.22f);
    }

    private AudioClip CreateToneSweep(string name, float startHz, float endHz, float duration, float amplitude, float noiseMix)
    {
        var sampleCount = Mathf.Max(1, Mathf.RoundToInt(SampleRate * duration));
        var data = new float[sampleCount];
        var span = Mathf.Max(0.0001f, duration);

        for (int i = 0; i < sampleCount; i++)
        {
            var t = i / (float)(sampleCount - 1);
            var phase = 2f * Mathf.PI * ((startHz * t * span) + ((endHz - startHz) * 0.5f * t * t * span));
            var env = Mathf.Sin(t * Mathf.PI);
            var tone = Mathf.Sin(phase);
            var noise = (Random.value * 2f - 1f) * noiseMix;
            data[i] = (tone * amplitude + noise * amplitude) * env;
        }

        var clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateNoiseBurst(string name, float duration, float amplitude)
    {
        var sampleCount = Mathf.Max(1, Mathf.RoundToInt(SampleRate * duration));
        var data = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            var t = i / (float)(sampleCount - 1);
            var env = (1f - t) * Mathf.Sin(t * Mathf.PI);
            var noise = (Random.value * 2f - 1f);
            data[i] = noise * amplitude * env;
        }

        var clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateSizzleLoop(string name, float duration, float lowPassHz, float hiPassHz, float amplitude)
    {
        var sampleCount = Mathf.Max(1, Mathf.RoundToInt(SampleRate * duration));
        var data = new float[sampleCount];
        var low = 0f;
        var high = 0f;
        var lowCoeff = Mathf.Clamp01(lowPassHz / SampleRate);
        var highCoeff = Mathf.Clamp01(hiPassHz / SampleRate);

        for (int i = 0; i < sampleCount; i++)
        {
            var white = (Random.value * 2f - 1f) * amplitude;
            low += (white - low) * lowCoeff;
            high += (low - high) * highCoeff;
            var tone = Mathf.Sin(2f * Mathf.PI * 34f * (i / (float)SampleRate)) * 0.01f;
            data[i] = Mathf.Clamp(high + tone, -0.95f, 0.95f);
        }

        var clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static void EnsureSizzleClip(AudioSource source, AudioClip runtimeClip, float pitch)
    {
        if (source == null)
        {
            return;
        }

        if (source.clip == null && runtimeClip != null)
        {
            source.clip = runtimeClip;
        }

        source.pitch = pitch;
    }
}
