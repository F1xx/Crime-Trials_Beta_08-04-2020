using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

[System.Serializable]
public class Timer
{
    public bool IsRunning { get; private set; }
    public float Duration { get; private set; }
    public bool IsOneShot = false;
    public bool DoesLoop = false;
    public int ID = 0;

    public bool ShouldResetOnSoftReset = false;
    public bool UpdateUnscaled = false;

    float m_TimeRemaining;

    List<Action> ListOfFunctions = new List<Action>();

    public Timer(int id, float timerLength, Action functionToCall = null, bool ResetsOnSoftReset = false)
    {
        Duration = timerLength;
        m_TimeRemaining = Duration;
        ShouldResetOnSoftReset = ResetsOnSoftReset;
        ID = id;

        AddListener(functionToCall);
        TimerManager.QueueForAddition(this);
    }

    ~Timer()
    {
        Destroy();
    }

    public void Destroy()
    {
        if (ListOfFunctions.Count > 0)
        {
            foreach (var func in ListOfFunctions)
            {
                EventManager.StopListening("OnTimerComplete" + ID.ToString(), func);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        IsRunning = false;
    }

    // Update is called once per frame
    public void Update()
    {
        if (IsRunning)
        {
            if (UpdateUnscaled)
            {
                m_TimeRemaining -= Time.unscaledDeltaTime;
            }
            else
            {
                m_TimeRemaining -= Time.deltaTime;
            }

            if (m_TimeRemaining <= 0)
            {
                Reset();
                EventManager.TriggerEvent("OnTimerComplete" + ID.ToString());
            }
        }
    }

    //MUST be called for the timer to run
    public void StartTimer()
    {
        IsRunning = true;
    }

    //More like a pause. Does NOT restart the timer.
    public void StopTimer()
    {
        IsRunning = false;
    }

    //returns the percentage complete. Will be a number from 0.0f to 1.0f
    public float GetPercentageComplete()
    {
        float passed = Duration - m_TimeRemaining;

        if(passed == 0.0f)
        {
            return 1.0f;
        }

        return passed / Duration;
    }

    public float GetTimeRemaining()
    {
        return m_TimeRemaining;
    }

    //add a function to be called when the timer ends
    public void AddListener(Action functionToCall)
    {
        if (functionToCall != null)
        {
            if (ListOfFunctions.Where(x => x == functionToCall).ToList().Any() == false)
            {
                ListOfFunctions.Add(functionToCall);
                EventManager.StartListening("OnTimerComplete" + ID.ToString(), functionToCall);
            }
            else
            {
                Debug.LogWarning(string.Format("Function {0} is already subscribed to Timer ID:{1}", functionToCall.Method.Name, ID));
            }
        }
    }

    public void RemoveListener(Action functionToRemove)
    {
        if (functionToRemove != null)
        {
            if (ListOfFunctions.Where(x => x == functionToRemove).ToList().Any() == false)
            {
                ListOfFunctions.Remove(functionToRemove);
                EventManager.StopListening("OnTimerComplete" + ID.ToString(), functionToRemove);
            }
            else
            {
                Debug.LogWarning(string.Format("Function {0} is already subscribed to Timer ID:{1}", functionToRemove.Method.Name, ID));
            }
        }
    }

    //Sets a new duration for the timer
    //Resets the timer
    public void SetDuration(float dur)
    {
        Duration = dur;

        if(IsRunning)
        {
            Reset();
        }

    }

    public void Restart()
    {
        Reset();
        StartTimer();
    }

    //resets variables
    //if IsOneShot is true it will be destroyed here
    //if Looping it will not turn stop running
    public void Reset()
    {
        if(IsOneShot)
        {
            TimerManager.QueueForRemoval(this);
        }

        if (!DoesLoop)
        {
            IsRunning = false;
        }

        m_TimeRemaining = Duration;
    }

    public bool IsTimerComplete()
    {
        return GetPercentageComplete() >= 1.0f;
    }


}
