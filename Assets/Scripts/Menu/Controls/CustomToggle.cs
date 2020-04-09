using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomToggle : BaseControl
{
    [SerializeField]
    protected Toggle m_Toggle = null;

    public bool ToggleValue { get { return _ToggleValue; } protected set { _ToggleValue = value; } }
    protected bool _ToggleValue = false;

    protected override void Awake()
    {
        base.Awake();

        m_Toggle.onValueChanged.AddListener(OnValueChanged);
    }

    protected override void LoadSettings()
    {
        if (PlayerPrefs.HasKey(PREF_KEY))
        {
            SetToggleValue(System.Convert.ToBoolean(PlayerPrefs.GetInt(PREF_KEY)));
        }
        else
        {
            SetToggleValue(m_Toggle.isOn);
            SaveDefaultAndValueToPlayerPrefs();
        }
    }

    public void OnValueChanged(bool val)
    {
        SetToggleValue(val);
    }

    void SetToggleValue(bool val)
    {
        ToggleValue = val;
        UpdateToggleValue();
        SaveValueToPlayerPrefs();
        BroadCastEvent();
    }


    public void UpdateToggleValue()
    {
        m_Toggle.isOn = ToggleValue;
    }

    protected override void BroadCastEvent()
    {
        OnToggleChangeParam param = new OnToggleChangeParam(ToggleValue, m_Toggle, PREF_KEY);

        EventManager.TriggerEvent(gameObject.name, param);
        EventManager.TriggerEvent("OnToggleChange", param);

        foreach (string name in AdditionalEventsToBroadcastOnChange)
        {
            EventManager.TriggerEvent(name, param);
        }
        base.BroadCastEvent();
    }

    protected override void ResetDefaults()
    {
        SetToggleValue(System.Convert.ToBoolean(PlayerPrefs.GetInt(PREF_KEY + "Default")));
        UpdateToggleValue();
    }

    protected override void SaveValueToPlayerPrefs()
    {
        PlayerPrefs.SetInt(PREF_KEY, System.Convert.ToInt32(ToggleValue));
        PlayerPrefs.Save();
    }

    protected override void SaveDefaultAndValueToPlayerPrefs()
    {
        PlayerPrefs.SetInt(PREF_KEY + "Default", System.Convert.ToInt32(ToggleValue));
        SaveValueToPlayerPrefs();
    }
}
