using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTurretRotateSound : BaseObject
{
    CalculateAngularVelocity angularVeloTester;

    [HideInInspector] public AudioChannel m_Channel { get; protected set; }

    StateMachineBase stateMachine;

    // Start is called before the first frame update
    void Start()
    {
        angularVeloTester = GetComponent<CalculateAngularVelocity>();
        m_Channel = GetComponent<AudioChannel>();
        stateMachine = GetComponentInParent<TurretStateMachine>();

        //listen for if a cutscne starts or finsihes 
        Listen("CutsceneStart", DisableDuringCutscene);
        Listen("CutsceneFinsished", EnableAfterCutscene);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //angularVeloTester.FixedUpdate();
        if (stateMachine.GetActiveState().Is<StateDisabled>() == false)
        {
            //if it isnt close enough to zero that means it is moving and should play sound
            if (!MathUtils.FloatCloseEnough(angularVeloTester.AngularVelocity(), 0.0f, 0.1f))
            {
                m_Channel.PlayAudio(true);
            }
            else
            {
                StopRotateAudio();
            }
        }
    }   

    public void StopRotateAudio()
    {
        m_Channel.StopAudio();
    }

    void DisableDuringCutscene()
    {
        //disables sound duting cutscenes
        this.gameObject.SetActive(false);
    }

    void EnableAfterCutscene()
    {
        //enables sound when cutscene is skipped or finishes
        this.gameObject.SetActive(true);
    }
}
