using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Electric : PainVolume
{
    [Tooltip("How much force to use against the player")]
    public float GroundPushForce = 20.0f;
    public float AirPushForce = 8.0f;
    public eTweenFunc ForceMode = eTweenFunc.LinearToTarget;

    [Tooltip("How long should the impulse be applied to the player. Increasing this will decrease the intensity of the impulse but prologue it.")]
    public float DurationOfImpulse = 0.5f;
    public float DownForce = 2.0f;

    Timer CooldownTimer = null;

    protected override void Awake()
    {
        base.Awake();

        CooldownTimer = CreateTimer(0.5f);
    }

    protected override void Start()
    {
        m_SoundChannel = GetComponentInChildren<AudioChannel>();
    }

    public void GetPushed(PlayerController controller, ControllerColliderHit hit)
    {
        if(CooldownTimer.IsRunning == false)
        {
            //Vector3 direction = hit.transform.position - transform.position;
            Vector3 direction = hit.normal;
            direction.Normalize();

            if (controller.CharCont.isGrounded)
            {
                controller.AddImpulse(direction * GroundPushForce, ForceMode, DurationOfImpulse);
            }
            else
            {
                //direction.y = -DownForce;
                controller.AddImpulse(direction * AirPushForce, ForceMode, DurationOfImpulse);
            }

            CooldownTimer.Restart();
        }
    }
}
