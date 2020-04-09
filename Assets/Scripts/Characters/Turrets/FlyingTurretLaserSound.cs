using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingTurretLaserSound : BaseObject
{
    [HideInInspector] public AudioChannel m_Channel { get; protected set; }

    public StateMachineBase stateMachine;

    // Start is called before the first frame update
    void Start()
    {
        m_Channel = GetComponent<AudioChannel>();
       // stateMachine = GetComponentInParent<TurretStateMachine>();

        //listen for if a cutscne starts or finsihes 
        Listen("CutsceneStart", DisableDuringCutscene);
        Listen("CutsceneFinsished", EnableAfterCutscene);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (stateMachine.GetActiveState().Is<StateAggressiveFlying>())
        {
            m_Channel.PlayAudio(true);
        }
        else
        {
            StopAudio();
        }
    }

    public void StopAudio()
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
