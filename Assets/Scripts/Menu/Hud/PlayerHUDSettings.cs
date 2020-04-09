using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHUDSettings : BaseObject
{
    [Header("HUD Settings")]
    public CanvasGroup TimerPanel = null;
    public CanvasGroup Crosshair = null;

    PlayerCrosshair CrosshairScript = null;

    public bool ShouldShowTime = true;
    public bool ShouldShowCrosshair = true;

    protected override void Awake()
    {
        base.Awake();

        CrosshairScript = GetComponentInChildren<PlayerCrosshair>();

        Listen("OnToggleCrosshair", OnToggleCrosshair);
        Listen("OnToggleHUDTimer", OnToggleHUDTimer);
        Listen("OnCrosshairChange", OnCrosshairChange);

        if(!ShouldShowCrosshair)
        {
            Crosshair.alpha = 0.0f;
        }
        if (!ShouldShowTime)
        {
            TimerPanel.alpha = 0.0f;
        }      
    }

    private void Start()
    {
        LoadSettings();
    }

    private void Update()
    {
       
    }

    void OnToggleCrosshair()
    {
        ShouldShowCrosshair = Convert.ToBoolean(PlayerPrefs.GetInt("OnToggleCrosshair"));

        ToggleCanvasVisibility(Crosshair, ShouldShowCrosshair);
    }

    void OnToggleHUDTimer()
    {
        ShouldShowTime = Convert.ToBoolean(PlayerPrefs.GetInt("OnToggleHUDTimer"));

        ToggleCanvasVisibility(TimerPanel, ShouldShowTime);
    }

    void ToggleCanvasVisibility(CanvasGroup group, bool val)
    {
        if (val == false)
        {
            group.alpha = 0.0f;
        }
        else
        {
            group.alpha = 1.0f;
        }
    }

    void OnCrosshairChange()
    {
        CrosshairScript.ChangeCrosshair(PlayerPrefs.GetInt("Crosshair"));
    }

    void LoadSettings()
    {
        if (PlayerPrefs.HasKey("OnToggleCrosshair"))
        {
            ShouldShowCrosshair = Convert.ToBoolean(PlayerPrefs.GetInt("OnToggleCrosshair"));
            ToggleCanvasVisibility(Crosshair, ShouldShowCrosshair);
        }

        if (PlayerPrefs.HasKey("OnToggleHUDTimer"))
        {
            ShouldShowTime = Convert.ToBoolean(PlayerPrefs.GetInt("OnToggleHUDTimer"));
            ToggleCanvasVisibility(TimerPanel, ShouldShowTime);
        }

        if (PlayerPrefs.HasKey("Crosshair"))
        {
            OnCrosshairChange();
        }
    }
}
