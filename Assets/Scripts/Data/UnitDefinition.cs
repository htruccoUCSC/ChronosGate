using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitDef", menuName = "Game/Unit Definition")]
public class UnitDefinition : ScriptableObject
{
    [Header("Identity")]
    public string UnitID;
    public string Name;
    public string PrefabPath;

    [Header("Generic Stats")]
    public int Cost;
    public float Range;

    [Header("Basic Attack Stats")]
    public float AttackSpeed;
    public float AttackDamage;

    [Header("Basic Ability Stats")]
    public float AbilityPower;
    public float MaxMana;
    public float StartingMana;
}

[Serializable]
public class UnitDatabase
{
    // This MUST match the top-level key in the JSON: "AllUnits"
    public List<UnitRawData> AllUnits;
}


// we need both this and UnitDefinition because one is for Unity's ScriptableObject system
// and the other is for newtonsoft to import the json data
// VARIABLE NAMES HAVE TO BE THE SAME AS IN UnitDefinition AND IN THE SPREADSHEET HEADER
[Serializable]
public class UnitRawData
{
    public string UnitID;
    public string Name;
    public string PrefabPath;
    public int Cost;
    public float Range;
    public float AttackSpeed;
    public float AttackDamage;
    public float AbilityPower;
    public float MaxMana;
    public float StartingMana;
}