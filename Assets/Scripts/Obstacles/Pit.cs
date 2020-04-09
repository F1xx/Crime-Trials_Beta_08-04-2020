using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pit : Hazard
{
    // Start is called before the first frame update
    void Start()
    {
        Renderer rend = gameObject.GetComponent<Renderer>();

        if(rend)
        {
            rend.enabled = false;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
