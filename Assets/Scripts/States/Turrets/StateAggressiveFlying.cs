using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable, CreateAssetMenu(fileName = "State", menuName = "ScriptableObjects/State/Enemy/FlyingTurret/Aggressive", order = 1)]
public class StateAggressiveFlying : StateBase
{
    float TurretRange;

    // Start is called before the first frame update
    public override void Start()
    {
        TurretRange = SM.gameObject.GetComponent<FlyingTurret>().TurretRange;
    }

    public override void LateUpdate()
    {
        //If we didnt hit the player
        if (!PlayerFinder.FindPlayer(out Collider hit, SM.gameObject, TurretRange))
        {
            SM.SetMovementState<StatePatrollingFlying>();
        }
        else 
        {
            //We hit the player, but if he isnt alive then we dont really care about him so go back to patrolling.
            if (hit.gameObject != null && hit.gameObject.GetComponent<HealthComponent>().IsAlive() == false)
            {
                SM.SetMovementState<StatePatrollingFlying>();
            }
        }
    }

}
