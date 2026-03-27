using UnityEngine;
using UnityEngine.UI;

public class DebugPanelView : MonoBehaviour
{
    [SerializeField] private Slider spawnRateSlider;
    [SerializeField] private Slider serviceRateSlider;
    [SerializeField] private Text spawnValueText;
    [SerializeField] private Text serviceValueText;
    [SerializeField] private Dropdown presetDropdown;
    [SerializeField] private Button resetButton;

    private GameManager gameManager;
    private bool suppressDropdown;

    public void Bind(GameManager manager)
    {
        gameManager = manager;
        SetupSlider(spawnRateSlider, gameManager != null ? gameManager.GetQueueSpawnMultiplier() : 1f, OnSpawnRateChanged);
        SetupSlider(serviceRateSlider, gameManager != null ? gameManager.GetQueueServiceMultiplier() : 1f, OnServiceRateChanged);
        SetupDropdown();
        SetupResetButton();
        UpdateLabels();
    }

    private void SetupSlider(Slider slider, float value, UnityEngine.Events.UnityAction<float> callback)
    {
        if (slider == null)
        {
            return;
        }

        slider.minValue = 0.5f;
        slider.maxValue = 2.5f;
        slider.wholeNumbers = false;
        slider.onValueChanged.RemoveAllListeners();
        slider.value = value;
        slider.onValueChanged.AddListener(callback);
    }

    private void OnSpawnRateChanged(float value)
    {
        gameManager?.SetQueueSpawnMultiplier(value);
        MarkPresetCustom();
        UpdateLabels();
    }

    private void OnServiceRateChanged(float value)
    {
        gameManager?.SetQueueServiceMultiplier(value);
        MarkPresetCustom();
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        if (spawnValueText != null)
        {
            spawnValueText.text = "x" + (spawnRateSlider != null ? spawnRateSlider.value.ToString("0.0") : "1.0");
        }
        if (serviceValueText != null)
        {
            serviceValueText.text = "x" + (serviceRateSlider != null ? serviceRateSlider.value.ToString("0.0") : "1.0");
        }
    }

    private void SetupDropdown()
    {
        if (presetDropdown == null)
        {
            return;
        }

        presetDropdown.ClearOptions();
        presetDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "0.5x",
            "1.0x",
            "2.0x",
            "Custom"
        });
        presetDropdown.value = 1;
        presetDropdown.onValueChanged.RemoveAllListeners();
        presetDropdown.onValueChanged.AddListener(OnPresetChanged);
    }

    private void OnPresetChanged(int index)
    {
        if (suppressDropdown)
        {
            return;
        }

        float value = 1f;
        switch (index)
        {
            case 0:
                value = 0.5f;
                break;
            case 1:
                value = 1f;
                break;
            case 2:
                value = 2f;
                break;
            default:
                return;
        }

        if (spawnRateSlider != null)
        {
            spawnRateSlider.value = value;
        }
        if (serviceRateSlider != null)
        {
            serviceRateSlider.value = value;
        }

        gameManager?.SetQueueSpawnMultiplier(value);
        gameManager?.SetQueueServiceMultiplier(value);
        UpdateLabels();
    }

    private void MarkPresetCustom()
    {
        if (presetDropdown == null)
        {
            return;
        }

        suppressDropdown = true;
        presetDropdown.value = 3;
        suppressDropdown = false;
    }

    private void SetupResetButton()
    {
        if (resetButton == null)
        {
            return;
        }

        resetButton.onClick.RemoveAllListeners();
        resetButton.onClick.AddListener(ResetMultipliers);
    }

    private void ResetMultipliers()
    {
        SetPresetIndex(1);
        UpdateLabels();
    }

    public int GetPresetIndex()
    {
        return presetDropdown != null ? presetDropdown.value : 1;
    }

    public void SetPresetIndex(int index)
    {
        if (presetDropdown == null)
        {
            return;
        }

        var clamped = Mathf.Clamp(index, 0, 3);
        suppressDropdown = true;
        presetDropdown.value = clamped;
        suppressDropdown = false;

        if (clamped == 0)
        {
            ApplyPresetValue(0.5f);
        }
        else if (clamped == 1)
        {
            ApplyPresetValue(1f);
        }
        else if (clamped == 2)
        {
            ApplyPresetValue(2f);
        }
    }

    public void SetSliderValues(float spawnValue, float serviceValue, bool markCustom)
    {
        if (spawnRateSlider != null)
        {
            spawnRateSlider.value = spawnValue;
        }
        if (serviceRateSlider != null)
        {
            serviceRateSlider.value = serviceValue;
        }

        if (markCustom)
        {
            MarkPresetCustom();
        }

        UpdateLabels();
    }

    private void ApplyPresetValue(float value)
    {
        if (spawnRateSlider != null)
        {
            spawnRateSlider.value = value;
        }
        if (serviceRateSlider != null)
        {
            serviceRateSlider.value = value;
        }

        gameManager?.SetQueueSpawnMultiplier(value);
        gameManager?.SetQueueServiceMultiplier(value);
        UpdateLabels();
    }
}
