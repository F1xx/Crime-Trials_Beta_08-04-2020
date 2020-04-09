using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to spawn an audio source wherever it is when it is called.
/// It feels a little weird but it literally just clones itself wherever it is
/// </summary>
[RequireComponent(typeof(AudioChannel))]
public class SpawnedOneShotAudio : MonoBehaviour
{
    AudioChannel Channel = null;
    [SerializeField]
    ScriptableAudio AudioToPlay = null;

    public void StartPlaying()
    {
        Channel = GetComponent<AudioChannel>();
        Channel.OwnedAudioObject = AudioToPlay;

        Channel.PlayAudio();
    }

    /// <summary>
    /// Spawn the audio at location
    /// </summary>
    /// <param name="audio">audio to play</param>
    /// <param name="transform">location to play audio</param>
    public void Spawn(ScriptableAudio audio, Vector3 pos)
    {
        //get a gameobject of our own type form the pool
        PooledObject pooled = ObjectPoolManager.Get(gameObject);
        pooled.gameObject.transform.position = pos;

        SpawnedOneShotAudio sao = pooled.gameObject.GetComponent<SpawnedOneShotAudio>();

        //tell it what to play
        sao.SetAudio(audio);
        //tell it to kill itself
        pooled.SetSelfDeactivation(sao.GetLengthOfAudio());
        //play
        sao.StartPlaying();
    }

    public void SetAudio(ScriptableAudio audio)
    {
        AudioToPlay = audio;
    }

    public float GetLengthOfAudio()
    {
        return AudioToPlay.GetPlayableAudio().AudioClip.length;
    }
}
