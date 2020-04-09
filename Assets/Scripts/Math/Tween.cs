using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Function handle for Tweening functions. Requires a Start value, an End Value, the current delta time and the total time elapsed.
/// This is all managed by default in the Tween class.
/// </summary>
/// <param name="StartValue"></param>
/// <param name="EndValue"></param>
/// <param name="DeltaTime"></param>
/// <param name="TotalTime"></param>
/// <returns></returns>
public delegate float TweenFunction(float StartValue, float EndValue, float DeltaTime, float TotalTime);
public delegate void TweenCallback();

/// <summary>
/// Wrapper for containing floats used for Tweening.
/// </summary>
/// 
[System.Serializable]
public class TweenableFloat
{
    public TweenableFloat() { }

    public TweenableFloat (float Value)
    {
        this.Value = Value;
    }

    public float Value = 0.0f;
}

/// <summary>
/// Wrapper for containing Vector3 used for Tweening.
/// </summary>
[System.Serializable]
public class TweenableVector3
{
    public TweenableVector3() { }

    public TweenableVector3(float X, float Y, float Z)
    {
        Value = new Vector3(X, Y, Z);
    }

    public TweenableVector3(Vector3 Value)
    {
        this.Value = Value;
    }

    public Vector3 Value = Vector3.zero;
}

[System.Serializable]
public abstract class Tween
{
    public float Duration = 1.0f;
    public float TotalTime = 0.0f;

    TweenFunction Tweener = null;
    TweenCallback ExitFunction = null;
    private bool IsRunning = true;

    public enum eExitMode
    {
        CompleteTweening,
        UndoTweening,
        IncompleteTweening
    }

    public Tween(float Duration, eTweenFunc TweenFunc, TweenCallback ExitFunction)
    {
        this.Duration = Duration;
        Tweener = GetTweenFromEnum(TweenFunc);
        this.ExitFunction = ExitFunction;
        TweenManager.QueueForAddition(this);
    }

    private TweenFunction GetTweenFromEnum(eTweenFunc TweenFunc)
    {
        switch (TweenFunc)
        {
            case eTweenFunc.Linear:
                return MathUtils.TweenFuncs.TweenFunc_Linear;
            case eTweenFunc.SineEaseIn:
                return MathUtils.TweenFuncs.TweenFunc_SineEaseIn;
            case eTweenFunc.SineEaseOut:
                return MathUtils.TweenFuncs.TweenFunc_SineEaseOut;
            case eTweenFunc.SineEaseInOut:
                return MathUtils.TweenFuncs.TweenFunc_SineEaseInOut;
            case eTweenFunc.BounceEaseIn:
                return MathUtils.TweenFuncs.TweenFunc_BounceEaseIn;
            case eTweenFunc.BounceEaseOut:
                return MathUtils.TweenFuncs.TweenFunc_BounceEaseOut;
            case eTweenFunc.BounceEaseInOut:
                return MathUtils.TweenFuncs.TweenFunc_BounceEaseInOut;
            case eTweenFunc.ElasticEaseIn:
                return MathUtils.TweenFuncs.TweenFunc_ElasticEaseIn;
            case eTweenFunc.ElasticEaseOut:
                return MathUtils.TweenFuncs.TweenFunc_ElasticEaseOut;
            case eTweenFunc.ElasticEaseInOut:
                return MathUtils.TweenFuncs.TweenFunc_ElasticEaseInOut;
            case eTweenFunc.LinearToTarget:
                return MathUtils.TweenFuncs.TweenFunc_LinearToTargetValue;
            default:
                Debug.LogWarning("Unknown Tweening Enum passed. Using Linear.");
                return MathUtils.TweenFuncs.TweenFunc_Linear;

        }
    }

    public bool HasTimeRemaining()
    {
        return !(Duration <= TotalTime);
    }

    public bool IsPaused()
    {
        return !IsRunning;
    }

    public float GetTimeRemaining()
    {
        return Duration - TotalTime;
    }

    public void Pause()
    {
        IsRunning = false;
    }

    public void Resume()
    {
        IsRunning = true;
    }

    public void StopTweening(eExitMode ExitMode, bool RunExitFunction = false)
    {
        switch (ExitMode)
        {
            case Tween.eExitMode.CompleteTweening:
                CompleteTween();
                break;
            case Tween.eExitMode.UndoTweening:
                UndoTween();
                break;
            case Tween.eExitMode.IncompleteTweening:
                IncompleteTween();
                break;
            default:
                Debug.LogWarning("Unknown exit condition met. Using IncompleteTweening instead.");
                IncompleteTween();
                break;
        }

        if (RunExitFunction == true)
        {
            ExitFunction?.Invoke();
        }

        TweenManager.QueueForRemoval(this);
    }

    protected virtual void CompleteTween()
    {
        TotalTime = Duration;
        OnUpdate();
    }

    protected virtual void UndoTween()
    {
        TotalTime = 0.0f;
        OnUpdate();
    }

    protected virtual void IncompleteTween()
    {

    }


    // Update is called once per frame
    public void Update()
    {
        if (IsRunning)
        {
            TotalTime += Time.deltaTime;
            OnUpdate();

            if (TotalTime >= Duration)
            {
                ExitFunction?.Invoke();

                TweenManager.QueueForRemoval(this);
                IsRunning = false;
            }
        }
    }

    protected virtual void OnUpdate()
    {

    }

    protected float TweenValue(float StartValue, float EndValue)
    {
        return Tweener(StartValue, EndValue, TotalTime, Duration);
    }
}


public class TweenFloat : Tween
{
    public TweenableFloat StartValue;
    public float EndValue;

    public float StartValueSnapshot;

    public TweenFloat(TweenableFloat StartValue, float EndValue, float Duration, eTweenFunc TweenFunc, TweenCallback ExitFunction)
        : base(Duration, TweenFunc, ExitFunction)
    {
        Reset(StartValue, EndValue);  
    }

    protected override void OnUpdate()
    {
        StartValue.Value = TweenValue(StartValueSnapshot, EndValue);
    }

    public void Reset(TweenableFloat StartValue, float EndValue)
    {
        this.StartValue = StartValue;
        this.EndValue = EndValue;
        TotalTime = 0.0f;

        StartValueSnapshot = StartValue.Value;
    }
}

[System.Serializable]
public class TweenVector3 : Tween
{
    public TweenableVector3 StartVector;
    public Vector3 EndVector;

    public Vector3 StartValueSnapshot;

    public TweenVector3(TweenableVector3 StartValue, Vector3 EndValue, float Duration, eTweenFunc TweenFunc, TweenCallback ExitFunction)
    : base(Duration, TweenFunc, ExitFunction)
    {
        Reset(StartValue, EndValue);     
    }

    public void Reset(TweenableVector3 StartValue, Vector3 EndValue)
    {
        this.StartVector = StartValue;
        this.EndVector = EndValue;
        TotalTime = 0.0f;

        StartValueSnapshot = StartValue.Value;
    }

    protected override void OnUpdate()
    {
        StartVector.Value.x = TweenValue(StartValueSnapshot.x, EndVector.x);
        StartVector.Value.y = TweenValue(StartValueSnapshot.y, EndVector.y);
        StartVector.Value.z = TweenValue(StartValueSnapshot.z, EndVector.z);      
    }
}
