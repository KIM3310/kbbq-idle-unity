using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private const int SampleRate = 44100;

    [Header("Sizzle Loops")]
    [SerializeField] private AudioSource sizzleLayerA;
    [SerializeField] private AudioSource sizzleLayerB;
    [SerializeField] private AudioSource sizzleLayerC;
    [SerializeField] private AudioSource musicLayer;
    [SerializeField] private AudioSource ambienceLayer;

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
    [SerializeField] private AudioClip comboClip;
    [SerializeField] private AudioClip levelUpClip;
    [SerializeField] private AudioClip tierUpClip;

    private AudioClip runtimeGrillLoadClip;
    private AudioClip runtimeGrillFlipClip;
    private AudioClip runtimeGrillCollectClip;
    private AudioClip runtimeGrillBurnClip;
    private AudioClip runtimeComboClip;
    private AudioClip runtimeLevelUpClip;
    private AudioClip runtimeTierUpClip;
    private AudioClip runtimeSizzleClipA;
    private AudioClip runtimeSizzleClipB;
    private AudioClip runtimeSizzleClipC;
    private AudioClip runtimeSizzleCrackleClip;
    private AudioClip runtimeMusicLoopClip;
    private AudioClip runtimeAmbienceLoopClip;
    private float sizzleIntensity;
    private float crackleTimer;
    private float targetMusicVolume;
    private float targetAmbienceVolume;

    private void Awake()
    {
        EnsureRuntimeSources();
        LoadResourceFallbacks();
        BuildRuntimeKitchenClips();
    }

    private void Start()
    {
        EnsureSizzleClip(sizzleLayerA, runtimeSizzleClipA, 0.96f);
        EnsureSizzleClip(sizzleLayerB, runtimeSizzleClipB, 1.03f);
        EnsureSizzleClip(sizzleLayerC, runtimeSizzleClipC, 1.10f);
        EnsureLoopClip(musicLayer, runtimeMusicLoopClip, 1f, 0.08f);
        EnsureLoopClip(ambienceLayer, runtimeAmbienceLoopClip, 1f, 0.04f);
        PlaySizzleLayers();
        PlayLoopLayer(musicLayer);
        PlayLoopLayer(ambienceLayer);
        SetSizzleIntensity(0f);
        SetRestaurantMood(0f, 0f, 0f, false);
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

    public void PlayCombo(int comboCount)
    {
        if (comboCount < 2)
        {
            return;
        }
        var volume = Mathf.Lerp(0.18f, 0.34f, Mathf.Clamp01(comboCount / 8f));
        PlayClipWithFallback(comboClip, runtimeComboClip, volume);
    }

    public void PlayLevelUp()
    {
        PlayClipWithFallback(levelUpClip, runtimeLevelUpClip, 0.48f);
    }

    public void PlayTierUp()
    {
        PlayClipWithFallback(tierUpClip, runtimeTierUpClip, 0.58f);
    }

    public void SetSizzleIntensity(float normalized)
    {
        var n = Mathf.Clamp01(normalized);
        sizzleIntensity = n;
        SetLayerVolume(sizzleLayerA, Mathf.Lerp(0.06f, 0.42f, n));
        SetLayerVolume(sizzleLayerB, Mathf.Lerp(0.04f, 0.34f, n * 0.95f));
        SetLayerVolume(sizzleLayerC, Mathf.Lerp(0.03f, 0.28f, n * 0.9f));
    }

    public void SetRestaurantMood(float queuePressure01, float comboPressure01, float tierPressure01, bool rushActive)
    {
        var queueMood = Mathf.Clamp01(queuePressure01);
        var comboMood = Mathf.Clamp01(comboPressure01);
        var tierMood = Mathf.Clamp01(tierPressure01);
        var rushMood = rushActive ? 1f : 0f;
        var musicMood = Mathf.Clamp01(0.18f + queueMood * 0.24f + comboMood * 0.32f + tierMood * 0.18f + rushMood * 0.18f);
        var ambienceMood = Mathf.Clamp01(0.10f + queueMood * 0.46f + tierMood * 0.20f + rushMood * 0.14f);

        targetMusicVolume = Mathf.Lerp(0.05f, 0.22f, musicMood);
        targetAmbienceVolume = Mathf.Lerp(0.03f, 0.16f, ambienceMood);

        if (musicLayer != null)
        {
            musicLayer.pitch = Mathf.Lerp(0.94f, 1.08f, Mathf.Clamp01(comboMood * 0.6f + rushMood * 0.4f));
        }
        if (ambienceLayer != null)
        {
            ambienceLayer.pitch = Mathf.Lerp(0.96f, 1.04f, Mathf.Clamp01(queueMood * 0.5f + tierMood * 0.5f));
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

    private void Update()
    {
        if (musicLayer != null)
        {
            musicLayer.volume = Mathf.Lerp(musicLayer.volume, targetMusicVolume, Time.unscaledDeltaTime * 1.6f);
        }
        if (ambienceLayer != null)
        {
            ambienceLayer.volume = Mathf.Lerp(ambienceLayer.volume, targetAmbienceVolume, Time.unscaledDeltaTime * 1.4f);
        }

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

    private void EnsureRuntimeSources()
    {
        uiSource = EnsureSource(uiSource, "UI One Shots", false, 0.9f);
        sizzleLayerA = EnsureSource(sizzleLayerA, "Sizzle Layer A", true, 0.08f);
        sizzleLayerB = EnsureSource(sizzleLayerB, "Sizzle Layer B", true, 0.06f);
        sizzleLayerC = EnsureSource(sizzleLayerC, "Sizzle Layer C", true, 0.05f);
        musicLayer = EnsureSource(musicLayer, "Dining Music", true, 0.08f);
        ambienceLayer = EnsureSource(ambienceLayer, "Dining Ambience", true, 0.04f);
    }

    private void LoadResourceFallbacks()
    {
        grillLoadClip = grillLoadClip != null ? grillLoadClip : Resources.Load<AudioClip>("Audio/impactSoft_heavy_001");
        grillFlipClip = grillFlipClip != null ? grillFlipClip : Resources.Load<AudioClip>("Audio/impactTin_medium_001");
        grillCollectClip = grillCollectClip != null ? grillCollectClip : Resources.Load<AudioClip>("Audio/impactPlate_medium_002");
        levelUpClip = levelUpClip != null ? levelUpClip : Resources.Load<AudioClip>("Audio/impactBell_heavy_001");
        tierUpClip = tierUpClip != null ? tierUpClip : Resources.Load<AudioClip>("Audio/impactBell_heavy_001");
        comboClip = comboClip != null ? comboClip : Resources.Load<AudioClip>("Audio/impactPlate_medium_002");
        if (ambienceLayer != null && ambienceLayer.clip == null)
        {
            ambienceLayer.clip = Resources.Load<AudioClip>("Audio/kitchen_cookware_clatter");
        }
    }

    private void BuildRuntimeKitchenClips()
    {
        runtimeGrillLoadClip = CreateToneSweep("rt_grill_load", 120f, 90f, 0.08f, 0.22f, 0.08f);
        runtimeGrillFlipClip = CreateToneSweep("rt_grill_flip", 430f, 260f, 0.07f, 0.18f, 0.20f);
        runtimeGrillCollectClip = CreateToneSweep("rt_grill_collect", 360f, 680f, 0.14f, 0.20f, 0.03f);
        runtimeGrillBurnClip = CreateNoiseBurst("rt_grill_burn", 0.20f, 0.28f);
        runtimeComboClip = CreateToneSweep("rt_combo", 520f, 880f, 0.12f, 0.20f, 0.04f);
        runtimeLevelUpClip = CreateToneSweep("rt_level_up", 320f, 920f, 0.26f, 0.24f, 0.02f);
        runtimeTierUpClip = CreateToneSweep("rt_tier_up", 220f, 1040f, 0.34f, 0.28f, 0.03f);
        runtimeSizzleClipA = CreateSizzleLoop("rt_sizzle_a", 0.32f, 250f, 1200f, 0.18f);
        runtimeSizzleClipB = CreateSizzleLoop("rt_sizzle_b", 0.32f, 210f, 980f, 0.15f);
        runtimeSizzleClipC = CreateSizzleLoop("rt_sizzle_c", 0.32f, 175f, 860f, 0.12f);
        runtimeSizzleCrackleClip = CreateNoiseBurst("rt_sizzle_crackle", 0.05f, 0.22f);
        runtimeMusicLoopClip = CreateMusicLoop("rt_dining_music", 6.0f, 0.10f);
        runtimeAmbienceLoopClip = CreateAmbienceLoop("rt_dining_ambience", 5.5f, 0.07f);
    }

    private AudioClip CreateToneSweep(string name, float startHz, float endHz, float duration, float amplitude, float noiseMix)
    {
        var sampleCount = Mathf.Max(1, Mathf.RoundToInt(SampleRate * duration));
        var data = new float[sampleCount];
        var span = Mathf.Max(0.0001f, duration);

        for (int i = 0; i < sampleCount; i++)
        {
            var t = i / (float)(sampleCount - 1);
            var curHz = Mathf.Lerp(startHz, endHz, t * t);
            var phase = 2f * Mathf.PI * curHz * t * span;
            
            var env = (t < 0.1f) ? (t / 0.1f) : Mathf.Exp(-t * 5f);
            
            var tone = Mathf.Sin(phase);
            tone += 0.5f * Mathf.Sin(phase * 2f);
            tone += 0.25f * Mathf.Sin(phase * 3f);
            tone /= 1.75f;

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
            var env = (t < 0.05f) ? (t / 0.05f) : Mathf.Exp(-t * 6f);
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

    private AudioClip CreateMusicLoop(string name, float duration, float amplitude)
    {
        var sampleCount = Mathf.Max(1, Mathf.RoundToInt(SampleRate * duration));
        var data = new float[sampleCount];
        var beatLength = duration / 8f;
        var baseNotes = new[] { 110f, 146.8f, 164.8f, 196f };

        for (int i = 0; i < sampleCount; i++)
        {
            var t = i / (float)SampleRate;
            var beatIndex = Mathf.FloorToInt(t / beatLength) % 8;
            var phrase = beatIndex / 2;
            var localBeatTime = (t % beatLength) / beatLength;
            var bassHz = baseNotes[phrase % baseNotes.Length];
            var chordHz = bassHz * 2f;
            var pulseEnv = Mathf.Exp(-localBeatTime * 5.2f);
            var kickEnv = Mathf.Exp(-localBeatTime * 9.5f);

            var bass = Mathf.Sin(2f * Mathf.PI * bassHz * t) * 0.26f * pulseEnv;
            var chord = (Mathf.Sin(2f * Mathf.PI * chordHz * t) + Mathf.Sin(2f * Mathf.PI * chordHz * 1.5f * t)) * 0.11f * pulseEnv;
            var kick = Mathf.Sin(2f * Mathf.PI * 54f * t) * 0.22f * kickEnv;
            var hi = ((beatIndex % 2 == 1) ? 1f : 0f) * Mathf.Sin(2f * Mathf.PI * 880f * t) * 0.025f * Mathf.Exp(-localBeatTime * 18f);

            data[i] = Mathf.Clamp((bass + chord + kick + hi) * amplitude, -0.95f, 0.95f);
        }

        var clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateAmbienceLoop(string name, float duration, float amplitude)
    {
        var sampleCount = Mathf.Max(1, Mathf.RoundToInt(SampleRate * duration));
        var data = new float[sampleCount];
        var low = 0f;
        var coeff = 0.018f;

        for (int i = 0; i < sampleCount; i++)
        {
            var t = i / (float)SampleRate;
            var white = (Random.value * 2f - 1f) * amplitude;
            low += (white - low) * coeff;
            var hum = Mathf.Sin(2f * Mathf.PI * 72f * t) * 0.02f;
            var chatter = Mathf.Sin(2f * Mathf.PI * 280f * t) * Mathf.Sin(2f * Mathf.PI * 0.23f * t) * 0.015f;
            data[i] = Mathf.Clamp(low * 0.65f + hum + chatter, -0.7f, 0.7f);
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

    private static void EnsureLoopClip(AudioSource source, AudioClip runtimeClip, float pitch, float volume)
    {
        if (source == null)
        {
            return;
        }

        if (source.clip == null && runtimeClip != null)
        {
            source.clip = runtimeClip;
        }

        source.loop = true;
        source.pitch = pitch;
        source.volume = volume;
    }

    private static void PlayLoopLayer(AudioSource source)
    {
        if (source != null && source.clip != null && !source.isPlaying)
        {
            source.Play();
        }
    }

    private AudioSource EnsureSource(AudioSource source, string name, bool loop, float volume)
    {
        if (source != null)
        {
            source.loop = loop;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = volume;
            return source;
        }

        var go = new GameObject(name, typeof(AudioSource));
        go.transform.SetParent(transform, false);
        source = go.GetComponent<AudioSource>();
        source.loop = loop;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = volume;
        return source;
    }
}
