using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Crouching rules:
/*
 *  - Must be in a ground State to transition into.
 *  - Must either have Crouch Toggle active or Crouch held down if toggling is disabled
 * 
 *  - Crouching reduces movement speed and switches the player model to 'Crouch'
 *  - Crouch is disabled by releasing the toggle or the key, or if the player becomes airborne (from falling)
 * 
 */

[System.Serializable, CreateAssetMenu(fileName = "State", menuName = "ScriptableObjects/State/Player/Crouching", order = 4)]
public class StateCrouching : StateBase
{
    [HideInInspector]
    public GameObject BaseLoc;

    //private float CheckForGroundRadius = 0.5f;
    //private float MinAllowedSurfaceAngle = 15.0f;

    private Animator PlayerAnimator;
    private int CrouchIndex;
    private int StandIndex;

    CharacterController characterController;

    public override void Start()
    {
        BaseLoc = SM.GetComponent<Character>().BaseLocationObj;
        PlayerAnimator = SM.transform.GetComponentInChildren<Animator>();
        characterController = SM.GetComponent<CharacterController>();


        if (PlayerAnimator)
        {
            StandIndex = PlayerAnimator.GetLayerIndex("Base Layer");
            CrouchIndex = PlayerAnimator.GetLayerIndex("Crouch Layer");
        }
    }

    public override void ActivateState()
    {
        base.ActivateState();

    }

    public override void DeactivateState()
    {
        base.DeactivateState();
    }

    public override void LateUpdate()
    {
        //Check if we're in the air so we can swap from Crouch to InAir
        //RaycastHit groundHitInfo;
        //Vector3 raystart = SM.gameObject.transform.position;
        //Vector3 basePos = BaseLoc.transform.position;

        //if after everything we didn't find ground we must be in the air (falling most likely, also after a jump)
        //if (!MathUtils.CheckForGroundBelow(out groundHitInfo, raystart, basePos, CheckForGroundRadius, MinAllowedSurfaceAngle, SM.GroundCheckMask))
        //{
        //    SM.SetMovementState<StateInAir>();
        //    return;
        //}
        if (characterController.isGrounded == false)
        {
            SM.SetMovementState<StateInAir>();
            //Debug.Log("swap to air");
            return;
        }

    }
}
