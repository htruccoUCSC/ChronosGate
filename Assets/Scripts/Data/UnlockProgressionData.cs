using System;
using System.Collections.Generic;

/// <summary>
/// Represents the unlock progression configuration.
/// Defines which units are available at start and the unlock sequence.
/// </summary>
[Serializable]
public class UnlockProgressionData
{
    public List<string> startingUnits;
    public List<WaveUnlock> unlockSequence;
}

/// <summary>
/// Defines which unit unlocks at a specific wave number.
/// </summary>
[Serializable]
public class WaveUnlock
{
    public int waveNumber;
    public string unitID;
    public string unlockMessage; // Optional message to show player
}
