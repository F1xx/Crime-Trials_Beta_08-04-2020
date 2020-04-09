using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanHealthComponent : HealthComponent
{
    /// <summary>
    /// an event that plays if the owner dies.
    /// </summary>
    protected override void OnDeath()
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
    }

    public override void Respawn()
    {
        base.Respawn();
        SetNotInvincible();
    }

}
