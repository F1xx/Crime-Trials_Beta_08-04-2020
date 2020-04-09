using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PooledObject
{
    public ObjectPool m_OwningPool = null;
    public GameObject gameObject = null;

    Timer m_DeathTimer = null;

    public void Activate()
    {
        if (gameObject)
        {
            gameObject.transform.root.gameObject.SetActive(true);
        }
    }

    public void Deactivate()
    {
        if(m_DeathTimer != null && m_DeathTimer.IsRunning)
        {
            m_DeathTimer.StopTimer();
            m_DeathTimer = null;
        }

        if (gameObject)
        {
            gameObject.transform.root.gameObject.SetActive(false);
        }
    }

    public void SetSelfDeactivation(float timeUntilDeactivation)
    {
        if(timeUntilDeactivation > 0.0f)
        {
            m_DeathTimer = TimerManager.MakeOneShotAutoStartTimer(timeUntilDeactivation, Deactivate);
        }
    }
}
