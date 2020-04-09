using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

[System.Serializable]
public class StateEvent : UnityEvent<StateChangeStruct> { }

[RequireComponent(typeof(Character))]
public abstract class StateMachineBase : BaseObject
{
    [Header("State Machine Settings"), Tooltip("The list of States this State Machine contains.")]
    public List<StateBase> ListOfStates = new List<StateBase>();

    [Tooltip("Drag the starting state here. If not set this will default to index 0 in the List")]
    public StateBase InitialState = null;

    [Tooltip("The current active State.")]
    public StateBase CurrentState = null;

    [Tooltip("Which audio channel will this state machine play audio from?")]
    public AudioChannel m_AudioChannel = null;

    protected int StateIndex = 0;

    [HideInInspector]
    public int GroundCheckMask;

    StateEvent StateChangeUnityEvent;

    StateBase QueuedState = null;

    
    protected virtual void Start()
    {
        if (ListOfStates.Count == 0)
        {
            Debug.LogWarning(string.Format("State machine is empty inside {0}", gameObject.name));
            return;
        }

        bool usingInitialState = false;
        if (InitialState != null)
        {
            if (ListOfStates.Contains(InitialState) == false)
            {
                Debug.LogError(string.Format("State machine contains an initial state that doesn't exist in the list of states. " +
                    "Initial state: {0}, GameObject: {1}", InitialState.name, gameObject.name));
            }
            else
            {
                usingInitialState = true;
                CurrentState = InitialState;
            }
        }

        for (int i = 0; i < ListOfStates.Count; i++)
        {
            ListOfStates[i].SM = this;
            ListOfStates[i].AudioChannelObject = m_AudioChannel;
            ListOfStates[i].Start();

            if (usingInitialState && InitialState == ListOfStates[i])
            {
                StateIndex = i;
                CurrentState = ListOfStates[i];
            }
        }

        Listen(gameObject, "OnRespawnEvent", OnRespawnEvent);
        Listen(gameObject, "OnDeathEvent", OnDeathEvent);
    }


    protected override void Awake()
    {
        base.Awake();

        GroundCheckMask = ~LayerMask.GetMask("Player", "Ignore Raycast");

        if (StateChangeUnityEvent == null)
        {
            StateChangeUnityEvent = new StateEvent();
        }
    }

    private void LateUpdate()
    {
        ListOfStates[StateIndex].LateUpdate();
        //Debug.Log("State: " + ListOfStates[StateIndex]);

        if(QueuedState != null)
        {
            SetQueuedMovementState();
        }
    }

    public T GetState<T>() where T : StateBase
    {
        var lookup = ListOfStates.Where(x => x.Is<T>());

        if (lookup.Any())
        {
            return lookup.First() as T;
        }
        else
        {
            Debug.LogWarning("State of type " + typeof(T) + " does not exist in this state machine.");
            return null;
        }
    }
 
    public void SetMovementState<T>() where T : StateBase
    {
        StateBase newState = GetState<T>();
        if (newState == null)
        {
            Debug.LogWarning("State switch attempted with a state that doesn't exist: " + typeof(T));
            return;
        }
        else if(newState == GetActiveState())
        {
            Debug.LogWarning("Same state yo: " + typeof(T));
            return;
        }

        if (QueuedState != null)
        {
            Debug.LogWarning("2 state events in 1 frame.");
        }

        QueuedState = newState;
    }

    public void SetMovementStateForced<T>() where T : StateBase
    {
        SetMovementState<T>();
        SetQueuedMovementState();
    }


    protected void SetQueuedMovementState()
    {
        StateBase previousState = GetActiveState();
        previousState.DeactivateState();

        //get our index
        for (int i = 0; i < ListOfStates.Count; i++)
        {
            if (QueuedState == ListOfStates[i])
            {
                StateIndex = i;
                break;
            }
        }

        CurrentState = QueuedState;
        QueuedState.ActivateState();

        //old dispatch method REMOVE
        StateChangeStruct changeStateEvent = new StateChangeStruct(previousState, QueuedState);
        StateChangeUnityEvent.Invoke(changeStateEvent);
        SendMessageUpwards("RecieveStateChangeEvent", changeStateEvent, SendMessageOptions.DontRequireReceiver);

        //new dispatch method
        OnStateChangeParams stateChangeParam = new OnStateChangeParams(previousState, QueuedState);
        EventManager.TriggerEvent(gameObject, "OnStateChangeEvent", stateChangeParam);

        QueuedState = null;
    }


    public StateBase GetActiveState()
    {
        return ListOfStates[StateIndex];
    }

    /// <summary>
    /// to subscribe your function MUST have the argument of type StateChangeStruct
    /// </summary>
    /// <param name="functionToCall"></param>
    public void SubscribeToStateChange(UnityAction<StateChangeStruct> functionToCall)
    {
        StateChangeUnityEvent.AddListener(functionToCall);
    }

    public void UnsubscribeToStateChange(UnityAction<StateChangeStruct> functionToCall)
    {
        StateChangeUnityEvent.RemoveListener(functionToCall);
    }


    protected abstract void OnDeathEvent(EventParam param);
    protected abstract void OnRespawnEvent();
}

public struct StateChangeStruct
{
    public StateChangeStruct(StateBase prev, StateBase current)
    {
        PreviousState = prev;
        CurrentState = current;
    }

    public StateBase PreviousState { get; private set; }
    public StateBase CurrentState { get; private set; }
}