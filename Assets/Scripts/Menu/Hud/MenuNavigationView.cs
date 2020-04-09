using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuNavigationView : MonoBehaviour
{
    List<TMPro.TMP_Text> Navigations = null;

    public TMPro.TMP_SpriteAsset GamePadAsset = null;
    public TMPro.TMP_SpriteAsset KBMAsset = null;

    protected eLastInputType LastInput = eLastInputType.None;

    //bool test = true;

    private void Awake()
    {
        Navigations = new List<TMPro.TMP_Text>();

        TMPro.TMP_Text[] temp = GetComponentsInChildren<TMPro.TMP_Text>();

        foreach(var txt in temp)
        {
            Navigations.Add(txt);
        }

        //for (int i = 0; i < transform.childCount; ++i)
        //{
        //    TMPro.TMP_Text temp = transform.GetChild(i).gameObject.GetComponent<TMPro.TMP_Text>();
        //    TMPro.TMP_Text temp = transform.GetChild(i).gameObject.GetComponent<TMPro.TMP_Text>();
        //    if (temp)
        //    {
        //        Navigations.Add(temp);
        //    }
        //}
    }

    private void Update()
    {
        eLastInputType tempInput = CheckLastInputType.GetLastInputType();

        //if the input type changed swap
        if (LastInput != tempInput)
        {

            switch (tempInput)
            {
                case eLastInputType.KBM:
                    SwapSpriteAsset(KBMAsset);
                    //SwapSpriteAsset(GamePadAsset);
                    break;
                case eLastInputType.Gamepad:
                    SwapSpriteAsset(GamePadAsset);
                    break;
                default:
                    break;
            }
        }

        LastInput = tempInput;

        //if (Input.GetKeyDown(KeyCode.Comma))
        //{
        //    if (test)
        //    {
        //        SwapSpriteAsset(KBMAsset);
        //        LastInput = eLastInputType.KBM;
        //    }
        //    else
        //    {
        //        SwapSpriteAsset(GamePadAsset);
        //        LastInput = eLastInputType.Gamepad;
        //    }
        //    test = !test;
        //}
    }

    void SwapSpriteAsset(TMPro.TMP_SpriteAsset asset)
    {
        foreach(TMPro.TMP_Text text in Navigations)
        {
            text.spriteAsset = asset;
        }
    }
}
