using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

public class LevelSelectButton : BaseObject, ISelectHandler, IDeselectHandler, IPointerClickHandler, ISubmitHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Video Player Stuff"), SerializeField]
    CanvasGroup VideoGroup = null;
    VideoPlayer Player = null;
    [SerializeField]
    CanvasGroup BorderGroup = null;
    [SerializeField]
    float FadeInDuration = 0.7f;
    [SerializeField]
    float FadeOutDuration = 1.0f;

    [Header("Loading Stuff"), SerializeField]
    string LevelToLoad = "";
    MenuBase MenuBaseScript = null;

    //Tweens
    TweenableFloat VideoAlpha;
    TweenFloat CurrentTweening = null;

    protected override void Awake()
    {
        Player = GetComponent<VideoPlayer>();
        MenuBaseScript = GameObject.Find("MenuScript").GetComponent<MenuBase>();

        BorderGroup.alpha = 0.0f;
        VideoGroup.alpha = 0.0f;
        VideoAlpha = new TweenableFloat(VideoGroup.alpha);
    }

    private void Update()
    {
        if (CurrentTweening != null)
        {
            VideoGroup.alpha = VideoAlpha.Value;
        }
    }

    #region Interface Handlers
    public void OnSelect(BaseEventData eventData)
    {
        Player.Play();
        BorderGroup.alpha = 1.0f;

        //get the remaining duration. Might be the full value or it might be less depending on if this happens mid-tween.
        float dur = FadeInDuration - (FadeInDuration * VideoGroup.alpha);
        CreateTween(1.0f, dur);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        Player.Stop();
        BorderGroup.alpha = 0.0f;

        //get the remaining duration. Might be the full value or it might be less depending on if this happens mid-tween.
        float dur = FadeOutDuration - (FadeOutDuration * (1 - VideoGroup.alpha));
        CreateTween(0.0f, dur);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if(!string.IsNullOrEmpty(LevelToLoad))
        {
            MenuBaseScript.LoadScene(LevelToLoad);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnSubmit(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
      //  OnSelect(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //OnDeselect(eventData);
    }
    #endregion

    #region Tweens

    void CreateTween(float target, float duration)
    {
        if (CurrentTweening != null)
        {
            CurrentTweening.StopTweening(Tween.eExitMode.IncompleteTweening, true);
        }

        CurrentTweening = (TweenFloat)TweenManager.CreateTween(VideoAlpha, target, duration, eTweenFunc.LinearToTarget, OnTweenEnd);
    }

    void OnTweenEnd()
    {
        VideoGroup.alpha = VideoAlpha.Value;

        CurrentTweening = null;
    }

    #endregion
}
