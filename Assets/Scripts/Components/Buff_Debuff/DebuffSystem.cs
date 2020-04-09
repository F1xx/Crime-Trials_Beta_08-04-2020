using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DebuffSystem : BaseObject
{
    [SerializeField]
    List<Debuff> Debuffs = new List<Debuff>();

    private HashSet<Debuff> m_DebuffsToRemove = new HashSet<Debuff>();
    private HashSet<Debuff> m_DebuffsToAdd = new HashSet<Debuff>();


    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.B))
        //{
        //    QueueForAddition(new JumpDebuff(10.0f, 2.0f, this, this.gameObject));
        //}
        //if (Input.GetKeyDown(KeyCode.N))
        //{
        //    QueueForAddition(new MovementDebuff(10.0f, 2.0f, this, this.gameObject));
        //}
    }

    public List<Debuff> GetAllDebuffs()
    {
        return Debuffs;
    }

    public List<T> GetAllDebuffsOfType<T>() where T : Debuff
    {
        List<T> debuffs = new List<T>();

        foreach(var debuff in Debuffs)
        {
            if(debuff.Is<T>())
            {
                T debuffCasted = (T)debuff;
                debuffs.Add(debuffCasted);
            }
        }

        return debuffs;
    }

    public List<T> GetAllDebuffsOfTypeFromCauser<T>(GameObject causer) where T : Debuff
    {
        List<T> debuffs = new List<T>();

        foreach (var debuff in Debuffs)
        {
            if (debuff.Is<T>() && debuff.Causer == causer)
            {
                T debuffCasted = (T)debuff;
                debuffs.Add(debuffCasted);
            }
        }

        return debuffs;
    }

    public void RemoveAllDebuffsofType<T>() where T : Debuff
    {
        for(int i = 0; i < Debuffs.Count; i++)
        {
            if (Debuffs[i].Is<T>())
            {
                QueueForRemoval(Debuffs[i]);
            }
        }
    }

    public void RemoveAllDebuffsofTypeFromCauser<T>(GameObject causer) where T : Debuff
    {
        for (int i = 0; i < Debuffs.Count; i++)
        {
            if (Debuffs[i].Is<T>() && Debuffs[i].Causer == causer)
            {
                QueueForRemoval(Debuffs[i]);
            }
        }
    }

    #region Memory Management
    private void LateUpdate()
    {
        foreach (var timer in m_DebuffsToRemove)
        {
            RemoveDebuff(timer);
        }
        m_DebuffsToRemove.Clear();

        foreach (var timer in m_DebuffsToAdd)
        {
            Debuffs.Add(timer);
        }
        m_DebuffsToAdd.Clear();

        Debuffs.TrimExcess();
    }

    private void RemoveDebuff(Debuff debuff)
    {
        if (debuff != null)
        {
            var debuffs = Debuffs.Where(x => x == debuff).ToList();
            if (debuffs.Any())
            {
                Debuffs.Remove(debuff);
            }
        }
    }

    public void QueueForRemoval(Debuff debuff)
    {
        if (debuff != null)
        {
            m_DebuffsToRemove.Add(debuff);
        }
    }

    public void QueueForAddition(Debuff debuff)
    {
        m_DebuffsToAdd.Add(debuff);
    }

    void ClearDebuffs()
    {
        foreach(var debuff in Debuffs)
        {
            debuff.DestroyDebuff();
        }
        Debuffs.Clear();
    }

    protected override void OnSoftReset()
    {
        ClearDebuffs();
    }

    protected override void OnDestroy()
    {
        ClearDebuffs();
    }
    #endregion
}
