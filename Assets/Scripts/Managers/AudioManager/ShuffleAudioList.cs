using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// Same as QueueAudioList but shuffles after every loop.
/// </summary>
[System.Serializable, CreateAssetMenu(fileName = "ShuffleAudioListPrefab", menuName = "ScriptableObjects/AudioList/ShuffleAudioList", order = 4)]
public class ShuffleAudioList : AudioList
{

    [SerializeField, Tooltip("Where to start in the queue.")]
    public ScriptableAudio StartPoint = null;

    private int QueueIndex = 0;
    private static RNGCryptoServiceProvider RNGProvider = new RNGCryptoServiceProvider();

    protected override void OnEnable()
    {
        base.OnEnable();

        if (StartPoint != null && AudioObjects.Count > 0)
        {
            for (int i = 0; i < AudioObjects.Count; i++)
            {
                if (StartPoint == AudioObjects[i])
                {
                    QueueIndex = i;
                    return;
                }
            }

            Debug.LogWarning("Supplied StartPoint does not exist in the shuffle.");
        }
    }

    protected override ScriptableAudio OnGetAudioObject()
    {
        ScriptableAudio audioToReturn = AudioObjects[QueueIndex];
        QueueIndex++;

        if (QueueIndex >= AudioObjects.Count)
        {
            QueueIndex = 0;
            Shuffle();
        }

        return audioToReturn;
    }

    private void Shuffle()
    {
        int iterations = AudioObjects.Count;

        //Shuffle using crypto logic I found online.
        while (iterations > 1)
        {
            byte[] box = new byte[1];

            do
            {
                RNGProvider.GetBytes(box);
            }
            while (!(box[0] < iterations * (byte.MaxValue / iterations)));

            int swapVal = (box[0] % iterations);
            iterations--;

            ScriptableAudio value = AudioObjects[swapVal];
            AudioObjects[swapVal] = AudioObjects[iterations];
            AudioObjects[iterations] = value;
        }
    }
}
