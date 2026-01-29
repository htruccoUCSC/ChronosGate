using System;
using System.Collections.Generic;

[Serializable] // This allows the data to show up in the Unity Inspector
public class UnitData
{
    public string UnitID;
    public string Name;
    public string PrefabPath;

    public int Cost;
    public float Range;       // How far down the lane they see

    public float AttackSpeed; // Attacks per second
    public float AttackDamage;

    public float AbilityPower; // Ability Value
    public float MaxMana;
    public float StartingMana;
}

// This wrapper class is necessary because JSON from a spreadsheet 
// is usually a list of objects, not just one object.
[Serializable]
public class UnitDatabase
{
    public List<UnitData> AllUnits;
}