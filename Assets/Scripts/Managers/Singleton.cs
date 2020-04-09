using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public abstract class Singleton<Type> : Singleton where Type : BaseObject
{
    private static Type m_Instance;
    private static readonly object Lock = new object();

    /// <summary>
    /// Return an instance to the Singleton or create one if it doesn't exist.
    /// This can return null if the application is quitting when this is called.
    /// </summary>
    /// <returns></returns>
    public static Type Instance()
    {
        if (Quitting)
        {
            return null;
        }
        lock (Lock)
        {
            //return our Singleton
            if (m_Instance != null)
                return m_Instance;

            //if we dont have a reference find it
            var instances = FindObjectsOfType<Type>();
            var count = instances.Length;

            if (count > 0)
            {
                //Found our reference
                if (count == 1)
                {
                    return m_Instance = instances[0];
                }

                //There was more than 1 reference somehow... destroy the others
                Debug.LogWarning("There should never be more than one Singleton, but " + count + " were found. The first instance found will be used, and all others will be destroyed.");
                for (var i = 1; i < instances.Length; i++)
                {
                    Destroy(instances[i]);
                }
                return m_Instance = instances[0];
            }

            //We didnt create an instance yet so make an empty game object to contain our Singleton and return it.
            //Debug.Log(typeof(Type).ToString() + ": An instance is needed in the scene and no existing instances were found, so a new instance will be created.");
            m_Instance = new GameObject($"{typeof(Type).ToString()}").AddComponent<Type>();

            return m_Instance;
        }
    }

    /// <summary>
    /// Every singleton by default is going to subscribe to SoftReset, HardReset, OnSceneLoad and OnSceneUnload.
    /// </summary>
    protected override void Awake()
    {
        ListenToHardResetEvents = true;
        ListenToSoftResetEvents = true;

        base.Awake();

        SceneManager.sceneLoaded += OnSceneLoad;
        SceneManager.sceneUnloaded += OnSceneUnload;

        OnAwake();
    }

    protected virtual void OnSceneLoad(Scene scene, LoadSceneMode mode) { }

    protected virtual void OnSceneUnload(Scene scene) { }

    protected abstract void OnAwake();

    protected override void OnDestroy()
    {
        base.OnDestroy();
        SceneManager.sceneLoaded -= OnSceneLoad;
        SceneManager.sceneUnloaded -= OnSceneUnload;
    }
}

[System.Serializable]
public abstract class Singleton : BaseObject
{
    public static bool Quitting { get; private set; }
    private void OnApplicationQuit()
    {
        Quitting = true;
    }
}
