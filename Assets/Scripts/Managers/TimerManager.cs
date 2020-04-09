using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

public class TimerManager : Singleton<TimerManager>
{
    private List<Timer> m_ActiveTimers = new List<Timer>();
    private HashSet<Timer> m_TimersToRemove = new HashSet<Timer>();
    private HashSet<Timer> m_TimersToAdd = new HashSet<Timer>();

    int TimerCount = 0;

    protected override void OnAwake()
    {
        DontDestroyOnLoad(this);
    }

    public static Timer MakeTimer(float timerLength, Action functionToCall = null, bool ResetsOnSoftReset = false)
    {
        Timer timer = Instance().CreateTimer(timerLength, functionToCall, ResetsOnSoftReset);
        return timer;
    }

    public static Timer MakeOneShotTimer(float timerLength, Action functionToCall = null, bool ResetsOnSoftReset = false)
    {
        Timer tim = Instance().CreateTimer(timerLength, functionToCall, ResetsOnSoftReset);
        tim.IsOneShot = true;
        return tim;
    }

    public static Timer MakeOneShotAutoStartTimer(float timerLength, Action functionToCall = null, bool ResetsOnSoftReset = false)
    {
        Timer tim = Instance().CreateTimer(timerLength, functionToCall, ResetsOnSoftReset);
        tim.IsOneShot = true;
        tim.StartTimer();
        return tim;
    }

    public static Timer MakeAutoStartTimer(float timerLength, Action functionToCall = null, bool ResetsOnSoftReset = false)
    {
        Timer tim = Instance().CreateTimer(timerLength, functionToCall, ResetsOnSoftReset);
        tim.StartTimer();
        return tim;
    }

    private Timer CreateTimer(float timerLength, Action functionToCall = null, bool ResetsOnSoftReset = false)
    {
        Timer tim = new Timer(TimerCount, timerLength, functionToCall, ResetsOnSoftReset);
        TimerCount++;
        return tim;
    }

    private static void RemoveTimer(Timer TimerObject)
    {
        TimerManager instance = Instance();

        if (instance && TimerObject != null)
        {
            var timers = instance.m_ActiveTimers.Where(x => x == TimerObject).ToList();
            if (timers.Any())
            {
                TimerObject.Destroy();
                instance.m_ActiveTimers.Remove(TimerObject);
            }
        }
    }

    private void RegisterTimer(Timer timer)
    {
        m_ActiveTimers.Add(timer);
    }

    public static int GetTimerIndex(Timer timer)
    {
        return Instance().m_ActiveTimers.BinarySearch(timer);
    }

    //You better know the index of the timer
    public static Timer GetTimer(int index)
    {
        if (index < 0 || index >= Instance().m_ActiveTimers.Count)
        {
            Debug.LogWarning("Timer manager: index out of range");
            return null;
        }

        return Instance().m_ActiveTimers[index];
    }

    public static void QueueForRemoval(Timer timerObject)
    {
        TimerManager instance = Instance();

        if (instance && timerObject != null)
        {
            instance.m_TimersToRemove.Add(timerObject);
        }
    }

    public static void QueueForAddition(Timer TweenObject)
    {
        TimerManager instance = Instance();

        if (instance)
        {
            Instance().m_TimersToAdd.Add(TweenObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Timer t in m_ActiveTimers)
        {
            t.Update();
        }
    }

    private void LateUpdate()
    {
        foreach (var timer in m_TimersToRemove)
        {
            RemoveTimer(timer);
        }
        m_TimersToRemove.Clear();

        foreach (var timer in m_TimersToAdd)
        {
            RegisterTimer(timer);
        }
        m_TimersToAdd.Clear();

        m_ActiveTimers.TrimExcess();
    }

    protected override void OnSoftReset()
    {
        base.OnSoftReset();

        foreach (var timer in m_ActiveTimers)
        {
            if (timer.ShouldResetOnSoftReset)
            {
                timer.Reset();
            }
        }
    }
}
