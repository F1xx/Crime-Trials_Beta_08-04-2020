using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideWaysNavigator : BaseControl
{
    [Header("SideWaysNavigator Options"), SerializeField]
    protected TMPro.TMP_Text TextToChange = null;
    [SerializeField, Tooltip("These are the options the navigator will display. Do NOT leave this empty")]
    protected string[] Options;

    protected int PreviousIndex = 0;

    int _CurrentIndex = 0;
    public int CurrentIndex { get { return _CurrentIndex; } protected set { _CurrentIndex = value; } }

    protected override void Awake()
    {
        base.Awake();
        Listen("Reset" + gameObject.name, ResetToPrevValue);

        if (Options.Length == 0)
        {
            Debug.LogError("Control: " + gameObject.name + " has no options set in it. Set options or many errors will occur.");
        }
    }

    protected override void LoadSettings()
    {
        if (PlayerPrefs.HasKey(PREF_KEY))
        {
            CurrentIndex = PlayerPrefs.GetInt(PREF_KEY);
        }
        else
        {
            CurrentIndex = 0;
            SaveDefaultAndValueToPlayerPrefs();
        }
        ChangeTextToIndex();
    }

    public virtual void NextSetting()
    {
        SetNewIndex((CurrentIndex + 1) % Options.Length);
    }

    public virtual void PreviousSetting()
    {
        SetNewIndex((CurrentIndex - 1) % Options.Length);
    }

    protected void SetNewIndex(int index, bool isReverting = false)
    {
        //don't do this if we're going backwards
        if (isReverting == false)
        {
            PreviousIndex = CurrentIndex;
        }

        CurrentIndex = index;

        CheckCurrentIndex();
        SaveValueToPlayerPrefs();
        ChangeTextToIndex();
        BroadCastEvent();
    }

    protected virtual void CheckCurrentIndex()
    {
        if (Options.Length < 1)
        {
            CurrentIndex = 0;
        }
        //if options is smaller than where we are, set us to the last thing
        //also if it is negative then just loop to the back
        else if (Options.Length < CurrentIndex || CurrentIndex < 0)
        {
            CurrentIndex = Options.Length - 1;
        }
    }

    protected virtual void ChangeTextToIndex()
    {
        TextToChange.text = Options[CurrentIndex];
    }

    public void SetOptions(string[] newOptions)
    {
        Options = newOptions;
        CheckCurrentIndex();
        ChangeTextToIndex();
    }

    public string[] GetOptions()
    {
        return Options;
    }

    protected override void SaveValueToPlayerPrefs()
    {
        PlayerPrefs.SetInt(PREF_KEY, CurrentIndex);
        PlayerPrefs.Save();
    }

    protected override void SaveDefaultAndValueToPlayerPrefs()
    {
        PlayerPrefs.SetInt(PREF_KEY + "Default", CurrentIndex);
        SaveValueToPlayerPrefs();
    }

    public void ResetToPrevValue()
    {
        SetNewIndex(PreviousIndex, true);
    }

    protected override void ResetDefaults()
    {
        SetNewIndex(PlayerPrefs.GetInt(PREF_KEY + "Default"));
    }

    /// <summary>
    /// Broadcasts an event using the Object's name as the event
    /// passes the variables that matter in an EventParam
    /// </summary>
    protected override void BroadCastEvent()
    {
        OnNavigatorChangedParam param = new OnNavigatorChangedParam(CurrentIndex, Options[CurrentIndex], this, PREF_KEY);

        EventManager.TriggerEvent(gameObject.name, param);
        EventManager.TriggerEvent("OnNavigatorChange", param);

        foreach (string name in AdditionalEventsToBroadcastOnChange)
        {
            EventManager.TriggerEvent(name, param);
        }
        base.BroadCastEvent();
    }

    public string GetCurrentValue()
    {
        return Options[CurrentIndex];
    }
}
