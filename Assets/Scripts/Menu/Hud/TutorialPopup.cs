using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPopup : BaseObject
{
    [SerializeField, Header("TutorialPopup")]
    TMPro.TMP_Text TitleText = null;
    [SerializeField]
    TMPro.TMP_Text BodyText = null;

    [SerializeField]
    float DurationToShow = 5.0f;
    float FadeOutDuration = 1.0f;
    Timer LifeTimer = null;
    Timer FadeOutTimer = null;

    [HideInInspector]
    public TutorialsHandler Handler = null;
    public bool IsActive { get; private set; }
    public string Body { get { return BodyText.text.ToString(); } }
    public string Title { get { return TitleText.text.ToString(); } }
    public float RemainingDuration { get { return LifeTimer.GetTimeRemaining(); } }
    public float BaseDuration { get { return DurationToShow; } }

    protected override void Awake()
    {
        base.Awake();

        LifeTimer = CreateTimer(DurationToShow, RemovePopup);
        FadeOutTimer = CreateTimer(FadeOutDuration, OnFadeOutFinish);

        Clear();
    }

    private void Update()
    {
        if (FadeOutTimer.IsRunning)
        {
            TitleText.alpha = 1.0f - FadeOutTimer.GetPercentageComplete();
            BodyText.alpha = 1.0f - FadeOutTimer.GetPercentageComplete();
        }
    }

    public void RemovePopup()
    {
        //IsActive = false;
        //LifeTimer.Reset();
        //FadeOutTimer.Restart();

        Clear();
        OnFadeOutFinish();
    }

    public void Clear()
    {
        IsActive = false;
        LifeTimer.Reset();
        FadeOutTimer.Reset();

        TitleText.alpha = 0.0f;
        BodyText.alpha = 0.0f;
        TitleText.text = "";
        BodyText.text = "";
    }

    public void SetAndStart(string title, string body, float duration)
    {
        Set(title, body, duration);
        IsActive = false;
        LifeTimer.Reset();
        StartTutorial();
    }

    public void Set(TutorialPopup from)
    {
        Set(from.Title, from.Body, from.RemainingDuration);
        IsActive = from.IsActive;

        if(IsActive)
        {
            StartTutorial();
        }
    }

    void Set(string title, string body, float duration)
    {
        DurationToShow = duration;
        LifeTimer.SetDuration(DurationToShow);

        TitleText.text = title;
        BodyText.text = body;
    }

    private void StartTutorial()
    {
        TitleText.alpha = 1.0f;
        BodyText.alpha = 1.0f;

        LifeTimer.Restart();
        IsActive = true;
    }

    void OnFadeOutFinish()
    {
        Handler.RemoveTutorial(this);
    }
}
