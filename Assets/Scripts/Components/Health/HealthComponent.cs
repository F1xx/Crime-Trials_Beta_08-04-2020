using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
[RequireComponent(typeof(AudioChannel))]
public abstract class HealthComponent : BaseObject
{
    [Header("Health Events")]
    [Tooltip("If enabled, this object will send GameObject-specific OnRespawnEvent events to the EventManager.")]
    public bool SendEventsWhenRespawned = false;

    [Tooltip("If enabled, this object will send GameObject-specific OnDeathEvent events to the EventManager.")]
    public bool SendEventsWhenDead = false;

    [Header("Health")]
    public float MaxHealth = 100.0f;
    public float MinHealth = 0.0f;
    [SerializeField]
    protected float m_CurrentHealth = 100.0f;
    public float CurrentHealth { get { return m_CurrentHealth; } protected set { m_CurrentHealth = value; CheckHealthBounds(); } }

    [Header("Invincibility")]
    [SerializeField]
    protected bool m_IsInvincible = false;
    
    [Header("Respawn"),Tooltip("How long it takes to respawn")]
    public float RespawnLength = 2.0f;
    protected bool m_isAlive = true;
    protected GameObject Attacker = null;

    
    [SerializeField,Header("Audio")]
    public ScriptableAudio OnDeathAudio = null;
    public ScriptableAudio OnDamageAudio = null;
    public ScriptableAudio OnRespawnAudio = null;
    public ScriptableAudio OnHealAudio = null;

    [Tooltip("How often the damage sound can play when you're taking damage. This is important for DPS so you don't hear a million sounds.")]
    public float CooldownBetweenDamageAudio = 1.0f;
    protected Timer CooldownOnDamageAudioTimer;

    [Tooltip("How often the healing sound can play when you're healing. This is important for HPS so you don't hear a million sounds.")]
    public float CooldownBetweenHealAudio = 1.0f;
    protected Timer CooldownOnHealAudioTimer;

    protected AudioChannel m_HealthAudioChannel = null;

    protected override void Awake()
    {
        base.Awake();

        ParentObject = transform.root.gameObject;

        m_HealthAudioChannel = GetComponent<AudioChannel>();
        CooldownOnDamageAudioTimer = CreateTimer(CooldownBetweenDamageAudio);
        CooldownOnHealAudioTimer = CreateTimer(CooldownBetweenHealAudio);
    }

    protected virtual void Start()
    {
    }

    /// <summary>
    /// does nothing if you're invincible, otherwise deals damage and triggers an event
    /// </summary>
    /// <param name="amount">How much damage does this comp take</param>
    /// <param name="offender">who is hurting me</param>
    /// <param name="ignoreInvincibility">do you bypass invincibility (good for things like pits, killplanes, etc.</param>
    public virtual void OnTakeDamage(float amount, GameObject offender, bool ignoreInvincibility = false)
    {
        //if we cannot take damage, return
        if (CanTakeDamage(ignoreInvincibility) == false)
        {
            return;
        }

        Attacker = offender;
        CurrentHealth = CurrentHealth - amount;

        if (OnDamageAudio && !CooldownOnDamageAudioTimer.IsRunning)
        {
            m_HealthAudioChannel.PlayAudio(OnDamageAudio);
            CooldownOnDamageAudioTimer.StartTimer();
        }

        OnDamageEventParam damEvent = new OnDamageEventParam(Attacker, amount, CurrentHealth, MaxHealth, MinHealth);
        EventManager.TriggerEvent(gameObject, "OnDamageEvent", damEvent);
    }

    /// <summary>
    /// Same as OnTakeDamage but can handle DPS differently if desired
    /// </summary>
    /// <param name="amount">How much damage does this comp take</param>
    /// <param name="offender">who is hurting me</param>
    /// <param name="ignoreInvincibility">do you bypass invincibility (good for things like pits, killplanes, etc.</param>
    public virtual void OnTakeDPS(float amount, GameObject offender, bool ignoreInvincibility = false)
    {
        OnTakeDamage(amount, offender, ignoreInvincibility);
    }

    /// <summary>
    /// an event that plays if the owner dies.
    /// </summary>
    protected virtual void OnDeath()
    {
        m_isAlive = false;

        if (SendEventsWhenDead)
        {
            OnDeathEventParam deathParam = new OnDeathEventParam(Attacker);
            EventManager.TriggerEvent(gameObject, "OnDeathEvent", deathParam);
        }

        if (OnDeathAudio)
        {
            m_HealthAudioChannel.PlayAudio(OnDeathAudio);
        }

        SetInvincible();

        Character character = GetComponent<Character>();
        InputComponentBase input = GetComponent<InputComponentBase>();

        if(input != null)
        {
            input.SetDisabled();
        }
        if(character != null)
        {
            //character.OnDeath();
        }
    }

    /// <summary>
    /// resets when we respawn
    /// </summary>
    public virtual void Respawn()
    {
        //Ping events if we were dead when respawn was called.
        if (IsAlive() == false)
        {          
            if (OnRespawnAudio)
            {
                m_HealthAudioChannel.PlayAudio(OnRespawnAudio);
            }

            if (SendEventsWhenRespawned == true)
            {
                EventManager.TriggerEvent(gameObject, "OnRespawnEvent");
            }
        }

        //Reset life values.
        m_isAlive = true;
        CurrentHealth = MaxHealth;
        
        //Call respawn on the character.
        Character character = GetComponent<Character>();
        if(character)
        {
            character.Respawn();
        }

        //Re-enable inputs.
        InputComponentBase input = GetComponent<InputComponentBase>();
        if (input != null)
        {
            input.SetEnabled();
        }
    }

    /// <summary>
    /// Kills this entity immediately. No OnDeath is triggered.
    /// </summary>
    public void Kill()
    {
        CurrentHealth = 0.0f;
        Respawn();
    }

    public bool IsAlive()
    {
        return m_isAlive;
    }

    public void Heal(float amount)
    {
        CurrentHealth += amount;

        if (OnHealAudio && !CooldownOnHealAudioTimer.IsRunning)
        {
            m_HealthAudioChannel.PlayAudio(OnHealAudio);
            CooldownOnHealAudioTimer.StartTimer();    
        }
    }

    /// <summary>
    /// do we have less than max health
    /// </summary>
    /// <returns></returns>
    public bool NeedsHealing()
    {
        return CurrentHealth != MaxHealth;
    }

    //sets invincibility to whatever it is not (off if on, on if off)
    public void ToggleInvincibility()
    {
        m_IsInvincible = !m_IsInvincible;
    }

    public void SetNotInvincible()
    {
        m_IsInvincible = false;
    }

    public void SetInvincible()
    {
        m_IsInvincible = true;
    }

    /// <summary>
    /// checks if the player dies and ensures they cannot gain more than max health.
    /// Called whenever CurrentHealth is modified.
    /// </summary>
    protected virtual void CheckHealthBounds()
    {
        if (m_isAlive)
        {
            if (CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }
            else if (CurrentHealth <= MinHealth)
            {
                OnDeath();
            }
        }
    }

    protected override void OnSoftReset()
    {
        Respawn();
    }

    protected virtual bool CanTakeDamage(bool ignoreInvincibility = false)
    {
        if(m_IsInvincible && !ignoreInvincibility || !m_isAlive)
        {
            return false;
        }

        return true;
    }
}
