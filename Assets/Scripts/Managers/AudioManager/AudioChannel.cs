using UnityEngine;

/// <summary>
/// AudioChannel is a wrapper class for an AudioSource that handles basic feeding of the AudioWrapper into a source.
/// This will eventually handle feeding queues, lists, or other various AudioWrapper objects.
/// </summary>
[System.Serializable]
[RequireComponent(typeof(AudioSource))]
public class AudioChannel : BaseObject
{
    [SerializeField, Header("Channel Defaults"), Tooltip("This is which AudioMixer group the AudioChannel will belong to on startup.")]
    private AudioManager.eChannelType ChannelType = AudioManager.eChannelType.SoundEffects;

    [SerializeField, Tooltip("By default, if an AudioWrapper is passed to the channel and it has the same priority level as the AudioWrapper already playing, the new one will be played " +
        "instead. If you don't want this, enable the field.")]
    private bool SamePriorityLevelInterruptsAudioFlow = false;

    public AudioSource AudioSourceObject { get; private set; }
    public AudioWrapper CurrentAudioWrapperPlayed { get; private set; }

    private bool CurrentlyPaused = false;

    //All variables involving Audio Playback are located here.
    #region Playback

    [Header("Playback")]
    [SerializeField, Tooltip("If enabled, this channel will play sounds from the supplied SctipableAudio automatically whenever " +
        "a sound from the SctipableAudio finishes playing. It's literally a playlist.")]
    private bool AutomaticChannelPlayback = false;

    [SerializeField, Tooltip("If enabled, the AudioChannel will play from the supplied ScriptableAudio when Start is called.")]
    private bool PlayOnStart = false;

    [SerializeField, Tooltip("Internal Audio Object managed by the channel. If this is set the channel can play audio by itself " +
        "either through PlayOnStart or AutomaticChannelPlayback (or explicitly commanded through another script).")]
    public ScriptableAudio OwnedAudioObject = null;

    [SerializeField, Tooltip("By default: AudioChannels will pause their audio when the game is paused. Set this true to override that.")]
    public bool IgnoreIngamePauses = false;

    #endregion

    //All event handling variables are here.
    #region Events

    [Header("Events")]
    [Tooltip("If enabled, this channel will send events when it starts playing audio.")]
    public bool SendEventsWhenAudioPlayed = false;

    [Tooltip("If enabled, this channel will send events when it stops playing audio (either due to requesting stop in code or" +
        " the audio clip ended). This will be automatically set to true if AutomaticChannelPlayback is true.")]
    public bool SendEventsWhenAudioStopped = false;

    [Tooltip("If enabled, this channel will send events whenever it pauses audio.")]
    public bool SendEventsWhenAudioPaused = false;

    [SerializeField, Tooltip("This is used for differentiating between channels if an object contains more than one for searching " +
        "purposes.")]
    private string ChannelName = "Placeholder";

    #endregion

    /// <summary>
    /// Register the Channel to the AudioManager and set up event listening if AutomaticChannelPlayback is set.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        AudioSourceObject = gameObject.GetComponent<AudioSource>();
        AudioSourceObject.transform.SetParent(gameObject.transform, false);

        AudioManager.RegisterChannel(this);

        if (AutomaticChannelPlayback == true)
        {
            SendEventsWhenAudioStopped = true;
            Listen(gameObject, "OnAudioChannelStopped:" + GetEventSignature(), AutoplayAudio);
        }

        Listen("OnGamePaused", OnMenuPause);
        Listen("OnGameUnpaused", OnMenuUnpause);
    }

    /// <summary>
    /// Handles auto play.
    /// </summary>
    private void Start()
    {
        if (PlayOnStart)
        {
            if (OwnedAudioObject != null)
            { 
                PlayAudio(OwnedAudioObject.GetPlayableAudio());
            }
        }
    }

    /// <summary>
    /// Updates volume each frame as master volume can be adjusted.
    /// </summary>
    public void Update()
    {
        //If the source is no longer playing but the CurrentAudioWrapperPlayed isn't empty this means the sound finished playing naturally.
        //So we'll flush data and send an event.
        if (IsPlaying() == false && CurrentAudioWrapperPlayed != null && !CurrentlyPaused)
        {
            StopAudioSource();
        }
    }

    public void MuteAudio(bool Muting)
    {
        AudioSourceObject.mute = Muting;
    }

    //All functions that involve stepping to a certain point in audio exist here.
    #region TimeStep Functions

    /// <summary>
    /// If a clip is playing or paused you can set exactly what percent of the clip you wish to play back at.
    /// </summary>
    /// <param name="Percent"></param>
    /// <returns></returns>
    public bool SetPlayTimeByPercentage(float Percent)
    {
        if (Percent < 0.0f || Percent > 1.0f)
        {
            Debug.LogWarning(string.Format("SetPlayTimeByPercentage received invalid Percentage value. " +
                "AudioChannel:{0}, Percent value:{1}", gameObject.name + ":" + ChannelName, Percent));
        }

        if (AudioSourceObject.clip != null)
        {
            int totalSamples = AudioSourceObject.clip.samples;

            int sampleToSeekTo = (int)(totalSamples * Percent);

            AudioSourceObject.timeSamples = sampleToSeekTo;

            return true;
        }

        return false;
    }

    /// <summary>
    /// If a clip is playing or paused you can set exactly what time you wish to play back at.
    /// </summary>
    /// <param name="TimeElapsed"></param>
    /// <returns></returns>
    public bool SetPlayTimeByTime(float TimeElapsed)
    {
        if (AudioSourceObject.clip != null)
        {
            float totalTime = AudioSourceObject.clip.length;

            if (totalTime > TimeElapsed && TimeElapsed >= 0.0f)
            {
                AudioSourceObject.time = TimeElapsed;
                return true;
            }
            else
            {
                Debug.LogWarning(string.Format("SetPlayTimeByTime received invalid TimeElapsed value. " +
                 "AudioChannel:{0}, Passed Time value:{1}, Clip Length:{2}", gameObject.name + ":" + ChannelName, TimeElapsed, totalTime));
            }

        }

        return false;
    }

    /// <summary>
    /// If a clip is playing or paused you can set exactly what sample you wish to play back at.
    /// </summary>
    /// <param name="SampleToSeek"></param>
    /// <returns></returns>
    public bool SetPlayTimeBySampleCount(int SampleToSeek)
    {
        if (AudioSourceObject.clip != null)
        {
            int totalSamples = AudioSourceObject.clip.samples;

            if (totalSamples >= SampleToSeek)
            {
                AudioSourceObject.timeSamples = SampleToSeek;
            }
            else
            {
                Debug.LogWarning(string.Format("SetPlayTimeBySampleCount received invalid sample seek value." +
                    " AudioChannel:{0}, Sample number:{1}, Max samples in clip: {2}", gameObject.name + ":" + ChannelName, SampleToSeek, totalSamples));
                return false;
            }

            return true;
        }

        return false;
    }

    #endregion

    public AudioManager.eChannelType GetChannelType() { return ChannelType; }

    public void SetChannelType(AudioManager.eChannelType TypeToSet)
    {
        ChannelType = TypeToSet;
        AudioManager.ChangeAudioChannelMixerGroup(this);
    }

    //All functions involving playing audio to the audio source are here
    #region Play Functions

    /// <summary>
    /// Plays an audio from the audio list the channel has known internally.
    /// </summary>
    public bool PlayAudio(bool SuppressWarnings = false)
    {
        if (OwnedAudioObject == null)
        {
            if (!SuppressWarnings)
                Debug.LogWarning("AudioChannel called PlayAudioWrapper but no AudioList was set.");
            return false;
        }

        AudioWrapper audioToTest = OwnedAudioObject.GetPlayableAudio();
        return PlayAudio(audioToTest, SuppressWarnings);
    }

    /// <summary>
    /// Plays an audio from the supplied ScriptableAudio.
    /// </summary>
    /// <param name="SuppliedAudio"></param>
    public bool PlayAudio(ScriptableAudio SuppliedAudio, bool SuppressWarnings = false)
    {
        if (SuppliedAudio)
        {
            AudioWrapper audioToTest = SuppliedAudio.GetPlayableAudio();
            return PlayAudio(audioToTest, SuppressWarnings);
        }
        if (!SuppressWarnings)
            Debug.LogWarning("AudioChannel called PlayAudio but ScriptableAudio was null.");
        return false;
    }

    /// <summary>
    /// Play AudioWrapper but fetches the AudioWrapper for you using a string from the AudioManager.
    /// </summary>
    /// <param name="SourceAudio"></param>
    public bool PlayAudio(string SourceAudio, bool SuppressWarnings = false)
    {
        ScriptableAudio source = AudioManager.GetAudioWrapper(SourceAudio);
        if (source)
        {
            AudioWrapper audioToTest = source.GetPlayableAudio();
            return PlayAudio(audioToTest, SuppressWarnings);
        }
        if (!SuppressWarnings)
            Debug.LogWarning(string.Format("AudioChannel called PlayAudio but no AudioWrapper matched name {0}.", SourceAudio));
        return false;
    }

    /// <summary>
    /// Play AudioWrapper on this channel.
    /// </summary>
    /// <param name="SourceAudio"></param>
    public bool PlayAudio(AudioWrapper SourceAudio, bool SuppressWarnings = false)
    {
        if (SourceAudio != null)
        {
            if (SourceAudio.SoundType == AudioWrapper.eSoundType.QuickPlay)
            {
                PlayOneshot(SourceAudio);
                return true;
            }
            else
            {
                Play(SourceAudio, SuppressWarnings);

                //Check if the current playing sound matches the one supplied here. If it does that means we are actually playing the sound
                //This check is important because its possible we try to play a sound but it failed to due to priority levels.
                if (SourceAudio == CurrentAudioWrapperPlayed)
                {

                    //If the AudioWrapper has subtitle info, we need to pass it along to the AudioManager.
                    if (AudioManager.ContainsSubtitleData(SourceAudio))
                    {
                        AudioManager.SetSubtitleAudio(SourceAudio);
                    }
                    //If it doesn't and it's commentator audio, warn the user because ALL commentator audio should have subtitles.
                    else if (SourceAudio.ChannelType == AudioManager.eChannelType.Commentator)
                    {
                        if (!SuppressWarnings)
                            Debug.LogWarning(string.Format("Warning: AudioWrapper {0} is Commentator audio but lacks subtitles for language {1}.", 
                            SourceAudio.name, AudioManager.GetSubtitleLanguage()));
                    }

                    return true;
                }

                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns whether the source is active or not.
    /// </summary>
    /// <returns></returns>
    public bool IsPlaying()
    {
        return AudioSourceObject.isPlaying && AudioSourceObject.clip != null;
    }

    /// <summary>
    /// Attempt to play an AudioWrapper on the AudioSource managed by this channel. You are NOT guaranteed the sound is actually played.
    /// It's possible it may not be due to various game settings.
    /// </summary>
    /// <param name="SourceAudio"></param>
    private void Play(AudioWrapper SourceAudio, bool SuppressWarnings = false)
    {
        if (IsPlaying())
        {
            if(SourceAudio == CurrentAudioWrapperPlayed)
            {
                return;
            }

            //If the sound that is requesting play has less priority than the current sound then we ignore the request.
            if (SourceAudio.SoundPriorityLevel < CurrentAudioWrapperPlayed.SoundPriorityLevel)
            {
                return;
            }
            //If they're the same priority then we must delegate based on SamePriorityLevelInterruptsAudioFlow value.
            else if (SourceAudio.SoundPriorityLevel == CurrentAudioWrapperPlayed.SoundPriorityLevel && SamePriorityLevelInterruptsAudioFlow == true)
            {
                return;
            }
            //If we're here that means the new audio is taking over. We're going to stop the current audio.
            else
            {
                StopAudioSource();
            }
        }

        //Check if the Channel types match. If they don't warn the editor since this is almost certainly not intended.
        if (SourceAudio.ChannelType != ChannelType)
        {
            if (!SuppressWarnings)
                Debug.LogWarning("Channel is playing an AudioWrapper of type " + SourceAudio.ChannelType +
                " but channel is type " + ChannelType + ".");
        }

        PlayAudioSource(SourceAudio);
    }

    /// <summary>
    /// Play an AudioWrapper on the source as a OneShot. OneShots do NOT trigger events in any way.
    /// </summary>
    /// <param name="SourceAudio"></param>
    private void PlayOneshot(AudioWrapper SourceAudio)
    {
        AudioSourceObject.PlayOneShot(SourceAudio.AudioClip);
    }

    /// <summary>
    /// Start the source and send an OnChannelAudioPlayed event.
    /// </summary>
    private void PlayAudioSource(AudioWrapper SourceAudio)
    {
        AudioSourceObject.clip = SourceAudio.AudioClip;
        AudioSourceObject.Play();
        CurrentlyPaused = false;
        CurrentAudioWrapperPlayed = SourceAudio;
        SendOnChannelAudioPlayedEvent();
    }

    #endregion

    //All functions involving stopping audio to the audio source are here
    #region Stop Functions

    /// <summary>
    /// Stop the AudioChannel from playing if a sound is playing. Will also forcibly prevent auto-playback.
    /// </summary>
    public void StopAudio()
    {
        //To prevent playback issues, I'm doing a little hack here where I temporarily null out AudioObject to prevent autoplay (if its enabled).
        //After the event passes over and we are certain to be not attempting a replay we assign the value.
        if (IsPlaying())
        {
            ScriptableAudio temp = OwnedAudioObject;
            OwnedAudioObject = null;

            StopAudioSource();

            OwnedAudioObject = temp;
        }
    }

    /// <summary>
    /// Flush the source and send an OnChannelAudioStopped event.
    /// </summary>
    private void StopAudioSource()
    {
        AudioSourceObject.Stop();
        CurrentlyPaused = false;
        AudioSourceObject.clip = null;
        CurrentAudioWrapperPlayed = null;
        SendOnChannelAudioStoppedEvent();
        
    }

    #endregion

    //All functions involving pausing audio to the audio source are here
    #region Pause Functions

    /// <summary>
    /// Pause/Unpause a currently playing AudioWrapper.
    /// </summary>
    /// <param name="Pause"></param>
    public void PauseAudioWrapper(bool Pause = true)
    {
        PauseAudioSource(Pause);
    }

    /// <summary>
    /// Pause the source and send an OnChannelAudioPaused event.
    /// </summary>
    private void PauseAudioSource(bool Pausing)
    {
        if (AudioSourceObject.clip == null)
        {
            Debug.LogWarning("Pause requested but no audio is currently being played in the source.");
            return;
        }

        if (Pausing && AudioSourceObject.isPlaying == true)
        {
            AudioSourceObject.Pause();
            CurrentlyPaused = true;
        }
        else if (Pausing == false && AudioSourceObject.isPlaying == false)
        {
            AudioSourceObject.UnPause();
            CurrentlyPaused = false;
        }
        else
        {
            Debug.LogWarning("Pause called on channel " + ChannelName + " but source was already paused/playing.");
            return;
        }

        SendOnChannelAudioPausedEvent(Pausing);
    }

    #endregion

    //All functions involving event handling in regards to audio are here
    #region Event Handling

    /// <summary>
    /// Calls the EventManager to trigger this audio channels play event.
    /// </summary>
    private void SendOnChannelAudioPlayedEvent()
    {
        if (SendEventsWhenAudioPlayed)
        {
            EventManager.TriggerEvent(gameObject, "OnAudioChannelPlayed:" + GetEventSignature());
        }
    }

    /// <summary>
    /// Calls the EventManager to trigger this audio channels stop event.
    /// </summary>
    private void SendOnChannelAudioStoppedEvent()
    {
        if (SendEventsWhenAudioStopped)
        {
            EventManager.TriggerEvent(gameObject, "OnAudioChannelStopped:" + GetEventSignature());
        }
    }

    /// <summary>
    /// Calls the EventManager to trigger this audio channels pause event.
    /// </summary>
    private void SendOnChannelAudioPausedEvent(bool Pausing)
    {
        if (SendEventsWhenAudioStopped)
        {
            EventManager.TriggerEvent(gameObject, "OnAudioChannelPaused:" + GetEventSignature(),
                new OnAudioPausedEventParams(this, Pausing));
        }
    }

    /// <summary>
    /// This returns the unique AudioChannel signature that is appended by all events generated by this channel.
    /// </summary>
    /// <returns></returns>
    public string GetEventSignature()
    {
        return ChannelName + ":" + GetHashCode().ToString();
    }

    #endregion Functions

    #region Volume Controls

    public float GetSourceVolume()
    {
        return AudioSourceObject.volume;
    }

    public void SetSourceVolume(float vol)
    {
        AudioSourceObject.volume = vol;
    }

    #endregion

    /// <summary>
    /// This function can be used to set an AudioList to be stored in the AudioChannel for auto play purposes.
    /// </summary>
    /// <param name="ListToPlay"></param>
    public void SetAudioList(AudioList ListToPlay)
    {
        OwnedAudioObject = ListToPlay;
    }

    /// <summary>
    /// Automatic function call that is hooked into the EventManager.
    /// </summary>
    private void AutoplayAudio()
    {
        if (OwnedAudioObject == null)
        {
            return;
        }

        PlayAudio();
    }

    /// <summary>
    /// Sets the channel to auto play. Will also set SendEventsWhenAudioStopped to true automatically so events can be fired to auto play if 
    /// AutoPlayEnabled is passed as true.
    /// </summary>
    /// <param name="AutoPlayEnabled"></param>
    public void SetAutoPlay(bool AutoPlayEnabled = true)
    {
        //Ignore sets that don't change anything.
        if (AutoPlayEnabled == AutomaticChannelPlayback)
        {
            return;
        }

        AutomaticChannelPlayback = AutoPlayEnabled;

        if (AutomaticChannelPlayback == false)
        {
            StopListen(gameObject, "OnAudioChannelStopped:" + GetEventSignature(), AutoplayAudio);
        }
        else
        {
            Listen(gameObject, "OnAudioChannelStopped:" + GetEventSignature(), AutoplayAudio);
            SendEventsWhenAudioStopped = true;
        }
    }

    /// <summary>
    /// Called when the PauseManager pauses the game.
    /// </summary>
    private void OnMenuPause()
    {
        if (!IgnoreIngamePauses && IsPlaying())
        {
            PauseAudioSource(true);

            if (SendEventsWhenAudioPaused)
            {
                OnAudioPausedEventParams param = new OnAudioPausedEventParams(this, true);

                EventManager.TriggerEvent(gameObject, "OnAudioChannelPaused" + GetEventSignature(), param);
            }
        }
    }

    /// <summary>
    /// Called when the PauseManager unpauses the game.
    /// </summary>
    private void OnMenuUnpause()
    {
       if (AudioSourceObject.clip != null && IsPlaying() == false)
        {
            PauseAudioSource(false);

            if (SendEventsWhenAudioPaused)
            {
                OnAudioPausedEventParams param = new OnAudioPausedEventParams(this, false);

                EventManager.TriggerEvent(gameObject, "OnAudioChannelPaused" + GetEventSignature(), param);
            }
        }
    }
}