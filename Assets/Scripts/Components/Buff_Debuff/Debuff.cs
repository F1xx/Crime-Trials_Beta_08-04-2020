using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Debuff
{
    public Debuff(float duration, DebuffSystem system, GameObject cause, System.Action functiontocallOndeath = null)
    {
        Duration = duration;
        DebuffSys = system;
        Causer = cause;
        CallOnDeath = functiontocallOndeath;

        TimerManager.MakeAutoStartTimer(duration, DestroyDebuff);
    }

    /// <summary>
    /// THIS MUST BE CALLED
    /// if not called ALL debuffs will be a memory leak.
    /// </summary>
    public void DestroyDebuff()
    {
        TimerManager.QueueForRemoval(DurationTimer);
        DurationTimer = null;
        DebuffSys.QueueForRemoval(this);

        if(CallOnDeath != null)
        {
            CallOnDeath.Invoke();
        }
    }

    public bool Is<T>() where T : Debuff
    {
        return GetType() == typeof(T);
    }

    public float Duration { get; protected set; }
    public Timer DurationTimer { get; private set; }
    public DebuffSystem DebuffSys { get; private set; }
    public GameObject Causer { get; private set; }
    public System.Action CallOnDeath { get; private set; }
}