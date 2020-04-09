using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : BaseObject
{


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (gameObject.activeSelf && collision.gameObject.tag == "Ball")
        {
            gameObject.SetActive(false);
            GameStateTracker.Instance().DecrementCounter();
        }
    }

    protected override void OnSoftReset()
    {
        base.OnSoftReset();
        gameObject.SetActive(true);
    }
}
