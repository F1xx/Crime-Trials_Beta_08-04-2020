using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldTimeManager : Singleton<WorldTimeManager>
{
    /// <summary>
    /// This is the actual time passed in the game. Note it is just a float in milliseconds.
    /// Get Formatted text for s string.
    /// </summary>
    public static float TimePassed { get; private set; }

    /// <summary>
    /// Whether or not the world timer is actually running
    /// </summary>
    public static bool IsRunning { get; private set; }

    protected override void OnAwake()
    {

    }

    void Update()
    {
        if (IsRunning)
        {
            TimePassed += Time.deltaTime;
        }
    }

    public static string GetTimeAsFormattedString()
    {
        int intTime = (int)TimePassed;
        int minutes = intTime / 60;
        int seconds = intTime % 60;
        float milliseconds = TimePassed * 1000;
        milliseconds = (milliseconds % 1000);

        return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }

    public static string GetTimeAsFormattedString(float time)
    {
        int intTime = (int)time;
        int minutes = intTime / 60;
        int seconds = intTime % 60;
        float milliseconds = time * 1000;
        milliseconds = (milliseconds % 1000);

        return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }

    public static void AddTime(float time)
    {
        TimePassed += time;
    }

    public static void RemoveTime(float time)
    {
        TimePassed -= time;
    }

    public static void StopTimer()
    {
        IsRunning = false;
    }

    public static void StartTimer()
    {
        IsRunning = true;
    }

    /// <summary>
    /// Turns the timer off and sets the passed time to 0
    /// </summary>
    public static void Reset()
    {
        TimePassed = 0.0f;
        IsRunning = false;
        EventManager.TriggerEvent("WorldTimerManagerReset");
    }

    protected override void OnHardReset()
    {
        Reset();
    }

    protected override void OnSoftReset()
    {
        //Punish player with adding time on soft reset?
        StartTimer();
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        base.OnSceneLoad(scene, mode);

        Reset();
    }
}
