using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlSchemeImageChange : BaseObject
{
    const string PREF = "ControlSchemeImageChange";

    //[SerializeField]
    //UnityEngine.UI.Image KeyboardImage = null;
    //[SerializeField]
    //UnityEngine.UI.Image GamepadImage = null;

    [SerializeField]
    GameObject KeyboardImage = null;
    [SerializeField]
    GameObject GamepadImage = null;

    protected override void Awake()
    {
        base.Awake();

        Listen(PREF, LoadSettings);
        LoadSettings();
    }

    void LoadSettings()
    {
        int selection = PlayerPrefs.GetInt(PREF, 0);

        if(selection == 0)
        {
            KeyboardImage.gameObject.SetActive(true);
            GamepadImage.gameObject.SetActive(false);
        }
        else
        {
            KeyboardImage.gameObject.SetActive(false);
            GamepadImage.gameObject.SetActive(true);
        }
    }
}
