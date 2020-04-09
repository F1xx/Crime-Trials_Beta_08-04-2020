using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PrefabManager : Singleton<PrefabManager>
{
    private Dictionary<string, GameObject> m_PrefabList = new Dictionary<string, GameObject>();
    private string LoadFilePath;

    public List<string> PrefabsLoaded = new List<string>();

    protected override void OnAwake()
    {
        if (m_PrefabList.Count == 0)
        {
            DontDestroyOnLoad(this);
            LoadFilePath = Application.dataPath + "/Resources/Prefabs";
            CollectAllFilesToLoad(LoadFilePath);
            LoadAllPrefabs();
        }
    }

    private void CollectAllFilesToLoad(string BaseDirectory)
    {
        if (Directory.Exists(BaseDirectory))
        {
            DirectoryInfo info = new DirectoryInfo(BaseDirectory);

            foreach (DirectoryInfo directory in info.GetDirectories())
            {
                CollectAllFilesToLoad(directory.FullName);
            }

            foreach (FileInfo item in info.GetFiles("*.prefab"))
            {
                //format string to be readable by unity
                string filename = item.FullName;
                filename = filename.Replace('\\', '/');
                filename = filename.Replace(LoadFilePath, "");
                filename = filename.Replace(".prefab", "");
                filename = "Prefabs" + filename;

                //ready to be used by unity resources
                PrefabsLoaded.Add(filename);
            }
        }
    }

    void LoadAllPrefabs()
    {
        for (int i = 0; i < PrefabsLoaded.Count; i++)
        {
            GameObject prefab = Resources.Load(PrefabsLoaded[i], typeof(GameObject)) as GameObject;

            string filename = Path.GetFileNameWithoutExtension(PrefabsLoaded[i]);
            filename = filename.ToUpper();

            m_PrefabList.Add(filename, prefab);
        }
    }

    //returns the prefab
    public static GameObject GetPrefab(string Name)
    {
        Name = Name.ToUpper();

        if (Instance().m_PrefabList.ContainsKey(Name) == false)
        {
            Debug.LogWarning("Prefab with name " + Name + " does not exist in this context.");
            return null;
        }

        return Instance().m_PrefabList[Name];
    }
}
