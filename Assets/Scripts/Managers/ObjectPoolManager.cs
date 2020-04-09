using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    List<ObjectPool> m_Pools = new List<ObjectPool>();

    protected override void OnAwake()
    {
    }

    private static ObjectPool CreateNewPool(GameObject prefab)
    {
        ObjectPool pool = new ObjectPool();
        pool.PoolType = typeof(GameObject);
        pool.ObjectName = prefab.name;
        Instance().m_Pools.Add(pool);
        return pool;
    }

    public static ObjectPool GetPool(string name, Type type)
    {
        var matchingPools = Instance().m_Pools.Where(x => x.ObjectName == name && x.PoolType == type);

        if (matchingPools.Any())
        {
            return matchingPools.First();
        }
        return null;
    }

    public static PooledObject Get(GameObject prefab)
    {
        ObjectPool objPool = GetPool(prefab.name, typeof(GameObject));

        if (objPool == null)
        {
            objPool = CreateNewPool(prefab);
        }

        PooledObject newObject = objPool.GetObject(prefab);
        newObject.Activate();

        return newObject;
    }

    public static PooledObject Get(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        PooledObject newObject = Get(prefab);
        newObject.gameObject.transform.position = pos;
        newObject.gameObject.transform.rotation = rot;
        return newObject;
    }

    public static PooledObject ObjectCreator(GameObject prefab)
    {
        GameObject newObject = (GameObject)Instantiate(prefab);
        PooledObject pobj = new PooledObject();
        pobj.gameObject = newObject;

        return pobj;
    }

    protected override void OnSceneUnload(Scene scene)
    {
        foreach(ObjectPool pool in m_Pools)
        {
            pool.ClearPool();
        }
    }
}
