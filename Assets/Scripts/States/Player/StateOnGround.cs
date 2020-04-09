using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StateOnGround logic switch rules:
///  - If not alive                     : StateDisabled
///  - If in air                        : StateInAir
///  - If crouch button pressed         : StateCrouching (handled in CrouchComponent.cs)
/// </summary>
/// 
[System.Serializable, CreateAssetMenu(fileName = "State", menuName = "ScriptableObjects/State/Player/OnGround", order = 2)]
public class StateOnGround : StateBase
{
    [HideInInspector]
    public GameObject BaseLoc;
    CharacterController characterController;

    //private float CheckForGroundRadius = 0.5f;
    //private float MinAllowedSurfaceAngle = 15.0f;

    public ScriptableAudio OnLandingAudio = null;

    public override void Start()
    {
        BaseLoc = SM.GetComponent<Character>().BaseLocationObj;
        characterController = SM.GetComponent<CharacterController>();
        EventManager.StartListening(SM.gameObject, "OnStateChangeEvent", OnStateChange);
    }

    public override void LateUpdate()
    {
        if (IsNotOnGround())
        {
            return;
        }
    }

    private void OnStateChange(EventParam param)
    {
        if (SM.CurrentState == this)
        {
            OnStateChangeParams stateParam = (OnStateChangeParams)param;

            if (stateParam.Previous.Is<StateInAir>())
            {
                if (AudioChannelObject && OnLandingAudio)
                {
                    AudioChannelObject.PlayAudio(OnLandingAudio);
                }
            }
        }
    }

    /// <summary>
    /// Switch to InAir if ground is not found. Return true if state change, false otherwise.
    /// </summary>
    /// <returns></returns>
    private bool IsNotOnGround()
    {
        //RaycastHit groundHitInfo;
        //Vector3 raystart = SM.gameObject.transform.position;
        //Vector3 basePos = BaseLoc.transform.position;
        ////check to see if player died first
        //
        ////if after everything we didn't find ground we must be in the air (falling most likely, also after a jump)
        //if (MathUtils.CheckForGroundBelow(out groundHitInfo, raystart, basePos, CheckForGroundRadius, MinAllowedSurfaceAngle, SM.GroundCheckMask))
        //{
        //    if (groundHitInfo.transform.tag == "Projectile")
        //    {
        //        SM.SetMovementState<StateInAir>();
        //        return true;
        //    }
        //
        //    return false;
        //}
        //
        //SM.SetMovementState<StateInAir>();
        //return true;
        if(characterController.isGrounded == false)
        {
            SM.SetMovementState<StateInAir>();
            //Debug.Log("swap to air");
            return true;
        }
        return false;

    }


}
