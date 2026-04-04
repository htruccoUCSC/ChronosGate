using System;
using System.Collections.Generic;
using UnityEngine;

public enum BasicAttackType
{
    None,
    Melee,
    Projectile
}

public enum UnitRarity
{
    Common,
    Rare,
    Epic
}

[CreateAssetMenu(fileName = "NewUnitDef", menuName = "Game/Unit Definition")]
public class UnitDefinition : ScriptableObject
{
    [Header("Identity")]
    public string UnitID;
    public string Name;
    public string Description;
    public string Faction;
    public string PrefabPath;
    // Falls back to Common if the spreadsheet leaves rarity blank or invalid.
    public UnitRarity Rarity = UnitRarity.Common;

    // Icon logic for inventory and UI
    // I think this is better than just constantly accessing the prefab each time we need the icon
    private Sprite _cachedIcon;

    public Sprite Icon
    {
        get
        {
            if (_cachedIcon != null) return _cachedIcon;

            // check if path exists
            if (string.IsNullOrEmpty(PrefabPath))
            {
                Debug.LogError($"[UnitDef] PrefabPath is empty for {Name}!");
                return null;
            }

            GameObject prefab = Resources.Load<GameObject>(PrefabPath);

            // check if Prefab loaded
            if (prefab == null)
            {
                Debug.LogError($"[UnitDef] Could not load Prefab at path: '{PrefabPath}' for {Name}. Check spelling or Resources folder!");
                return null;
            }

            // check for Renderer
            SpriteRenderer sr = prefab.GetComponentInChildren<SpriteRenderer>(true);
            if (sr != null)
            {
                _cachedIcon = sr.sprite;
            }
            else
            {
                Debug.LogError($"[UnitDef] Prefab '{prefab.name}' loaded, but has NO SpriteRenderer on it or its children!");
            }

            return _cachedIcon;
        }
    }

    [Header("Combat Behavior")]
    public string AttackType;
    public BasicAttackType AttackFunction;
    public float LaunchAngle;

    [Header("Generic Stats")]
    public int Cost;
    public float Range;
    public int Health;

    [Header("Basic Attack Stats")]
    public float AttackSpeed;
    public float AttackDamage;

    [Header("Basic Ability Stats")]
    public float AbilityPower;
    public float AbilityManaCost;
    public float StartingMana;
    public float ManaPerShot;
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
    public string Description;
    public string PrefabPath;
    public string Faction;
    public string Rarity;

    public int Cost;
    public int Health;
    public float Range;
    public float AttackSpeed;
    public float AttackDamage;
    public float AbilityPower;
    public float AbilityManaCost;

    public float StartingMana;
    public float ManaPerShot;
    // this is an exception to the naming convention since it's an enum in UnitDefinition
    public string AttackType;
    public float LaunchAngle;
}
