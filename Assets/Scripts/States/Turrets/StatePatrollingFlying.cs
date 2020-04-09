using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable, CreateAssetMenu(fileName = "State", menuName = "ScriptableObjects/State/Enemy/FlyingTurret/Patrolling", order = 2)]
public class StatePatrollingFlying : StateBase
{
    float TurretRange;

    // Start is called before the first frame update
    public override void Start()
    {
        FlyingTurret Turret = SM.gameObject.GetComponent<FlyingTurret>();
        TurretRange = Turret.TurretRange;
    }

    public override void LateUpdate()
    {
        //search for the player. Switch states if the player is alive.
        if (PlayerFinder.FindPlayer(out Collider hit, SM.gameObject, TurretRange) && hit.gameObject.GetComponent<HealthComponent>().IsAlive())
        {
            SwitchStateToShooting();
        }
    }
    
    public void SwitchStateToShooting()
    {
        SM.SetMovementState<StateAggressiveFlying>();
    }

}
