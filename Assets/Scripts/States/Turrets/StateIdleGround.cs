using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable, CreateAssetMenu(fileName = "State", menuName = "ScriptableObjects/State/Enemy/GroundTurret/Idle", order = 1)]
public class StateIdleGround : StateBase
{
    float TurretRange;

    // Start is called before the first frame update
    public override void Start()
    {
        GroundTurret GroundTurretObj = SM.gameObject.GetComponent<GroundTurret>();
        TurretRange = GroundTurretObj.TurretRange;
    }

    public void Awake()
    {

    }

    public override void LateUpdate()
    {

        //search for the player in direction turret is facing
        if(PlayerFinder.FindPlayer(out Collider hit, SM.gameObject, TurretRange))
        {
            SwitchStateToAggressive();
        }

    }

    public void SwitchStateToAggressive()
    {
        SM.SetMovementState<StateAggressiveGround>();
    }
}
