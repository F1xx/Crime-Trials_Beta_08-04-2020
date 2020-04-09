using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretStateMachine : StateMachineBase
{

    protected override void Start()
    {
        //base.Start();

        if (ListOfStates.Count == 0)
        {
            Debug.LogWarning(string.Format("State machine is empty inside {0}", gameObject.name));
            return;
        }
        else
        {
            List<StateBase> createdList = new List<StateBase>();

            foreach (var state in ListOfStates)
            {
                StateBase instanceState = ScriptableObject.Instantiate(state);
                createdList.Add(instanceState);
            }

            ListOfStates = createdList;
        }

        bool usingInitialState = false;
        if (InitialState != null)
        {
            for (int i = 0; i < ListOfStates.Count; i++)
            {
                if (ListOfStates[i].GetType() == InitialState.GetType())
                {
                    usingInitialState = true;
                    CurrentState = ListOfStates[i];
                }
            }
        }

        for (int i = 0; i < ListOfStates.Count; i++)
        {
            ListOfStates[i].SM = this;
            ListOfStates[i].AudioChannelObject = m_AudioChannel;
            ListOfStates[i].Start();

            if (usingInitialState && InitialState == ListOfStates[i])
            {
                StateIndex = i;
                CurrentState = ListOfStates[i];
            }
        }

        Listen(gameObject, "OnRespawnEvent", OnRespawnEvent);
        Listen(gameObject, "OnDeathEvent", OnDeathEvent);
    }

    protected override void OnDeathEvent(EventParam param)
    {
        SetMovementStateForced<StateDisabled>();
    }

    protected override void OnRespawnEvent()
    {
        SetMovementStateForced<StateIdleGround>();
    }
}
