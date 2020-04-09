using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioChannel))]
public class CustomSlider_Audio : CustomSlider
{
    AudioChannel m_Channel = null;
    public ScriptableAudio MenuTestSound = null;
    public AudioManager.eChannelType AudioType = AudioManager.eChannelType.SoundEffects;

    protected override void Awake()
    {
        base.Awake();

        m_Channel = GetComponent<AudioChannel>();
    }

    public override void OnValueChanged(float val)
    {
        base.OnValueChanged(val);
        ChangeVolume(val);
    }


    void ChangeVolume(float amount, bool playAudio = true)
    {
        amount = Mathf.Clamp(amount, 0.0f, 1.0f);

        OnVolumeChangeParam param = new OnVolumeChangeParam(amount, AudioType);
        EventManager.TriggerEvent("OnVolumeChange", param);

        if (playAudio)
        {
            if (m_Channel.GetChannelType() != AudioType && AudioType != AudioManager.eChannelType.Master)
            {
                m_Channel.SetChannelType(AudioType);
            }

            m_Channel.PlayAudio(MenuTestSound, true);
        }
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

    protected override void ResetDefaults()
    {
        SetSliderValue(PlayerPrefs.GetFloat(PREF_KEY + "Default"));
        UpdateSliderValue();
        ChangeVolume(PlayerPrefs.GetFloat(PREF_KEY + "Default"), false);
    }
}
