using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// Plays a list in order.
/// </summary>
[System.Serializable, CreateAssetMenu(fileName = "QueueAudioListPrefab", menuName = "ScriptableObjects/AudioList/QueueAudioList", order = 3)]
public class QueueAudioList : AudioList
{
    [SerializeField, Tooltip("Where to start in the queue.")]
    public ScriptableAudio StartPoint = null;

    private int QueueIndex = 0;

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

            Debug.LogWarning("Supplied StartPoint does not exist in the queue.");
        }
    }


    protected override ScriptableAudio OnGetAudioObject()
    {
        ScriptableAudio audioToReturn = AudioObjects[QueueIndex];
        QueueIndex++;

        if (QueueIndex >= AudioObjects.Count)
        {
            QueueIndex = 0;
        }

        return audioToReturn;
    }
}