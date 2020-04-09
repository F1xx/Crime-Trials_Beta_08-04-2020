using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Hypothetically this can be used to heal as well ironically
//Note this requires the collider of this object to be set as a trigger
[RequireComponent(typeof(Collider))]
[System.Serializable]
public class PainVolume : Hazard
{
    [Header("Damage")]
    public float m_EntryDamage = 0; //Damage Taken Upon First Contact
    public float m_DPS = 0; //Damage taken per second while remaining within
    public float m_ExitDamage = 0; //Damage taken upon leaving the volume

    [Header("Audio")]
    public ScriptableAudio AudioOnEnter = null;
    public float AudioOnEnterCooldown = 1.0f;
    Timer AudioOnEnterCooldownTimer;

    public ScriptableAudio AudioOnExit = null;
    public float AudioOnExitCooldown = 1.0f;
    Timer AudioOnExitCooldownTimer;

    public ScriptableAudio AudioOnStay = null;
    public float AudioOnStayCooldown = 1.0f;
    Timer AudioOnStayCooldownTimer;

    [Header("Special")]
    public bool IgnoreInvincibility = false;

    protected AudioChannel m_SoundChannel;

    bool m_IsOn = true;

    protected override void Awake()
    {
        base.Awake();

        ParentObject = transform.root.gameObject;
        AudioOnEnterCooldownTimer = CreateTimer(AudioOnEnterCooldown);
        AudioOnExitCooldownTimer = CreateTimer(AudioOnExitCooldown);
        AudioOnStayCooldownTimer = CreateTimer(AudioOnStayCooldown);
    }

    protected virtual void Start()
    {
        Collider objectCollider;

        objectCollider = GetComponent<Collider>();

        if (objectCollider)
        {
            objectCollider.isTrigger = true;
        }

        m_SoundChannel = GetComponentInChildren<AudioChannel>();
    }

    /// <summary>
    /// Turns the pain volume on or off (true = on, false = off)
    /// </summary>
    /// <param name="state">true is on, flase is off</param>
    public void TurnOnOff(bool state)
    {
        m_IsOn = state;
    }

    /// <summary>
    /// Basic trigger logic is run here when a collider touches the pain volume collider.
    /// </summary>
    /// <param name="other"></param>
    protected virtual void OnTriggerEnter(Collider other)
    {
        m_SoundChannel.transform.position = other.gameObject.transform.position;
        EnterDamage(other.gameObject);
    }

    /// <summary>
    /// Collision stay is called every frame a collider stays within the collider of the pain volume.
    /// Audio is set to playback every X seconds (as defined by CooldownBetweenCollisionOnStayAudio).
    /// </summary>
    /// <param name="other"></param>
    protected virtual void OnTriggerStay(Collider other)
    {
        m_SoundChannel.transform.position = other.gameObject.transform.position;
        StayDamage(other.gameObject);
    }

    /// <summary>
    /// This is called when the collider leaves this pain volume's collision radius.
    /// </summary>
    /// <param name="other"></param>
    protected virtual void OnTriggerExit(Collider other)
    {
        m_SoundChannel.transform.position = other.gameObject.transform.position;
        ExitDamage(other.gameObject);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        m_SoundChannel.transform.position = collision.gameObject.transform.position;
        EnterDamage(collision.gameObject);
    }

    protected virtual void OnCollisionExit(Collision collision)
    {
        m_SoundChannel.transform.position = collision.gameObject.transform.position;
        ExitDamage(collision.gameObject);
    }

    protected virtual void OnCollisionStay(Collision collision)
    {
        m_SoundChannel.transform.position = collision.gameObject.transform.position;
        StayDamage(collision.gameObject);
    }

    protected virtual void EnterDamage(GameObject obj)
    {
        if (m_IsOn)
        {

            if (AudioOnEnter && !AudioOnEnterCooldownTimer.IsRunning)
            {
                AudioOnEnterCooldownTimer.StartTimer();
                m_SoundChannel?.PlayAudio(AudioOnEnter);
            }

            if (m_EntryDamage != 0.0f)
            {
                HealthComponent health = obj.GetComponent<HealthComponent>();

                if (health != null)
                {
                    health.OnTakeDamage(m_EntryDamage, ParentObject, IgnoreInvincibility);
                }
            }
        }
    }

    protected virtual void ExitDamage(GameObject obj)
    {
        if (m_IsOn)
        {

            if (AudioOnExit && !AudioOnExitCooldownTimer.IsRunning)
            {
                AudioOnExitCooldownTimer.StartTimer();
                m_SoundChannel?.PlayAudio(AudioOnExit);
            }

            if (m_ExitDamage != 0.0f)
            {
                HealthComponent health = obj.GetComponent<HealthComponent>();

                if (health != null)
                {
                    health.OnTakeDamage(m_ExitDamage, ParentObject, IgnoreInvincibility);
                }
            }
        }
    }

    protected virtual void StayDamage(GameObject obj)
    {
        if (m_IsOn)
        {
            if (AudioOnStay && !AudioOnStayCooldownTimer.IsRunning)
            {
                AudioOnStayCooldownTimer.StartTimer();
                m_SoundChannel?.PlayAudio(AudioOnStay);
            }

            if (m_DPS != 0.0f)
            {
                HealthComponent health = obj.GetComponent<HealthComponent>();

                if (health != null)
                {
                    health.OnTakeDPS(m_DPS * Time.deltaTime, ParentObject, IgnoreInvincibility);
                }
            }
        }
    }

    protected override void OnSoftReset()
    {
        base.OnSoftReset();
        m_SoundChannel.transform.position = gameObject.transform.position;
    }
}