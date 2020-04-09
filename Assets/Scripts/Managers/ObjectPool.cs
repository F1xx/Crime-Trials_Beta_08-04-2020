using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ObjectPool
{
    public Type PoolType;
    public string ObjectName = "";

    List<PooledObject> m_PoolGameObjects = new List<PooledObject>();

    public int ObjectCount
    {
        get { return m_PoolGameObjects.Count; }
    }

    public PooledObject GetObject(GameObject prefab)
    {
        PooledObject retrievedObject = null;

        var disabledObjects = m_PoolGameObjects.Where(x => x.gameObject.activeSelf == false);

        if (disabledObjects.Any())
        {
            retrievedObject = disabledObjects.First();
        }
        else
        {
            retrievedObject = CreateNewObject(prefab);
        }

        retrievedObject.Activate();

        return retrievedObject;
    }

    public PooledObject CreateNewObject(GameObject prefab)
    {
        PooledObject returnObject = ObjectPoolManager.ObjectCreator(prefab);
        returnObject.m_OwningPool = this;
        m_PoolGameObjects.Add(returnObject);

        return returnObject;
    }

    //avoid using this. It is not proper memory management.
    public void ClearPool()
    {
        m_PoolGameObjects.Clear();
    }
}
