using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsHandler : BaseObject
{
    [Header("Crosshair")]
    public Button Crosshair = null;
    [Tooltip("Make sure this matches what is under Crosshairs in the PlayerHUD PlayerCrosshair Script")]
    public Sprite[] Crosshairs = null;

    protected override void Awake()
    {
        base.Awake();


        //OnBrightnessChange();

        Listen("OnSettingsReset", ResetDefaultSettings);
        //Listen("OnBrightnessChange", OnBrightnessChange);

        SetupControls();
        StartListening();
    }

    void SetupControls()
    {
        SetCrosshair("Crosshair", Crosshair);
    }

    void StartListening()
    {
        Crosshair.onClick.AddListener(ChangeCrosshair);
    }

    /// <summary>
    /// If the default exists set them to their default values.
    /// Default values should always be set on first run.
    /// Note this only actually resets the 
    /// </summary>
    public void ResetDefaultSettings()
    {
        //reset the special ones
        //OnBrightnessChange();
        ResetCrosshair();
    }

    public void ChangeCrosshair()
    {
        string key = "Crosshair";
        if (PlayerPrefs.HasKey(key))
        {
            int x = PlayerPrefs.GetInt(key);
            x++;

            if (x >= Crosshairs.Length)
            {
                x = 0;
            }

            OnIntChange(x, key, "OnCrosshairChange");
            SetCrosshair(key, Crosshair);
        }
    }

    void ResetCrosshair()
    {
        OnIntChange(0, "Crosshair", "OnCrosshairChange");
        ChangeCrosshair();
    }

    void OnBrightnessChange()
    {
        if (PlayerPrefs.HasKey("OnBrightnessChange"))
        {
            float amount = PlayerPrefs.GetFloat("OnBrightnessChange");

            amount = Mathf.Clamp(amount, 0.0f, 1.0f);

            RenderSettings.ambientIntensity = amount;

            RenderSettings.ambientLight = new Color(amount, amount, amount, 1.0f);
        }
    }

    void OnIntChange(int val, string key, string eventName = "")
    {
        PlayerPrefs.SetInt(key, val);
        PlayerPrefs.Save();

        if (string.IsNullOrEmpty(eventName) == false)
        {
            EventManager.TriggerEvent(eventName);
        }
    }

    //changes the image of the button to the current crosshair
    void SetCrosshair(string key, Button button)
    {
        if (PlayerPrefs.HasKey(key))
        {
            int x = PlayerPrefs.GetInt(key);

            if(x >= Crosshairs.Length)
            {
                x = 0;
            }
            button.image.sprite = Crosshairs[x]; 
        }
        else
        {
            //sets the crosshair to the first
            OnIntChange(0, key);
        }
    }
}
