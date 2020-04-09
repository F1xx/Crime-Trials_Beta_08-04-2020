using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum eTweenFunc
{
    Linear,
    LinearToTarget,
    SineEaseIn,
    SineEaseOut,
    SineEaseInOut,
    BounceEaseIn,
    BounceEaseOut,
    BounceEaseInOut,
    ElasticEaseIn,
    ElasticEaseOut,
    ElasticEaseInOut
}

public class TweenManager : Singleton<TweenManager>
{
    public List<Tween> m_ActiveTweens = new List<Tween>();
    private HashSet<Tween> m_TweensToRemove = new HashSet<Tween>();
    private HashSet<Tween> m_TweensToAdd = new HashSet<Tween>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var tween in m_ActiveTweens)
        {
            tween.Update();
        }
    }

    /// <summary>
    /// Clean the list of any outdated Tweens during LateUpdate.
    /// </summary>
    private void LateUpdate()
    {

        foreach (var tween in m_TweensToRemove)
        {
            RemoveTween(tween);
        }
        m_TweensToRemove.Clear();

        foreach (var tween in m_TweensToAdd)
        {
            RegisterTween(tween);
        }
        m_TweensToAdd.Clear();
    }

    protected override void OnAwake(){}

    /// <summary>
    /// Create a Tween for floats.
    /// Returns a reference to the Tween if you want to use it but by default is managed by the TweenManager.
    /// </summary>
    /// <param name="Start"></param>
    /// <param name="End"></param>
    /// <param name="Duration"></param>
    /// <param name="TweenFunc"></param>
    /// <param name="ExitFunction"></param>
    /// <returns></returns>
    public static Tween CreateTween(TweenableFloat Start, float End, float Duration, eTweenFunc TweenFunc, TweenCallback ExitFunction = null)
    {
        Tween tween = new TweenFloat(Start, End, Duration, TweenFunc, ExitFunction);
        return tween;
    }

    /// <summary>
    /// Create a Tween for Vector3s.
    /// Returns a reference to the Tween if you want to use it but by default is managed by the TweenManager.
    /// </summary>
    /// <param name="Start"></param>
    /// <param name="End"></param>
    /// <param name="Duration"></param>
    /// <param name="TweenFunc"></param>
    /// <param name="ExitFunction"></param>
    /// <returns></returns>
    public static Tween CreateTween(TweenableVector3 Start, Vector3 End, float Duration, eTweenFunc TweenFunc, TweenCallback ExitFunction = null)
    {
        Tween tween = new TweenVector3(Start, End, Duration, TweenFunc, ExitFunction);
        return tween;
    }

    public static void RegisterTween(Tween TweenObject)
    {
        Instance().m_ActiveTweens.Add(TweenObject);
    }

    /// <summary>
    /// Unregister a Tween from the TweenManager. Tweens automatically call this function when they expire.
    /// </summary>
    /// <param name="TweenObject"></param>
    public static void QueueForRemoval(Tween TweenObject)
    {
        Instance().m_TweensToRemove.Add(TweenObject);

    }

    public static void QueueForAddition(Tween TweenObject)
    {
        Instance().m_TweensToAdd.Add(TweenObject);
    }

    private void RemoveTween(Tween TweenObject)
    {
        for (int i = 0; i < m_ActiveTweens.Count; i++)
        {
            if (TweenObject == m_ActiveTweens[i])
            {
                m_ActiveTweens.RemoveAt(i);
                return;
            }
        }

        //Debug.LogWarning("Tween that didn't exist in TweenManager attempted to be removed.");
    }
}
