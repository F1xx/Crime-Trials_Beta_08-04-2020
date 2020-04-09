using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EventManager : Singleton<EventManager>
{

    private Dictionary<string, Action> NoParamEventDictionary = new Dictionary<string, Action>();
    private Dictionary<string, Action<EventParam>> OneParamEventDictionary = new Dictionary<string, Action<EventParam>>();

    protected override void OnAwake()
    {
        DontDestroyOnLoad(this);
    }

    /// <summary>
    /// Subscribe as a listener to an event. If it doesn't exist this will create it.
    /// </summary>
    /// <param name="relativeEventOwner">This is if the event is relative to a specific object. Pass in said object.</param>
    /// <param name="eventName">The name of the event</param>
    /// <param name="listener">The function you passed in when you started listening (function pointer)</param>
    public static void StartListening(GameObject relativeEventOwner, string eventName, Action listener)
    {
        eventName += relativeEventOwner.transform.root.name + relativeEventOwner.transform.root.GetHashCode().ToString();

        StartListening(eventName, listener);
    }

    /// <summary>
    /// Subscribe as a listener to an event. If it doesn't exist this will create it.
    /// </summary>
    /// <param name="relativeEventOwner">This is if the event is relative to a specific object. Pass in said object.</param>
    /// <param name="eventName">The name of the event</param>
    /// <param name="listener">The function you passed in when you started listening (function pointer)</param>
    public static void StartListening(GameObject relativeEventOwner, string eventName, Action<EventParam> listener)
    {
        eventName += relativeEventOwner.transform.root.name + relativeEventOwner.transform.root.GetHashCode().ToString();

        StartListening(eventName, listener);
    }

    /// <summary>
    /// Subscribe as a listener to an event. If it doesn't exist this will create it.
    /// </summary>
    /// <param name="eventName">The name of the event</param>
    /// <param name="listener">The function you passed in when you started listening (function pointer)</param>
    public static void StartListening(string eventName, Action listener)
    {
        Action thisEvent;
        EventManager instance = Instance();

        if (instance)
        {
            if (instance.NoParamEventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent += listener;
                instance.NoParamEventDictionary[eventName] = thisEvent;
            }
            else
            {
                thisEvent += listener;
                instance.NoParamEventDictionary.Add(eventName, thisEvent);
            }
        }
    }

    /// <summary>
    /// Subscribe as a listener to an event. If it doesn't exist this will create it.
    /// </summary>
    /// <param name="eventName">The name of the event</param>
    /// <param name="listener">The function you passed in when you started listening (function pointer)</param>
    public static void StartListening(string eventName, Action<EventParam> listener)
    {
        Action<EventParam> thisEvent;
        EventManager instance = Instance();

        if (instance)
        {
            if (instance.OneParamEventDictionary.TryGetValue(eventName, out thisEvent))
            {
                //Add more event to the existing one
                thisEvent += listener;

                //Update the Dictionary
                instance.OneParamEventDictionary[eventName] = thisEvent;
            }
            else
            {
                //Add event to the Dictionary for the first time
                thisEvent += listener;
                instance.OneParamEventDictionary.Add(eventName, thisEvent);
            }
        }
    }

    /// <summary>
    /// Remove yourself as a listener to the named Event.
    /// </summary>
    /// <param name="eventName">The name of the event</param>
    /// <param name="listener">The function you passed in when you started listening (function pointer)</param>
    public static void StopListening(string eventName, Action listener)
    {
        Action thisEvent = null;
        EventManager instance = Instance();

        if (instance && instance.NoParamEventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent -= listener;
            instance.NoParamEventDictionary[eventName] = thisEvent;
        }
    }

    /// <summary>
    /// Remove yourself as a listener to the named Event.
    /// </summary>
    /// <param name="eventName">The name of the event</param>
    /// <param name="listener">The function you passed in when you started listening (function pointer)</param>
    public static void StopListening(string eventName, Action<EventParam> listener)
    {
        Action<EventParam> thisEvent;
        EventManager instance = Instance();

        if (instance && instance.OneParamEventDictionary.TryGetValue(eventName, out thisEvent))
        {
            //Remove event from the existing one
            thisEvent -= listener;
            instance.OneParamEventDictionary[eventName] = thisEvent;
        }
    }

    /// <summary>
    /// Remove yourself as a listener to the named Event.
    /// </summary>
    /// <param name="relativeEventOwner">This is if the event is relative to a specific object. Pass in said object.</param>
    /// <param name="eventName">The name of the event</param>
    /// <param name="listener">The function you passed in when you started listening (function pointer)</param>
    public static void StopListening(GameObject relativeEventOwner, string eventName, Action listener)
    {
        eventName += relativeEventOwner.transform.root.name + relativeEventOwner.transform.root.GetHashCode().ToString();

        StopListening(eventName, listener);
    }

    /// <summary>
    /// Remove yourself as a listener to the named Event.
    /// </summary>
    /// <param name="relativeEventOwner">This is if the event is relative to a specific object. Pass in said object.</param>
    /// <param name="eventName">The name of the event</param>
    /// <param name="listener">The function you passed in when you started listening (function pointer)</param>
    public static void StopListening(GameObject relativeEventOwner, string eventName, Action<EventParam> listener)
    {
        eventName += relativeEventOwner.transform.root.name + relativeEventOwner.transform.root.GetHashCode().ToString();

        StopListening(eventName, listener);
    }

    /// <summary>
    /// If the event exists this will call all listening functions
    /// </summary>
    /// <param name="eventName">The name of the event</param>
    public static void TriggerEvent(string eventName)
    {
        Action thisEvent = null;
        if (Instance() && Instance().NoParamEventDictionary.TryGetValue(eventName, out thisEvent))
        {
            if (thisEvent != null)
            {
                thisEvent.Invoke();
            }
        }
    }

    /// <summary>
    /// If the event exists this will call all listening functions
    /// </summary>
    /// <param name="relativeEventOwner">This is if the event is relative to a specific object. Pass in said object.</param>
    /// <param name="eventName">The name of the event</param>
    public static void TriggerEvent(GameObject relativeEventOwner, string eventName)
    {
        eventName += relativeEventOwner.transform.root.name + relativeEventOwner.transform.root.GetHashCode().ToString();

        TriggerEvent(eventName);
    }

    /// <summary>
    /// If the event exists this will call all listening functions
    /// </summary>
    /// <param name="relativeEventOwner">This is if the event is relative to a specific object. Pass in said object.</param>
    /// <param name="eventName">The name of the event</param>
    /// <param name="eventParam">A struct holding prameters relative to the event. See EventParam.</param>
    public static void TriggerEvent(GameObject relativeEventOwner, string eventName, EventParam eventParam)
    {
        eventName += relativeEventOwner.transform.root.name + relativeEventOwner.transform.root.GetHashCode().ToString();

        TriggerEvent(eventName, eventParam);
    }

    /// <summary>
    /// If the event exists this will call all listening functions
    /// </summary>
    /// <param name="eventName">The name of the event</param>
    /// <param name="eventParam">A struct holding prameters relative to the event. See EventParam.</param>
    public static void TriggerEvent(string eventName, EventParam eventParam)
    {
        Action<EventParam> thisEvent = null;
        if (Instance() && Instance().OneParamEventDictionary.TryGetValue(eventName, out thisEvent))
        {
            if (thisEvent != null)
            {
                thisEvent.Invoke(eventParam);
            }
        }
    }
}

public interface EventParam { }

/// <summary>
/// Holds information about the object's death. Currently only who dealt the final blow.
/// </summary>
public struct OnDeathEventParam : EventParam
{
    public OnDeathEventParam(GameObject attacker)
    {
        Attacker = attacker;
    }
    public GameObject Attacker { get; private set; }
}


/// <summary>
/// Triggered whenever something takes damage. Holds info about the damage event like 
/// how much was dealt, what their health is after taking damage, what their max/min health is
/// and who dealt the damage
/// </summary>
public struct OnDamageEventParam : EventParam
{
    public OnDamageEventParam(GameObject attacker, float damageAmount, float currentHealth, float maxHealth = 100.0f, float minHealth = 0.0f)
    {
        Attacker = attacker;
        DamageAmount = damageAmount;
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
        MinHealth = minHealth;
    }

    public float DamageAmount { get; private set; }
    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }
    public float MinHealth { get; private set; }

    public GameObject Attacker { get; private set; }
}

public struct MinigameParam : EventParam
{
    public MinigameParam(GameStateTracker.eMinigameState state)
    {
        GameState = state;
    }

    public GameStateTracker.eMinigameState GameState { get; private set; }
}

public struct OnAudioPausedEventParams : EventParam
{
    public OnAudioPausedEventParams(AudioChannel ChannelObject, bool IsPaused)
    {
        ChannelObjectThatWasAltered = ChannelObject;
        Paused = IsPaused;
    }

    public AudioChannel ChannelObjectThatWasAltered { get; private set; }
    public bool Paused { get; private set; }
}

public struct OnStateChangeParams : EventParam
{
    public OnStateChangeParams(StateBase prev, StateBase current)
    {
        Previous = prev;
        Current = current;
    }

    public StateBase Previous { get; private set; }
    public StateBase Current { get; private set; }
}

public struct SingleBoolParam : EventParam
{
    public SingleBoolParam(bool param)
    {
        Param = param;
    }

    public bool Param { get; private set; }
}

public struct OnVolumeChangeParam : EventParam
{
    public OnVolumeChangeParam(float amount, AudioManager.eChannelType channel)
    {
        Amount = amount;
        Channel = channel;
    }

    public float Amount { get; private set; }
    public AudioManager.eChannelType Channel { get; private set; }
}

public struct OnCheckpointReachedParam : EventParam
{
    public OnCheckpointReachedParam(int index, float currentTime)
    {
        CheckpointIndex = index;
        TimeReached = currentTime;
    }

    public int CheckpointIndex { get; private set; }
    public float TimeReached { get; private set; }
}

public struct OnLevelCompleteParam : EventParam
{
    public OnLevelCompleteParam(float currentTime)
    {
        TimeReached = currentTime;
    }

    public float TimeReached { get; private set; }
}

public struct OnNavigatorChangedParam : EventParam
{
    public OnNavigatorChangedParam(int index, string option, SideWaysNavigator nav, string playerPref)
    {
        OptionIndex = index;
        OptionName = option;
        Nav = nav;
        PlayerPrefValue = playerPref;
    }

    public int OptionIndex { get; private set; }
    public string OptionName { get; private set; }
    public SideWaysNavigator Nav { get; private set; }
    public string PlayerPrefValue { get; private set; }
}

public struct OnSliderChangeParam : EventParam
{
    public OnSliderChangeParam(float value, UnityEngine.UI.Slider slider, string playerPref)
    {
        Value = value;
        ChangedSlider = slider;
        PlayerPrefValue = playerPref;
    }

    public float Value { get; private set; }
    public UnityEngine.UI.Slider ChangedSlider { get; private set; }
    public string PlayerPrefValue { get; private set; }
}

public struct OnToggleChangeParam : EventParam
{
    public OnToggleChangeParam(bool value, UnityEngine.UI.Toggle toggle, string playerPref)
    {
        Value = value;
        ChangedToggle = toggle;
        PlayerPrefValue = playerPref;
    }

    public bool Value { get; private set; }
    public UnityEngine.UI.Toggle ChangedToggle { get; private set; }
    public string PlayerPrefValue { get; private set; }
}

public struct OnUnlockParam : EventParam
{
    public OnUnlockParam(Sprite sprite, string unlockText, int index, string key)
    {
        SpriteOfUnlock = sprite;
        UnlockText = unlockText;
        Key = key;
        Index = index;
    }

    public Sprite SpriteOfUnlock { get; private set; }
    public string UnlockText { get; private set; }
    public string Key { get; private set; }
    public int Index { get; private set; }
}

/// <summary>
/// Called when a binding is changed
/// </summary>
public struct OnChangeKeybindParam : EventParam
{
    public OnChangeKeybindParam(KeyAction action, KeyBindType type, KeyCode code)
    {
        ActionType = action;
        Type = type;
        Code = code;
    }

    public KeyAction ActionType { get; private set; }
    public KeyBindType Type { get; private set; }
    public KeyCode Code { get; private set; }
}