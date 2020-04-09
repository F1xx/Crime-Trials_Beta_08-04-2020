using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyBindButtonVisuals : BaseObject
{
    [SerializeField]
    TMPro.TMP_Text TextToChange = null;

    protected override void Awake()
    {
        base.Awake();
    }

    public void UpdateVisuals(KeyCode code, KeyBindType type)
    {
        if (code == KeyCode.None || code == KeyCode.Clear)
        {
            TextToChange.text = "--";
            return;
        }


        //if KBM just write the text
        if (type == KeyBindType.primary || type == KeyBindType.alternate)
        {
            TextToChange.text = code.ToString();
        }
        //if its gamepad then we have to use sprites
        else
        {
            string text = "<sprite=";

            switch(code)
            {
                case KeyCode.JoystickButton0:
                    text += TextToChange.spriteAsset.GetSpriteIndexFromName("A").ToString();
                    break;
                case KeyCode.JoystickButton1:
                    text += TextToChange.spriteAsset.GetSpriteIndexFromName("B").ToString();
                    break;
                case KeyCode.JoystickButton2:
                    text += TextToChange.spriteAsset.GetSpriteIndexFromName("X").ToString();
                    break;
                case KeyCode.JoystickButton3:
                    text += TextToChange.spriteAsset.GetSpriteIndexFromName("Y").ToString();
                    break;
                case KeyCode.JoystickButton4:
                    text += TextToChange.spriteAsset.GetSpriteIndexFromName("LB").ToString();
                    break;
                case KeyCode.JoystickButton5:
                    text += TextToChange.spriteAsset.GetSpriteIndexFromName("RB").ToString();
                    break;
                case KeyCode.JoystickButton6://don't have a sprite
                    TextToChange.text = "Options";
                    return;
                case KeyCode.JoystickButton7://don't have a sprite
                    TextToChange.text = "Start";
                    return;
                case KeyCode.JoystickButton8:
                    text += TextToChange.spriteAsset.GetSpriteIndexFromName("L").ToString();
                    break;
                case KeyCode.JoystickButton9:
                    text += TextToChange.spriteAsset.GetSpriteIndexFromName("R").ToString();
                    break;
            }
            TextToChange.text = text + ">";
        }
    }
}
