using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TipTextGenerator : MonoBehaviour
{
    TextMeshProUGUI TipText;
    List<string> Tips = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        TipText = GetComponent<TextMeshProUGUI>();

        //to add tips duplicate the line below and switch out the test in the string and thats it
        Tips.Add("Why are  you looking here what are you BABY? Huh little baby needs help with playing video games?");
        Tips.Add("Hey, you can do more than just move. Wanna know how? Check the controls.");
        Tips.Add("Press 'N' to inspect your super cool arm");
        Tips.Add("Did you see the unlock you may have gotten? Guess what! YOU CAN EQUIP IT! Just look through the menus like a normal person.");
        Tips.Add("Did you find our Crime Trials patented FleshRoom?");
        Tips.Add("Get a credit card when you turn 18. Every once in a while, use it for little things like a chocolate bar, then pay it off right away. this helps you build this really important thing called credit which will let you buy a house or car later when you need to. ");

        int Tip = Random.Range(0, Tips.Count);

        TipText.text = Tips[Tip];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
