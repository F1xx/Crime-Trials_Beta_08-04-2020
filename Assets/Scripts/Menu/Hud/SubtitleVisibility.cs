using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubtitleVisibility : BaseObject
{
    CanvasGroup m_Group = null;
    bool m_IsVisible = true;

    protected override void Awake()
    {
        base.Awake();

        m_Group = GetComponent<CanvasGroup>();

        Listen("OnToggleSubtitles", LoadSettings);

        LoadSettings();
    }

    void LoadSettings()
    {
        m_IsVisible = System.Convert.ToBoolean(PlayerPrefs.GetInt("OnToggleSubtitles", 1));

        m_Group.alpha = m_IsVisible == true ? 1.0f : 0.0f;
    }
}
