using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTimerStartTrigger : BaseObject
{

    bool HasLeft = false;
    bool HasReachedCheckpoint = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    protected override void Awake()
    {
        base.Awake();

        //make sure timer doesnt run when scene is first loaded
        WorldTimeManager.StopTimer();
        WorldTimeManager.Reset();

        //listen for checkpoint reached event
        Listen("OnCheckPointReached", OnCheckpointReached);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerExit(Collider other)
    {
        if(!HasLeft)
        {
            if (other.CompareTag("Player"))
            {
                WorldTimeManager.StartTimer();
                HasLeft = true;
            }
        }
    }
    
    void OnCheckpointReached(EventParam param)
    {
        HasReachedCheckpoint = true;
    }

    protected override void OnSoftReset()
    {
        base.OnSoftReset();
        if(!HasReachedCheckpoint)
        {
            HasLeft = false;
            if(WorldTimeManager.IsRunning)
            {
                WorldTimeManager.Reset();
            }
        }
    }

    protected override void OnHardReset()
    {
        base.OnHardReset();
        HasLeft = false;
        HasReachedCheckpoint = false;
    }
}
