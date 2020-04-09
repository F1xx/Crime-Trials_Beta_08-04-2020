using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetKeybinds : MonoBehaviour
{
    public void CallResetKeybinds()
    {
        EventManager.TriggerEvent("OnResetDefaultKeybinds");
    }
}
