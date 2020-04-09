using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatureSwap : BaseObject
{
    [SerializeField]
    Material MatureVersion = null;
    [SerializeField]
    Material VanillaVersion = null;

    Renderer Rend = null;

    [SerializeField]
    string PrefKey = "OnMatureSettingChange";
    bool m_IsMature = false;

    protected override void Awake()
    {
        base.Awake();

        Rend = GetComponent<Renderer>();

        Listen(PrefKey, OnMatureSettingChange);
        LoadSettings();
        Toggle();
    }

    void LoadSettings()
    {
        m_IsMature = System.Convert.ToBoolean(PlayerPrefs.GetInt(PrefKey, 0));
    }

    void OnMatureSettingChange()
    {
        bool oldsetting = m_IsMature;

        LoadSettings();

        if(oldsetting != m_IsMature)
        {
            Toggle();
        }
    }

    void Toggle()
    {
        if(m_IsMature)
        {
            Rend.material = MatureVersion;
        }
        else
        {
            Rend.material = VanillaVersion;
        }
    }
}
