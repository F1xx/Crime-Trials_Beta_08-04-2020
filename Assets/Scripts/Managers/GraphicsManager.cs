using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class GraphicsManager : Singleton<GraphicsManager>
{
    public const string RESOLUTION_PREF_KEY = "OnResolutionSettingsChange";
    public const string FULLSCREEN_PREF_KEY = "OnFullscreenChange";

    [SerializeField]
    //Text resolutionText = null;

    Resolution[] m_Resolutions;

    int currentResolutionIndex = 0;
    bool m_IsFullScreen = true;

    protected override void OnAwake()
    {
        DontDestroyOnLoad(this);

        m_Resolutions = Screen.resolutions;

        Listen(RESOLUTION_PREF_KEY, ReceiveResolutionChange);
        Listen(FULLSCREEN_PREF_KEY, ReceiveFullscreenChange);

        currentResolutionIndex = PlayerPrefs.GetInt(RESOLUTION_PREF_KEY, 0);
        m_IsFullScreen =  System.Convert.ToBoolean(PlayerPrefs.GetInt(RESOLUTION_PREF_KEY, 1));
    }

    /// <summary>
    /// Applies resolution based on currently set variables (set based on events)
    /// </summary>
    public static void ApplyResolution()
    {
        GraphicsManager instance = Instance();
        Resolution newRes = instance.m_Resolutions[instance.currentResolutionIndex];

        if (instance.IsNewResolutionDifferent(newRes))
        {
            Screen.SetResolution(newRes.width, newRes.height, instance.m_IsFullScreen);
        }
    }

    /// <summary>
    /// If the new resolution is different in height, width or fullscreen has changed then it will return true
    /// </summary>
    /// <param name="newRes">the resolution to potentially change to</param>
    /// <returns>true if resolution is different</returns>
    bool IsNewResolutionDifferent(Resolution newRes)
    {
        return newRes.width != Screen.currentResolution.width ||
               newRes.height != Screen.currentResolution.height ||
               Instance().m_IsFullScreen != Screen.fullScreen; 
    }

    void ReceiveResolutionChange(EventParam evParam)
    {
        OnNavigatorChangedParam param = (OnNavigatorChangedParam)evParam;

        currentResolutionIndex = param.OptionIndex;
    }

    /// <summary>
    /// called by event. This will just set the variable.
    /// </summary>
    void ReceiveFullscreenChange()
    {
        m_IsFullScreen = System.Convert.ToBoolean(PlayerPrefs.GetInt(FULLSCREEN_PREF_KEY, 1));
    }
}
