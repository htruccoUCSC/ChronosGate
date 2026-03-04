using System;
using System.Collections.Generic;

/// <summary>
/// Represents the player's current progression state.
/// This is what gets saved/loaded.
/// </summary>
[Serializable]
public class PlayerProgressionData
{
    public List<string> unlockedUnitIDs = new List<string>();
    public int highestWaveReached = 0;
    public bool isProgressionMode = true;
    
    public PlayerProgressionData()
    {
        unlockedUnitIDs = new List<string>();
        highestWaveReached = 0;
        isProgressionMode = true;
    }
}
