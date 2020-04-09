using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is a container class which is used to handle and maintain the impulses and Tweens that the PlayerController uses
/// Unless you need to ZeroOut then its mostly just make it and it will do it's own thing. Aside from that it is just disposal to ensure no memory leaks.
/// </summary>
public class PlayerImpulse : System.IDisposable
{
    public enum eImpulseType
    {
        Basic,
        Slide
    }

    public PlayerImpulse(Vector3 force, float duration, eTweenFunc forceMode = eTweenFunc.Linear, eImpulseType type = eImpulseType.Basic)
    {
        Force = force;
        TweenableForce = new TweenableVector3(Force);
        Duration = duration;
        ForceMode = forceMode;
        ImpulseType = type;
        TweenedImpulse = (TweenVector3)TweenManager.CreateTween(TweenableForce, Vector3.zero, Duration, ForceMode);
    }

    public void ZeroOutY()
    {
        Duration = TweenedImpulse.Duration - TweenedImpulse.TotalTime;
        Vector3 force = TweenedImpulse.StartVector.Value;
        force.y = 0.0f;
        Force = force;

        TweenedImpulse.StopTweening(Tween.eExitMode.IncompleteTweening);

        TweenableForce.Value = Force;
        TweenedImpulse = (TweenVector3)TweenManager.CreateTween(TweenableForce, Vector3.zero, Duration, ForceMode);
    }

    public void ScaleImpulse(float scale, float newDuration)
    {
        Force = TweenedImpulse.StartValueSnapshot * scale;
        TweenableForce.Value = Force;

        TweenedImpulse.Reset(TweenableForce, Vector3.zero);
        TweenedImpulse.Duration = newDuration;
        TweenedImpulse.Resume();
    }

    public void Dispose()
    {
        TweenableForce = null;

        if (TweenedImpulse != null)
        {
            TweenedImpulse.StopTweening(Tween.eExitMode.IncompleteTweening);
            TweenedImpulse = null;
        }
    }

    public Vector3 Force { get; private set; }
    public float Duration { get; private set; }
    public TweenableVector3 TweenableForce { get; private set; }
    public eTweenFunc ForceMode { get; private set; }
    public TweenVector3 TweenedImpulse { get; private set; }
    public eImpulseType ImpulseType { get; private set; }
}
