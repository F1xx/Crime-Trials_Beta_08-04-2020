using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unlock_Skin : UnlockBase
{
    protected override void Awake()
    {
        base.Awake();

        UnlockKey = "Arm";
        UnlockValue = 3;
        UnlockDisplayText = "Skin Arm!";

        Listen("OnMatureSettingChange", OnMatureSettingChangeEvent);

    }

    private void OnMatureSettingChangeEvent()
    {
        if (PlayerPrefs.HasKey("OnMatureSettingChange"))
        {
            bool val = System.Convert.ToBoolean(PlayerPrefs.GetInt("OnMatureSettingChange", 0));

            if (val == true)
            {
                ActivateUnlock();
            }
        }
    }
}
