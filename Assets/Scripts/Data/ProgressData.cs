using System.Collections.Generic;

[System.Serializable]
public class ProgressData
{
    public List<Level> levels = new List<Level>();
    public int lastCompletedLevel = 0;
}