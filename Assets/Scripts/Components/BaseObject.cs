using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// The CrimeTrials base class. Has built-in hooks to the EventManager and TimerManager as well as preset event handles for 
/// SoftReset and HardReset.
/// </summary>
[System.Serializable]
public abstract class BaseObject : MonoBehaviour
{

    #region Event Variables

    [Header("Events")]
    [SerializeField, Tooltip("Set whether or not this object will respond to game-wide OnSoftReset events.")]
    private bool _ListenToSoftResetEvents = true;

    [SerializeField, Tooltip("Set whether or not this object will respond to game-wide OnHardReset events.")]
    private bool _ListenToHardResetEvents = false;

    public bool ListenToSoftResetEvents { get { return _ListenToSoftResetEvents; } set { SetListenToResetEvents(value, false, true); _ListenToSoftResetEvents = value; } }
    public bool ListenToHardResetEvents { get { return _ListenToHardResetEvents; } set { SetListenToResetEvents(value, false, false); _ListenToHardResetEvents = value; } }

    [SerializeField]
    private List<OneParamEventBinding> OneParamEvents = new List<OneParamEventBinding>();
    [SerializeField]
    private List<NoParamEventBinding> NoParamEvents = new List<NoParamEventBinding>();

    #endregion

    #region Timer Variables

    [SerializeField, Header("Timers"), Tooltip("List of timers managed by this object.")]
    private List<Timer> m_Timers = new List<Timer>();

    #endregion

    /// <summary>
    /// This is the parent object.
    /// </summary>
    public GameObject ParentObject { get; set; }

    protected virtual void Awake()
    {
        SetListenToResetEvents(ListenToSoftResetEvents, true, true);
        SetListenToResetEvents(ListenToHardResetEvents, true, false);
    }

    /// <summary>
    /// Function that is automatically called as part of setting BaseObject reset bools. Allows for quick and easy
    /// subscribing/unsubscribing from reset events in the EventManager.
    /// </summary>
    /// <param name="SetValue">The raw value of the bool.</param>
    /// <param name="RunOnce">If this is part of the run-once sequence.</param>
    /// <param name="IsSoftReset">Is this for soft reset or hard reset?</param>
    private void SetListenToResetEvents(bool SetValue, bool RunOnce, bool IsSoftReset)
    {
        bool toCompare = IsSoftReset ? ListenToSoftResetEvents : ListenToHardResetEvents;

        //Ignore same set calls unless it's the awake call.
        if (SetValue == toCompare && RunOnce == false)
        {
            return;
        }

        //Link the delegate if we're listening
        if (SetValue == true)
        {
            if (IsSoftReset)
            {
                Listen("OnSoftReset", SoftReset);
            }
            else
            {
                Listen("OnHardReset", HardReset);
            }
        }

        //Unlink the delegate if we're not listening anymore
        else if (RunOnce == false && SetValue == false)
        {
            if (IsSoftReset)
            {
                StopListen("OnSoftReset", SoftReset);
            }
            else
            {
                StopListen("OnHardReset", HardReset);
            }
        }
    }

    #region Listening

    /// <summary>
    /// Subscribe to an internal function that is managed by EventManager.
    /// </summary>
    /// <param name="EventName"></param>
    /// <param name="Func"></param>
    public void Listen(GameObject Obj, string EventName, System.Action<EventParam> Func)
    {
        if (GetExistingOneEventParam(Obj, EventName, Func) == null)
        {
            if (Obj != null)
            {
                EventManager.StartListening(Obj, EventName, Func);
            }
            else
            {
                EventManager.StartListening(EventName, Func);
            }

            OneParamEvents.Add(new OneParamEventBinding(Obj, EventName, Func));
        }
    }

    /// <summary>
    /// Subscribe to an internal function that is managed by EventManager.
    /// </summary>
    /// <param name="EventName"></param>
    /// <param name="Func"></param>
    public void Listen(GameObject Obj, string EventName, System.Action Func)
    {
        if (GetExistingNoEventParam(Obj, EventName, Func) == null)
        {
            if (Obj != null)
            {
                EventManager.StartListening(Obj, EventName, Func);
            }
            else
            {
                EventManager.StartListening(EventName, Func);
            }

            NoParamEvents.Add(new NoParamEventBinding(Obj, EventName, Func));
        }
    }

    /// <summary>
    /// Subscribe to an external function that is managed by EventManager.
    /// </summary>
    /// <param name="EventName"></param>
    /// <param name="Func"></param>
    public void Listen(string EventName, System.Action Func)
    {
        Listen(null, EventName, Func);
    }

    /// <summary>
    /// Subscribe to an external function that is managed by EventManager.
    /// </summary>
    /// <param name="EventName"></param>
    /// <param name="Func"></param>
    public void Listen(string EventName, System.Action<EventParam> Func)
    {
        Listen(null, EventName, Func);
    }

    #endregion

    #region Stop Listening

    /// <summary>
    /// Cleanup to remove a function from an EventManager event.
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public void StopListen(GameObject Obj, string EventName, System.Action<EventParam> Func)
    {
        var foundParam = GetExistingOneEventParam(Obj, EventName, Func);
        if (StopListen(foundParam) == false)
        {
            Debug.LogWarning(string.Format("{0} is not listening to event {1}", gameObject.name, EventName));
        }
    }

    public void StopListen(string EventName, System.Action<EventParam> Func)
    {
        var foundParam = GetExistingOneEventParam(null, EventName, Func);
        if (StopListen(foundParam) == false)
        {
            Debug.LogWarning(string.Format("{0} is not listening to event {1}", gameObject.name, EventName));
        }
    }

    /// <summary>
    /// Cleanup to remove a function from an EventManager event.
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    private bool StopListen(OneParamEventBinding param)
    {
        if (param != null)
        {
            if (param.Obj == null)
            {
                EventManager.StopListening(param.EventName, param.Func);
            }
            else
            {
                EventManager.StopListening(param.Obj, param.EventName, param.Func);
            }

            OneParamEvents.Remove(param);
            OneParamEvents.TrimExcess();
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Cleanup to remove a function from an EventManager event.
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public void StopListen(GameObject Obj, string EventName, System.Action Func)
    {
        var foundParam = GetExistingNoEventParam(Obj, EventName, Func);
        if(StopListen(foundParam) == false)
        {
            Debug.LogWarning(string.Format("{0} is not listening to event {1}", gameObject.name, EventName));
        }
    }

    /// <summary>
    /// Cleanup to remove a function from an EventManager event.
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public void StopListen(string EventName, System.Action Func)
    {
        StopListen(null, EventName, Func);
    }

    /// <summary>
    /// Cleanup to remove a function from an EventManager event.
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    private bool StopListen(NoParamEventBinding param)
    {
        if (param != null)
        {
            if (param.Obj == null)
            {
                EventManager.StopListening(param.EventName, param.Func);
            }
            else
            {
                EventManager.StopListening(param.Obj, param.EventName, param.Func);
            }

            NoParamEvents.Remove(param);
            NoParamEvents.TrimExcess();
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion

    #region Event Bindings

    /// <summary>
    /// Internally used to check if a particular OneParam Event subscription exists in this GameObject.
    /// </summary>
    /// <param name="Obj"></param>
    /// <param name="EventName"></param>
    /// <param name="Func"></param>
    /// <returns></returns>
    private OneParamEventBinding GetExistingOneEventParam(GameObject Obj, string EventName, System.Action<EventParam> Func)
    {
        var foundParam = OneParamEvents.Where(x => x.Obj == Obj && x.EventName == EventName && x.Func == Func).ToList();

        if (foundParam.Any())
        {
            return foundParam[0];
        }

        return null;
    }

    /// <summary>
    /// Internally used to check if a particular NoParam Event subscription exists in this GameObject.
    /// </summary>
    /// <param name="Obj"></param>
    /// <param name="EventName"></param>
    /// <param name="Func"></param>
    /// <returns></returns>
    private NoParamEventBinding GetExistingNoEventParam(GameObject Obj, string EventName, System.Action Func)
    {
        var foundParam = NoParamEvents.Where(x => x.Obj == Obj && x.EventName == EventName && x.Func == Func).ToList();

        if (foundParam.Any())
        {
            return foundParam[0];
        }

        return null;
    }

    /// <summary>
    /// Wrapper for a OneParam event binding.
    /// </summary>
    [System.Serializable]
    private class OneParamEventBinding
    {
        public OneParamEventBinding(GameObject OwnerObj, string Name, System.Action<EventParam> FunctionHandle)
        {
            Obj = OwnerObj;
            EventName = Name;
            Func = FunctionHandle;
        }

        public GameObject Obj;
        public string EventName;
        public System.Action<EventParam> Func;
    }

    /// <summary>
    /// Wrapper for a NoParam event binding.
    /// </summary>
    [System.Serializable]
    private class NoParamEventBinding
    {
        public NoParamEventBinding(GameObject OwnerObj, string Name, System.Action FunctionHandle)
        {
            EventName = Name;
            Func = FunctionHandle;
            Obj = OwnerObj;
        }

        public GameObject Obj;
        public string EventName;
        public System.Action Func;
    }

    #endregion

    #region Make Timers

    /// <summary>
    /// Create a timer. This timer is internally managed by BaseObject and will be destroyed when this object is destroyed.
    /// </summary>
    /// <param name="timerLength">The length of the timer.</param>
    /// <param name="functionToCall">The function to call when the timer completes.</param>
    /// <param name="AutoStart">Should this timer run as soon as it's created?</param>
    /// <param name="Oneshot">Should this timer only exist for 1 loop?</param>
    /// <returns></returns>
    public Timer CreateTimer(float timerLength, System.Action functionToCall = null, bool AutoStart = false, bool Oneshot = false, bool ResetOnSoftReset = false)
    {
        Timer timer;

        if (AutoStart == false)
            if (Oneshot == false)
                timer = TimerManager.MakeTimer(timerLength, functionToCall, ResetOnSoftReset);
            else
                timer = TimerManager.MakeOneShotTimer(timerLength, functionToCall, ResetOnSoftReset);
        else
            if (Oneshot == false)
                timer = TimerManager.MakeAutoStartTimer(timerLength, functionToCall, ResetOnSoftReset);
            else
                timer = TimerManager.MakeOneShotAutoStartTimer(timerLength, functionToCall, ResetOnSoftReset);

        m_Timers.Add(timer);

        return timer;
    }

    #endregion

    void SoftReset()
    {
        OnSoftReset();
    }

    protected virtual void OnSoftReset() { }

    void HardReset()
    {
        OnHardReset();
    }

    protected virtual void OnHardReset() { }

    protected virtual void OnDestroy()
    {
        //Unsubscribe from all OneParam events.
        while (OneParamEvents.Count > 0)
        {
            if (StopListen(OneParamEvents[0]) == false)
            {
                Debug.LogError(string.Format("ERROR detaching from events on {0}:OnDestroy", gameObject.name));
                break;
            }
        }
        //Unsubscribe from all NoParam events.
        while (NoParamEvents.Count > 0)
        {
            if (StopListen(NoParamEvents[0]) == false)
            {
                Debug.LogError(string.Format("ERROR detaching from events on {0}:OnDestroy", gameObject.name));
                break;
            }
        }

        //Unsubscribe all timers
        while (m_Timers.Count > 0)
        {
            TimerManager.QueueForRemoval(m_Timers[0]);
            m_Timers.RemoveAt(0);
        }
    }

}
