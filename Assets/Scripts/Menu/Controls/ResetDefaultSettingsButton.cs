using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetDefaultSettingsButton : BaseObject
{
    UnityEngine.UI.Button ResetButton = null;

    protected override void Awake()
    {
        base.Awake();

        ResetButton = GetComponent<UnityEngine.UI.Button>();
        ResetButton.onClick.AddListener(ResetSettings);
    }

    void ResetSettings()
    {
        EventManager.TriggerEvent("OnSettingsReset");
    }
}
