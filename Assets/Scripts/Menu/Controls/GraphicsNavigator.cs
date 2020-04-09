using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// a subversion of the other navigator. All this does is specifically decide in itself what the options are.
/// </summary>
public class GraphicsNavigator : SideWaysNavigator
{
    protected override void Awake()
    {
        Resolution[] resolutions = Screen.resolutions;

        Options = new string[resolutions.Length];

        for(int i = 0; i < resolutions.Length; i++)
        {
            Options[i] = resolutions[i].ToString();
        }

        //do base awake AFTER setting our options
        base.Awake();
    }
}
