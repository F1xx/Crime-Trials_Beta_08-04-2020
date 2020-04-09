using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DangerLiquid : Hazard
{
    [SerializeField]
    float AmountToSlow = 8.0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            DebuffSystem sys = other.gameObject.GetComponent<DebuffSystem>();

            if(sys)
            {
                sys.QueueForAddition(new MovementDebuff(float.MaxValue, AmountToSlow, sys, gameObject));
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //if (other.tag == "Player")
        //{
            //
        //}
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            DebuffSystem sys = other.gameObject.GetComponent<DebuffSystem>();

            if (sys)
            {
                sys.RemoveAllDebuffsofTypeFromCauser<MovementDebuff>(gameObject);
            }
        }
    }
}
