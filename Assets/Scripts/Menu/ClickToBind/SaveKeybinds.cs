using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveKeybinds : MonoBehaviour
{
    public void Save()
    {
        EventManager.TriggerEvent("OnSaveKeybinds");
    }
}
