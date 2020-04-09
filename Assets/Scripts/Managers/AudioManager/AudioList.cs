using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AudioList class. This class does 3 things:
///  - Holds a list of ScriptableAudio.
///  - Grabs audio from the list using specified algorithms that children are responsible for creating.
///  - Basic safety handling and uniqueness ensuring.
/// </summary>
[System.Serializable]
public abstract class AudioList : ScriptableAudio
{
    [SerializeField, Tooltip("Add all audio that belongs in the list here. Please do not add yourself to the list. It shouldn't break " +
        "anything but you really shouldn't test it.")]
    public List<ScriptableAudio> AudioObjects = new List<ScriptableAudio>();

    [SerializeField, Tooltip("Ensures that each time the AudioList returns a ScriptableAudio that it will be different than the last " +
        "ScriptableAudio sent. This will be overridden if the list only contains 1 or less objects and forced to be false.")]
    bool EnsuresUniqueSound = true;

    private AudioWrapper LastAudioWrapperDispatched = null;

    static private readonly int MaximumAttemptsToFindSuitableAudio = 10;

    protected virtual void OnEnable()
    {
        //Clear the list of any objects that are equal to this object. Prevents infinite loops later.
        for (int i = 0; i < AudioObjects.Count; i++)
        {
            if (AudioObjects[i] == this)
            {
                AudioObjects.RemoveAt(i);
                i--;
            }
        }

        //Override the user-set flag if the list cannot support unique sounds due to size limitations.
        EnsuresUniqueSound = AudioObjects.Count <= 1 ? false : EnsuresUniqueSound;
    }

    /// <summary>
    /// Get a ScriptableAudio from the list.
    /// </summary>
    /// <returns></returns>
    private ScriptableAudio GetAudioObject()
    {
        return OnGetAudioObject();
    }

    /// <summary>
    /// Abstract function that contains the algorithm for fetching an AudioWrapper from the derived class.
    /// </summary>
    /// <returns></returns>
    protected abstract ScriptableAudio OnGetAudioObject();

    /// <summary>
    /// Request an AudioWrapper from this List. This is where we ensure that uniqueness is met if it is set and feasible.
    /// </summary>
    /// <returns></returns>
    public override AudioWrapper GetPlayableAudio()
    {
        if (AudioObjects != null && AudioObjects.Count > 0)
        {
            AudioWrapper audioWrapperToSend = null;

            //adding a safety net here in case somehow this loops multiple times because a list had incompatible audio in it with certain settings
            int AttemptsCounter = 0;

            do
            {
                AttemptsCounter++;
                ScriptableAudio AudioObject = GetAudioObject();

                //After getting a ScriptableAudio from the list, we get an AudioWrapper from it.
                if (AudioObject)
                {
                    audioWrapperToSend = AudioObject.GetPlayableAudio();
                }

                //If the wrapper isn't null (it's only null because of maturity), check uniqueness settings and compare if needed.
                if (EnsuresUniqueSound && audioWrapperToSend != null && LastAudioWrapperDispatched == audioWrapperToSend)
                {
                    audioWrapperToSend = null;
                }

                //If we attempted this too many times in a row. Pass an error to the console.
                if (audioWrapperToSend == null && AttemptsCounter >= MaximumAttemptsToFindSuitableAudio)
                {
                    Debug.LogError("AudioList exceeded maximum attempts searching for playable audio. Perhaps this list does not contain " +
                        "any audio suitable for all audiences.");
                    return null;
                }

            } while (audioWrapperToSend == null);

            LastAudioWrapperDispatched = audioWrapperToSend;
            return audioWrapperToSend;
        }
        else
        {
            Debug.LogWarning("Requested ScriptableAudio from an empty AudioList.");
            return null;
        }
    }
}