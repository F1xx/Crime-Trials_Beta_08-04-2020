using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseControl : BaseObject
{
    [Header("FILL THIS OUT IMMEDIATELY"), SerializeField, Tooltip("This is how all settings are filled out and figured out and saved. FILL IT IN. Use the PlayerPref/EventNames Documents on the Google Drive")]
    protected string PREF_KEY = "";
    [SerializeField, Tooltip("Should this navigator reset when all other settings reset?")]
    protected bool ListenToAllSettingsReset = true;
    [SerializeField, Tooltip("Use this if you want this to broadcast additional events when its value changes. Note all events will have OnNavigatorChangedParam passed as well. Useful if you want a specific SideWaysNavigator to broadcast more general events.")]
    protected string[] AdditionalEventsToBroadcastOnChange;

    [SerializeField, Tooltip("Use this if you want this to broadcast additional events when its value changes. DOES NOT PASS THE STRUCT. Useful if you want a specific SideWaysNavigator to broadcast more general events.")]
    protected string[] AdditionalEventsToBroadcastOnChangeNoParameter;

    protected override void Awake()
    {
        base.Awake();

        if(string.IsNullOrEmpty(PREF_KEY))
        {
            Debug.LogError("Control: " + gameObject.name + " Has not been given its Playerpref. Functionality will be lost and some potentially broken.");
        }

        if (ListenToAllSettingsReset)
        {
            Listen("OnSettingsReset", ResetDefaults);
        }

        LoadSettings();
    }

    /// <summary>
    /// prep the slider here
    /// </summary>
    protected abstract void LoadSettings();

    /// <summary>
    /// Broadcasts an event using the Object's name as the event
    /// passes the variables that matter in an EventParam
    /// </summary>
    protected virtual void BroadCastEvent()
    {
        if (string.IsNullOrEmpty(PREF_KEY) == false)
        {
            EventManager.TriggerEvent(PREF_KEY);
        }

        foreach (string name in AdditionalEventsToBroadcastOnChangeNoParameter)
        {
            EventManager.TriggerEvent(name);
        }
    }

    /// <summary>
    /// overwrite this function and make sure the control is properly set back to default state.
    /// </summary>
    protected abstract void ResetDefaults();

    /// <summary>
    /// overwrite this and save whatever values need saving in the PlayerPrefs
    /// </summary>
    protected abstract void SaveValueToPlayerPrefs();

    /// <summary>
    /// Same as SaveValue but ensure you also save a default so you can reset properly
    /// </summary>
    protected abstract void SaveDefaultAndValueToPlayerPrefs();
}
