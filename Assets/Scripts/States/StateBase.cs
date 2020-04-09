using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base StateBase Class. Please follow these rules for writing new children with their summary:
/// 1. Name the class
/// 2. List various switch state conditions in order of priority.
/// </summary>
/// 
[System.Serializable]
public abstract class StateBase : ScriptableObject
{
    [HideInInspector]
    public StateMachineBase SM = null;

    [HideInInspector]
    public AudioChannel AudioChannelObject = null;

    protected HealthComponent m_Health;


    [Header("Audio"), Tooltip("If set: this State will place the specified Audio when the State is activated.")]
    public ScriptableAudio AudioToPlayOnActivated = null;

    [Tooltip("If set: this State will place the specified Audio when the State is deactivated.")]
    public ScriptableAudio AudioToPlayOnDeactivated = null;


    public virtual void Start()
    {
        m_Health = SM.GetComponent<HealthComponent>();
    }

    public virtual void ActivateState()
    {
        if (AudioToPlayOnActivated && AudioChannelObject)
        {
            AudioChannelObject.PlayAudio(AudioToPlayOnActivated);
        }
    }

    public virtual void DeactivateState()
    {
        if (AudioToPlayOnDeactivated && AudioChannelObject)
        {
            AudioChannelObject.PlayAudio(AudioToPlayOnDeactivated);
        }
    }

    //note this is a fake LateUpdate and not called by the system
    //we want the state to update after everything else has had time to update itself and go from there
    public abstract void LateUpdate();

    //Compare against type
    public bool Is<T>() where T : StateBase
    {
        return GetType() == typeof(T);
    }
}
