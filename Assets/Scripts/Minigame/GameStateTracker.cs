using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameStateTracker : BaseObject
{

    public enum eMinigameState
    {
        Ready,
        Playing
    }

    protected override void Awake()
    {
        base.Awake();
        m_GameState = eMinigameState.Ready;

        m_Controller = GameObject.Find("Controller").GetComponent<BreakoutController>();
        m_Ball = transform.Find("Ball").gameObject;

        //Get all block references
        TotalBlocksInLevel = transform.GetComponentsInChildren<Block>().Length;
        if (TotalBlocksInLevel == 0)
        {
            Debug.LogError("Empty level.");
        }

        Listen("ToggleMinigameEasterEgg", OnToggleMinigame);
    }


    //Handle basic controls
    private void FixedUpdate()
    {
        if (m_GameState == eMinigameState.Playing)
        {
            
        }
        //If we're waiting for paddle movement then dont update anything else.
        else
        {
            if (m_Controller.GetMoveInput() != Vector3.zero)
            {
                EventManager.TriggerEvent("ToggleMinigameEasterEgg", new MinigameParam(eMinigameState.Playing));
            }
        }
    }

    private void OnToggleMinigame(EventParam param)
    {
        MinigameParam eventParam = (MinigameParam)param;
        m_GameState = eventParam.GameState;
    }

    public eMinigameState GetGameState()
    {
        return m_GameState;
    }

    public bool CanUpdate()
    {
        return m_GameState == eMinigameState.Playing;
    }

    public static GameStateTracker Instance()
    {
        if (m_Instance == null)
        {
            GameObject obj = GameObject.Find("Breakout");

            if (obj)
            {
                return m_Instance = obj.GetComponent<GameStateTracker>();
            }
            
        }

        return m_Instance;
    }

    public void DecrementCounter()
    {
        TotalBlocksInLevel--;
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (TotalBlocksInLevel <= 0)
        {
            EventManager.TriggerEvent("ToggleMinigameEasterEgg", new MinigameParam(eMinigameState.Ready));
            EventManager.TriggerEvent("OnSoftReset");
        }
    }

    protected override void OnSoftReset()
    {
        base.OnSoftReset();
        m_GameState = eMinigameState.Ready;
    }

    private void OnEnable()
    {
        m_Ball.SetActive(true);
        
        if (CanUpdate())
        {
            Ball comp = m_Ball.GetComponent<Ball>();
            comp.AssignVelocity(comp.LastKnownVelocity);
        }
    }


    private static GameStateTracker m_Instance = null;
    private BreakoutController m_Controller;

    public eMinigameState m_GameState;

    private GameObject m_Ball;
    public int TotalBlocksInLevel;
}
