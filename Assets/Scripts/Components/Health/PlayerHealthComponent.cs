
using UnityEngine;
using System.Collections.Generic;

public class PlayerHealthComponent : HealthComponent
{
    [Header("Invincibility"), SerializeField]
    protected bool GodMode = false;
    [SerializeField]
    protected float m_InvinceTime = 0.05f;
    [SerializeField]
    protected float m_RespwnInvinceTime = 0.2f;
    [SerializeField]
    protected bool PauseInvincibility = false;

    [SerializeField]
    Color FullHealthColor = Color.green;
    [SerializeField]
    Color DeadColor = Color.red;

    [Header("Health Visuals")]
    Renderer ArmScreen = null;

    Timer m_InvinceFrames = null;
    Timer m_RespawnFrames = null;

    Animator m_PlayerAnimator = null;
    int m_DeathLayerIndex = 0;
    float m_DeathLayerWeight = 0.0f;
    //all other index layers to make sure only death animations play on death
    List<int> SetMeToZeroBeforeDeath = null;
    

    int m_DeathCount = 0;

    protected override void Awake()
    {
        base.Awake();

        m_InvinceFrames = CreateTimer(m_InvinceTime, SetNotInvincible);
        m_RespawnFrames = CreateTimer(m_RespwnInvinceTime);

        ArmScreen = GameObject.Find("Armgun_Screen").GetComponent<Renderer>();
        ArmScreen.material.SetColor("_BaseColor", Color.Lerp(DeadColor, FullHealthColor, CurrentHealth / MaxHealth));

        m_PlayerAnimator = GetComponent<Animator>();
        if (m_PlayerAnimator != null)
        {
            m_DeathLayerIndex = m_PlayerAnimator.GetLayerIndex("Death");
            //list of SetMeToZeroBeforeDeath
            SetMeToZeroBeforeDeath = new List<int>();
            SetMeToZeroBeforeDeath.Add(m_PlayerAnimator.GetLayerIndex("Shooting"));
            SetMeToZeroBeforeDeath.Add(m_PlayerAnimator.GetLayerIndex("Aim"));
            SetMeToZeroBeforeDeath.Add(m_PlayerAnimator.GetLayerIndex("Unaim"));
            SetMeToZeroBeforeDeath.Add(m_PlayerAnimator.GetLayerIndex("Crouching"));
            SetMeToZeroBeforeDeath.Add(m_PlayerAnimator.GetLayerIndex("Uncrouch"));
            SetMeToZeroBeforeDeath.Add(m_PlayerAnimator.GetLayerIndex("Sliding"));
            SetMeToZeroBeforeDeath.Add(m_PlayerAnimator.GetLayerIndex("Crouch_Walking"));
            SetMeToZeroBeforeDeath.Add(m_PlayerAnimator.GetLayerIndex("Falling"));
            SetMeToZeroBeforeDeath.Add(m_PlayerAnimator.GetLayerIndex("Pause"));
            SetMeToZeroBeforeDeath.Add(m_PlayerAnimator.GetLayerIndex("Reverse_Pause"));
            SetMeToZeroBeforeDeath.Add(m_PlayerAnimator.GetLayerIndex("Running"));
            SetMeToZeroBeforeDeath.Add(m_PlayerAnimator.GetLayerIndex("WallRunning_Right"));
            SetMeToZeroBeforeDeath.Add(m_PlayerAnimator.GetLayerIndex("WallRunning_Left"));
            SetMeToZeroBeforeDeath.Add(m_PlayerAnimator.GetLayerIndex("Stop_Running"));
            SetMeToZeroBeforeDeath.Add(m_PlayerAnimator.GetLayerIndex("Slide2Crouch"));
            SetMeToZeroBeforeDeath.Add(m_PlayerAnimator.GetLayerIndex("Double_Jump"));
            SetMeToZeroBeforeDeath.Add(m_PlayerAnimator.GetLayerIndex("Jump"));
            SetMeToZeroBeforeDeath.Add(m_PlayerAnimator.GetLayerIndex("Landing"));
            SetMeToZeroBeforeDeath.Add(m_PlayerAnimator.GetLayerIndex("Uncrouch"));
        }

        Listen("OnGamePaused", OnPause);
        Listen("OnGameUnpaused", OnUnpause);
        Listen("OnCheckPointReached", OnCheckPointReached);
    }

    public override void OnTakeDamage(float amount, GameObject offender, bool ignoreInvincibility = false)
    {
        if (CanTakeDamage(ignoreInvincibility) == false)
        {
            return;
        }

        base.OnTakeDamage(amount, offender, ignoreInvincibility);

        OnDamageEventParam damEvent = new OnDamageEventParam(Attacker, amount, CurrentHealth, MaxHealth, MinHealth);
        EventManager.TriggerEvent("OnPlayerDamaged", damEvent);

        //if not already invincible, become invincible, but only if the dmg source doesnt ignore invincibility (so dps doesnt give immunity frames)
        if (!ignoreInvincibility && !m_IsInvincible)
        {
            SetInvincible();
            m_InvinceFrames.Restart();
        }
    }

    public override void OnTakeDPS(float amount, GameObject offender, bool ignoreInvincibility = false)
    {
        base.OnTakeDPS(amount, offender, ignoreInvincibility);
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        OnDeathEventParam args = new OnDeathEventParam(Attacker);

        EventManager.TriggerEvent("OnPlayerDeathEvent", args);

        DeathAnim(args);

        m_DeathCount++;
    }

    protected override void OnSoftReset()
    {
        m_RespawnFrames.Restart();
        SetNotInvincible();

        base.OnSoftReset();
    }

    public void ToggleGodMode()
    {
        GodMode = !GodMode;
    }

    protected override bool CanTakeDamage(bool ignoreInvincibility = false)
    {
        //we still need them to die if they jump in a pit
        if (GodMode || PauseInvincibility)
        {
            return false;
        }

        if (m_RespawnFrames.IsRunning || !m_isAlive || m_IsInvincible && !ignoreInvincibility)
        {
            return false;
        }

        return true;
    }

    void OnCheckPointReached(EventParam param)
    {
        Heal(100.0f);
    }

    public int GetDeathCounter()
    {
        return m_DeathCount;
    }

    protected void OnPause()
    {
        PauseInvincibility = true;
    }

    protected void OnUnpause()
    {
        PauseInvincibility = false;
    }

    public void DeathAnim(EventParam param)
    {
        if (m_PlayerAnimator != null)
        {
            //set everything but death layer to 0
            foreach (int layer in SetMeToZeroBeforeDeath)
            {
                m_PlayerAnimator.SetLayerWeight(layer, 0.0f);
            }
            //cycle through all attackers if no animation for scpecific then genereic death 
            if (Attacker.gameObject.CompareTag("DangerLiquid"))
            {
                //m_PlayerAnimator.SetInteger("DeathType", 3);
                m_DeathLayerWeight = Mathf.Lerp(m_DeathLayerWeight, 1.0f, 1);
                m_PlayerAnimator.SetLayerWeight(m_DeathLayerIndex, m_DeathLayerWeight);
                m_PlayerAnimator.Play("Sludge_Death", m_DeathLayerIndex);
            }
            else if (Attacker.gameObject.CompareTag("ElectricWall"))
            {
                //m_PlayerAnimator.SetInteger("DeathType", 2);
                m_DeathLayerWeight = Mathf.Lerp(m_DeathLayerWeight, 1.0f, 1);
                m_PlayerAnimator.SetLayerWeight(m_DeathLayerIndex, m_DeathLayerWeight);
                m_PlayerAnimator.Play("Electric_Death", m_DeathLayerIndex);
            }
            else
            {
                //m_PlayerAnimator.SetInteger("DeathType", 1);
                m_DeathLayerWeight = Mathf.Lerp(m_DeathLayerWeight, 1.0f, 1);
                m_PlayerAnimator.SetLayerWeight(m_DeathLayerIndex, m_DeathLayerWeight);
                m_PlayerAnimator.Play("Death", m_DeathLayerIndex);
            }
        }
    }

    protected override void CheckHealthBounds()
    {
        base.CheckHealthBounds();

        ArmScreen.material.SetColor("_BaseColor", Color.Lerp(DeadColor, FullHealthColor, CurrentHealth / MaxHealth));

    }
}
