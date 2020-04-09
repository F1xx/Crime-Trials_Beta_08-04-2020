using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyButtonGraphicsSettings : MonoBehaviour
{
    public void Apply()
    {
        GraphicsManager.ApplyResolution();
    }
}
