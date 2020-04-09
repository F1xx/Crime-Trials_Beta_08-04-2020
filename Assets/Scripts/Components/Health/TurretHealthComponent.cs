using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretHealthComponent : HealthComponent
{
    protected Timer m_RespawnTimer = null;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override void Awake()
    {
        base.Awake();
        m_RespawnTimer = CreateTimer(RespawnLength, Respawn);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnTakeDamage(float amount, GameObject offender, bool ignoreInvincibility = false)
    {
        base.OnTakeDamage(amount, offender, ignoreInvincibility);
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        EventManager.TriggerEvent("FlyingTurretDeath");
        EventManager.TriggerEvent("GroundTurretDeath");
        m_RespawnTimer.StartTimer();
    }

    public override void Respawn()
    {
        base.Respawn();

        SetNotInvincible();
        m_RespawnTimer.Reset();
    }
    protected override void CheckHealthBounds()
    {
        base.CheckHealthBounds();
    }
}
