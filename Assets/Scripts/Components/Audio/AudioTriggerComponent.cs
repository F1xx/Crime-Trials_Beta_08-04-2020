using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic trigger component for playing audio that involves collision detection on (usually) invisible objects.
/// </summary>
[RequireComponent(typeof(Collider))]
public class AudioTriggerComponent : MonoBehaviour
{
    public ScriptableAudio AudioOnEnter = null;
    public ScriptableAudio AudioOnExit = null;

    [Tooltip("Does this component use the Commentator audio channel or does it use its own?")]
    public bool IsCommentatorAudio = true;

    [Tooltip("Specify if the enter and exit audio play only once or EVERY time collision happens.")]
    public bool OnlyPlaysOnce = true;

    [Tooltip("If the Audio fails to play due to priority or any other reason when it was triggered, it doesn't count as being played for the purposes " +
        "of OnlyPlayOnce")]
    public bool IgnoreFailedAudioPlayback = false;

    AudioChannel m_SoundChannel = null;
    Collider m_TriggerCollider;

    private int NumTimesPlayedEnter = 0;
    private int NumTimesPlayedEnd = 0;

    // Start is called before the first frame update
    void Start()
    {
        m_TriggerCollider = GetComponent<Collider>();
        m_TriggerCollider.isTrigger = true;

        if (IsCommentatorAudio == true)
        {
            AudioChannel comChannel = Commentator.Instance().GetComponent<AudioChannel>();

            if (comChannel != null)
            {
                m_SoundChannel = comChannel;
            }
            else
            {
                Debug.LogWarning("AudioTriggerComponent is set with IsCommentatorAudio true but a Commentator does not exist.");
            }
        }
        else
        {
            AudioChannel channel = GetComponent<AudioChannel>();
            if (channel == null)
            {
                Debug.LogError("AudioTriggerComponent does not have an AudioChannel to speak to. Attach one or set to IsCommentatorAudio true.");
            }
            else
            {
                m_SoundChannel = channel;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayEnterAudio();
    }

    private void OnTriggerExit(Collider other)
    {
        PlayExitAudio();
    }

    protected virtual void PlayEnterAudio()
    {
        if (m_SoundChannel && AudioOnEnter != null)
        {

            if (OnlyPlaysOnce)
            {
                if (NumTimesPlayedEnter == 0)
                {
                    m_SoundChannel.PlayAudio(AudioOnEnter);
                    NumTimesPlayedEnter++;
                }
            }
            else
            {
                m_SoundChannel.PlayAudio(AudioOnEnter);
                NumTimesPlayedEnter++;
            }

        }
    }

    protected virtual void PlayExitAudio()
    {
        if (m_SoundChannel && AudioOnExit != null)
        {

            if (OnlyPlaysOnce)
            {
                if (NumTimesPlayedEnd == 0)
                {
                    m_SoundChannel.PlayAudio(AudioOnExit);
                    NumTimesPlayedEnd++;
                }
            }
            else
            {
                m_SoundChannel.PlayAudio(AudioOnExit);
                NumTimesPlayedEnd++;
            }

        }
    }
}
