using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomSlider : BaseControl
{
    [Header("SliderSettings"), SerializeField]
    protected TMPro.TMP_Text SliderValueText = null;
    [SerializeField]
    protected Slider m_Slider = null;

    [SerializeField, Range(0,2), Tooltip("0 - Show the Percentage, 1 - Show the value, 2 - Show the Value / MaxValue")]
    protected int TextType = 0;

    public float SliderValue { get { return _SliderValue; } protected set { _SliderValue = value; } }
    protected float _SliderValue = 0.0f;

    protected override void Awake()
    {
        base.Awake();

        m_Slider.onValueChanged.AddListener(OnValueChanged);
    }

    protected override void LoadSettings()
    {
        if (PlayerPrefs.HasKey(PREF_KEY))
        {
            SetSliderValue(PlayerPrefs.GetFloat(PREF_KEY));
        }
        else
        {
            SetSliderValue(m_Slider.value);
            SaveDefaultAndValueToPlayerPrefs();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        SliderValue = m_Slider.value;
        SetSliderText();
    }
#endif

    protected void SetSliderValue(float val)
    {
        SliderValue = val;
        SetSliderText();
        UpdateSliderValue();
        SaveValueToPlayerPrefs();
        BroadCastEvent();
    }

    public virtual void OnValueChanged(float val)
    {
        if(MathUtils.FloatCloseEnough(val, 0.0f, 0.01f))
        {
            val = 0.0f;
        }

        SetSliderValue(val);
    }

    protected void SetSliderText()
    {
        switch(TextType)
        {
            case 0:
                SetPercentageText();
                break;
            case 1:
                SetValueText();
                break;
            case 2:
                SetValueOverMaxText();
                break;
        }
    }

    void SetPercentageText()
    {
        float range = (SliderValue - m_Slider.minValue) / (m_Slider.maxValue - m_Slider.minValue);

        if (m_Slider.wholeNumbers)
        {
            //SliderValueText.text = string.Format("{0:0%}", SliderValue / m_Slider.maxValue);
            SliderValueText.text = string.Format("{0:0%}", range);
        }
        else
        {
            //SliderValueText.text = string.Format("{0:0.0%}", SliderValue / m_Slider.maxValue);
            SliderValueText.text = string.Format("{0:0.0%}", range);
        }
    }

    void SetValueText()
    {
        if (m_Slider.wholeNumbers)
        {
            SliderValueText.text = SliderValue.ToString("F0");
        }
        else
        {
            SliderValueText.text = SliderValue.ToString("F2");
        }
    }

    void SetValueOverMaxText()
    {
        if (m_Slider.wholeNumbers)
        {
            SliderValueText.text = string.Format("{0:0} / {1:0}", SliderValue, m_Slider.maxValue);
        }
        else
        {
            SliderValueText.text = string.Format("{0:0.00} / {1:0.00}", SliderValue, m_Slider.maxValue);
        }
    }

    public void UpdateSliderValue()
    {
        m_Slider.value = SliderValue;
    }


    /// <summary>
    /// Broadcasts an event using the Object's name as the event
    /// passes the variables that matter in an EventParam
    /// </summary>
    protected override void BroadCastEvent()
    {
        OnSliderChangeParam param = new OnSliderChangeParam(SliderValue, m_Slider, PREF_KEY);

        EventManager.TriggerEvent(gameObject.name, param);
        EventManager.TriggerEvent("OnSliderChange", param);

        foreach (string name in AdditionalEventsToBroadcastOnChange)
        {
            EventManager.TriggerEvent(name, param);
        }

        base.BroadCastEvent();
    }

    /// <summary>
    /// overwrite this function and make sure the control is properly set back to default state.
    /// </summary>
    protected override void ResetDefaults()
    {
        SetSliderValue(PlayerPrefs.GetFloat(PREF_KEY + "Default"));
        UpdateSliderValue();
    }

    /// <summary>
    /// overwrite this and save whatever values need saving in the PlayerPrefs
    /// </summary>
    protected override void SaveValueToPlayerPrefs()
    {

        PlayerPrefs.SetFloat(PREF_KEY, SliderValue);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Same as SaveValue but ensure you also save a default so you can reset properly
    /// </summary>
    protected override void SaveDefaultAndValueToPlayerPrefs()
    {
        PlayerPrefs.SetFloat(PREF_KEY + "Default", SliderValue);
        SaveValueToPlayerPrefs();
    }
}
