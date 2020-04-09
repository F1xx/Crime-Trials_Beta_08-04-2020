using UnityEngine;

[System.Serializable]
public abstract class ScriptableAudio : ScriptableObject
{
    /// <summary>
    /// Get an AudioWrapper from the ScriptableAudio. WARNING: can return null (typically with UnityEditor warnings).
    /// </summary>
    /// <returns></returns>
    public abstract AudioWrapper GetPlayableAudio();

    /// <summary>
    /// Request the ScriptableAudio to play a sound on a specific AudioChannel.
    /// </summary>
    /// <param name="ChannelToPlayIn">The AudioChannel that the ScriptableAudio will send itself to play in.</param>
    public void PlayAudioToChannel(AudioChannel ChannelToPlayIn)
    {
        if (ChannelToPlayIn != null)
        {
            ChannelToPlayIn.PlayAudio(this);
        }
        else
        {
            Debug.LogWarning(string.Format("ScriptableAudio.{0}: called PlayAudioToChannel with null AudioChannel.", name));
        }
    }

}
