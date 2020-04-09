using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Commentator : Singleton<Commentator>
{
    public ScriptableAudio IdleComments = null;
    public ScriptableAudio RespawnComments = null;
    public ScriptableAudio PopupComment = null;

    [Header("Checkpoint Comments")]
    public ScriptableAudio GenericCheckpointComments = null;
    public ScriptableAudio FastCheckpointComments = null;
    public ScriptableAudio SlowCheckpointComments = null;
    public int ChanceForSpeedCheckpointComment = 20;

    AudioChannel CommentatorChannel = null;

    private GameObject m_Player = null;

    [Header("Death Comments")]
    public ScriptableAudio DeathComments = null;
    public ScriptableAudio TurretDeathComments = null;
    public ScriptableAudio BeeDeathComments = null;

    //random control variables for commentator flow
    bool HasPassedCheckpoint = false;
    bool IsFirstPopup = true;

    [Header("End Of Level Comments")]
    public ScriptableAudio NoDeathsList = null;
    public ScriptableAudio LowDeathsComments = null;
    public ScriptableAudio ManyDeathsComments = null;
    [Tooltip("How many deaths are required to be considered in Many Deaths.")]
    public int ManyDeathsRequirement = 20;

    protected override void OnAwake()
    {
        m_Player = GameObject.Find("Player");

        CommentatorChannel = GetComponent<AudioChannel>();

        Debug.Assert(m_Player);
    }

    private void Start()
    {
        Listen("OnPlayerDeathEvent", OnPlayerDeathEvent);
        Listen("OnPlayerRespawnEvent", OnPlayerRespawnEvent);
        Listen("OnCheckPointReached", OnPlayerCheckpointEvent);
        //Added by Liam
        Listen("OnLevelComplete", OnLevelCompleteEvent);

        Listen("OnPopupTriggeredEvent", OnPopupTriggerEvent);
        Listen("OnPlayerIdleEvent", OnPlayerIdleEvent);
        Listen("OnTargetDestroyed", OnBeeDeathEvent);
    }

    void OnPopupTriggerEvent()
    {
        if (IsFirstPopup)
        {
            CommentatorChannel.PlayAudio(PopupComment);
            IsFirstPopup = false;
        }
    }

    void OnBeeDeathEvent()
    {
        CommentatorChannel.PlayAudio(BeeDeathComments);
    }

    void OnPlayerIdleEvent()
    {
        CommentatorChannel.PlayAudio(IdleComments);
    }

    public void OnLevelCompleteEvent(EventParam param)
    {
        int deathCounter = m_Player.GetComponent<PlayerHealthComponent>().GetDeathCounter();

        if (deathCounter == 0)
        {
            CommentatorChannel.PlayAudio(NoDeathsList);
            return;
        }

        if (deathCounter >= ManyDeathsRequirement)
        {
            CommentatorChannel.PlayAudio(ManyDeathsComments);
            return;
        }

        CommentatorChannel.PlayAudio(LowDeathsComments);
    } 

    public void OnPlayerDeathEvent(EventParam param)
    {
        OnDeathEventParam Dparam = (OnDeathEventParam)param;

        //example of if we care about ground turrets
        if (Dparam.Attacker.name.ToUpper().Contains("TURRET_GROUND"))
        {
            CommentatorChannel.PlayAudio(TurretDeathComments);
            return;
        }

        CommentatorChannel.PlayAudio(DeathComments);
    }

    public void OnPlayerRespawnEvent()
    {
        CommentatorChannel.PlayAudio(RespawnComments);
    }

    public void OnPlayerCheckpointEvent(EventParam param)
    {

        //random chance whether or not we'll reply if the player is fast or slow.
        int response = Random.Range(1, 100);

        //if we care
        if (response >= 100 - ChanceForSpeedCheckpointComment)
        {
            OnCheckpointReachedParam Param = (OnCheckpointReachedParam)param;
            float heatTime = SQLManager.GetRunCheckpointDataFor(Param.CheckpointIndex); //WorldTimeManager.TimePassed - 1.0f;

            if (heatTime < 0.0f)
            {
                return;
            }

            float difference = Param.TimeReached - heatTime;

            if (difference < 0.0f)
            {
                CommentatorChannel.PlayAudio(FastCheckpointComments);
            }
            else
            {
                CommentatorChannel.PlayAudio(SlowCheckpointComments);
            }
        }
        //we dont care
        else
        {
            CommentatorChannel.PlayAudio(GenericCheckpointComments);
        }
    }

    private void PlayRandom(List<AudioClip> list)
    {
        if(CommentatorChannel.IsPlaying())
        {
            return;
        }
        AudioManager.PlayRandomSoundFromList(list, CommentatorChannel);
    }
}
