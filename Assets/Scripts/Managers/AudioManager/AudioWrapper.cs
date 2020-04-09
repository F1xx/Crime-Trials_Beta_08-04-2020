using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "AudioWrapperPrefab", menuName = "ScriptableObjects/AudioWrapper", order = 2)]
public class AudioWrapper : ScriptableAudio
{
    [Tooltip("Set source audio here.")]
    public AudioClip AudioClip;

    [Tooltip("Set audio playback type here. WARNING: AudioWrappers set to QuickPlay will NOT send events when played/stopped by an" +
        " AudioChannel.")]
    public eSoundType SoundType = eSoundType.QuickPlay;

    [Tooltip("Set audio maturity filter here. Maturity level will be used to filter results when playing audio with maturity settings enabled.")]
    public eMaturityLevel MaturityLevel = eMaturityLevel.Mature;

    [Tooltip("What channel type this AudioWrapper 'should' be played on. It will still play on other channels but with warning.")]
    public AudioManager.eChannelType ChannelType;

    [Tooltip("Sound priority: if a channel is already playing a sound and a new one is queued, this will determine if" +
        "the sound is overwritten or ignored. Any AudioWrapper set to QuickPlay do not care about their Priority Level.")]
    public ePriorityLevel SoundPriorityLevel = ePriorityLevel.Normal;

    public enum eMaturityLevel
    {
        AllAudiences,//For all audiences.
        Teen,//For teens.
        Mature,//Mature only. Will be omitted when maturity settings are enabled.
    }

    public enum eSoundType
    {
        NormalPlay,//The sound will replace an existing sound in the AudioChannel.
        QuickPlay,//The sound will play on top of an existing sound in the AudioChannel. This does not send any events when played.
    }

    public enum ePriorityLevel
    {
        Lowest,//5
        Lower,//4
        Normal,//3
        Higher,//2
        Highest//1
    }

    /// <summary>
    /// Currently does nothing. Will potentially generate a wrapper using filename to set specifiers.
    /// </summary>
    /// <param name="TypeSpecifiers"></param>
    public void SetWrapper(string[] TypeSpecifiers)
    {
        //if (TypeSpecifiers[0] == "Quick")
        //{
        //    IsQuickPlay = true;
        //}

        //Channel = TypeSpecifiers[1];
    }

    public override AudioWrapper GetPlayableAudio()
    {

        if (AudioManager.Instance().MaturitySettings == false && MaturityLevel == eMaturityLevel.Mature)
            return null;

        return this;
    }
}
