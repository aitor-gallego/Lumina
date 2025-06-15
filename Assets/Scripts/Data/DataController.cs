using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System;

public class DataController : MonoBehaviour
{
    public static DataController Instance;
    private string path;
    public ProgressData CurrentData;
    private bool save = true;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);

        path = Path.Combine(Application.persistentDataPath, "progress.json");
        Load();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        save = scene.name != "Final" && scene.name != "Menu";
    }

    public void SaveLevel(string levelName, float completionTime, int levelIndex)
    {
        if (!save)
            return;

        var entry = CurrentData.levels.FirstOrDefault(e => e.name == levelName);

        if (entry == null)
        {
            entry = new Level
            {
                name = levelName,
                completed = true,
                time = completionTime
            };
            CurrentData.levels.Add(entry);
        }
        else
        {
            entry.completed = true;
            if (entry.time == 0f || completionTime < entry.time)
                entry.time = completionTime;
        }

        if (levelIndex > CurrentData.lastCompletedLevel)
            CurrentData.lastCompletedLevel = levelIndex;

        Save();
    }

    public void Save()
    {
        if (!save)
            return;

        var json = JsonUtility.ToJson(CurrentData, true);
        var protectedBytes = SecureData.Encriptar(json);
        File.WriteAllBytes(path, protectedBytes);
    }

    public void Load()
    {
        if (File.Exists(path))
        {
            try
            {
                var bytes = File.ReadAllBytes(path);
                var json = SecureData.Desencriptar(bytes);
                CurrentData = JsonUtility.FromJson<ProgressData>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
                File.Delete(path);
                CurrentData = new ProgressData();
                Save();
            }
        }
        else
        {
            CurrentData = new ProgressData();
            Save();
        }
    }

    public void Reset()
    {
        CurrentData.lastCompletedLevel = 0;

        foreach (var entry in CurrentData.levels)
            entry.completed = false;

        Save();
    }
}