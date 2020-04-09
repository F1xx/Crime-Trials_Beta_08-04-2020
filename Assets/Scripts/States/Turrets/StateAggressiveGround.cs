using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable, CreateAssetMenu(fileName = "State", menuName = "ScriptableObjects/State/Enemy/GroundTurret/Aggressive", order = 2)]
public class StateAggressiveGround : StateBase
{
    float TurretRange;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        TurretRange = SM.gameObject.GetComponent<GroundTurret>().TurretRange;
    }

    public override void LateUpdate()
    {
        //hit is player
        if (!PlayerFinder.FindPlayer(out Collider hit, SM.gameObject, TurretRange))
        {
            SM.SetMovementState<StateIdleGround>();
        }
        if (hit != null && hit.gameObject != null)
        {
            if (hit.gameObject.GetComponent<HealthComponent>().IsAlive() == false)
            {
                SM.SetMovementState<StateIdleGround>();
                return;
            }
        }
    }

}
