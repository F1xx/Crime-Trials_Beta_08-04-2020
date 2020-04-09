using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingSlider : Slider
{
    TMPro.TMP_Text Completiontext = null;

    protected override void Awake()
    {
        base.Awake();

        Completiontext = GetComponentInChildren<TMPro.TMP_Text>();
    }

    protected override void Update()
    {
        base.Update();
        if (Completiontext)
        {
            Completiontext.text = (100 * SceneLoader.Progress).ToString("F1") + "%";
        }
        value = SceneLoader.Progress;
    }
}
