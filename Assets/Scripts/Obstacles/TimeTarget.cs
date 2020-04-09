using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTarget : BaseObject
{
    [Header("TimeTarget Requirements"), SerializeField]
    MultipleParticleSystemHandler ParticlesToSpawnOnHit = null;
    [SerializeField]
    Shootable ShootableObject = null;

    [Header("TimeTarget Effect"), SerializeField]
    float TimeSavedOnShot = 3.0f;

    [Header("TimeTarget Movement"), SerializeField, Tooltip("Changes how much the direction effects the movement. 0 = not at all, 1 = completely.")]
    Vector3 MovementWeights = new Vector3(0.0f, 1.0f, 0.0f);
    [SerializeField]
    float MovementFrequency = 1.0f;
    float m_Frequency;
    float m_Phase = 0.0f;
    [SerializeField]
    float MovementAmplitude = 1.0f;

    [Header("TimeTarget Audio"), SerializeField]
    SpawnedOneShotAudio SpawnedAudioOnPop = null;
    [SerializeField]
    ScriptableAudio AudioToPlayOnPop = null;


    protected override void Awake()
    {
        base.Awake();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        MovementWeights.x = Mathf.Clamp(MovementWeights.x, 0.0f, 1.0f);
        MovementWeights.y = Mathf.Clamp(MovementWeights.y, 0.0f, 1.0f);
        MovementWeights.z = Mathf.Clamp(MovementWeights.z, 0.0f, 1.0f);
    }
#endif

    private void Update()
    {
        MoveViaSine();
    }

    void MoveViaSine()
    {
        if(MovementFrequency != m_Frequency)
        {
            CalcNewFreq();
        }

        Vector3 pos = transform.position;

        float sine = Mathf.Sin(Time.time * MovementFrequency + m_Phase) * Time.deltaTime * MovementAmplitude;

        pos.x += sine * MovementWeights.x;
        pos.y += sine * MovementWeights.y;
        pos.z += sine * MovementWeights.z;

        if(pos != transform.position)
        {
            transform.position = pos;
        }
    }

    void CalcNewFreq()
    {
        float curr = (Time.time * m_Frequency + m_Phase) % (2.0f * Mathf.PI);
        float next = (Time.time * MovementFrequency) % (2.0f * Mathf.PI);
        m_Phase = curr - next;
        m_Frequency = MovementFrequency;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            ShootBullet sb = collision.gameObject.GetComponent<ShootBullet>();

            if (sb)
            {
                if (sb.ParentObject.CompareTag("Player"))
                {
                    Die();
                }
            }
        }
    }

    protected void Die()
    {
        PlayEffectOnce();
        PlaySoundOnce();
        WorldTimeManager.RemoveTime(TimeSavedOnShot);
        ShootableObject.SetCanBeShot(false);
        ShootableObject.TriggerShotEvent();

        EventManager.TriggerEvent("OnTargetDestroyed");

        gameObject.transform.root.gameObject.SetActive(false);
    }

    protected void PlayEffectOnce()
    {
        ParticlesToSpawnOnHit.Play(transform.position);
    }

    protected void PlaySoundOnce()
    {
        SpawnedAudioOnPop.Spawn(AudioToPlayOnPop, transform.position);
    }
}
