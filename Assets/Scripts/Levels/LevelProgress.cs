using UnityEngine;

public static class LevelProgress
{
    private const string ProgressKey = "MaxLevelUnlocked";

    public static int GetMaxUnlockedLevel()
    {
        return PlayerPrefs.GetInt(ProgressKey, 1);
    }

    public static void UnlockLevel(int levelIndex)
    {
        int current = GetMaxUnlockedLevel();
        if (levelIndex > current)
        {
            PlayerPrefs.SetInt(ProgressKey, levelIndex);
            PlayerPrefs.Save();
        }
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(ProgressKey);
    }
}
