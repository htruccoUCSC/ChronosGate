using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class DatabaseLoader : MonoBehaviour
{
    public string fileName = "units.json";

    // where units definitions are stored after loading
    public Dictionary<string, UnitDefinition> UnitLookup = new Dictionary<string, UnitDefinition>();
    public bool IsLoaded { get; private set; }

    void Awake()
    {
        LoadData();
    }

    public void LoadData()
    {
        IsLoaded = false;
        StartCoroutine(LoadDataCoroutine());
    }

    private IEnumerator LoadDataCoroutine()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

#if UNITY_WEBGL && !UNITY_EDITOR
        using (var request = UnityWebRequest.Get(filePath))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Cannot load JSON file at {filePath}: {request.error}");
                IsLoaded = true;
                yield break;
            }

            ParseJson(request.downloadHandler.text);
        }
#else
        if (File.Exists(filePath))
        {
            string jsonText = File.ReadAllText(filePath);
            ParseJson(jsonText);
        }
        else
        {
            Debug.LogError("Cannot find JSON file at " + filePath);
        }
#endif

        IsLoaded = true;
        yield return null;
    }

    private void ParseJson(string jsonText)
    {
        UnitDatabase dataBatch = JsonConvert.DeserializeObject<UnitDatabase>(jsonText);
        if (dataBatch == null || dataBatch.AllUnits == null)
        {
            Debug.LogError("[DatabaseLoader] Failed to parse unit database or AllUnits was null.");
            return;
        }

        UnitLookup.Clear();

        foreach (var unit in dataBatch.AllUnits)
        {
            if (unit == null)
            {
                continue;
            }

            // creates unity scriptable object instance
            UnitDefinition unitDef = ScriptableObject.CreateInstance<UnitDefinition>();

            // c sharp reflection (some bullshit) to copy fields from raw data to scriptable object
            // this is done to write less repetitive code assigning each field one by one
            var rawFields = typeof(UnitRawData).GetFields();
            var defType = typeof(UnitDefinition);

            foreach (var field in rawFields)
            { 
                if (field.Name == nameof(UnitRawData.Rarity))
                {
                    continue;
                }

                var targetField = defType.GetField(field.Name);

                if (targetField != null)
                {
                    targetField.SetValue(unitDef, field.GetValue(unit));
                }

            }

            // parse attack type string to enum
            if (Enum.TryParse(unit.AttackType, out BasicAttackType type))
            {
                unitDef.AttackFunction = type;
            }
            else
            {
                // if parsing fails, set to none
                unitDef.AttackFunction = BasicAttackType.None;
            }

            // The spreadsheet exports rarity as text, so we clean it up once when units.json is loaded.
            if (string.IsNullOrWhiteSpace(unit.Rarity))
            {
                unitDef.Rarity = UnitRarity.Common;
            }
            else if (Enum.TryParse(unit.Rarity, true, out UnitRarity rarity))
            {
                unitDef.Rarity = rarity;
            }
            else
            {
                Debug.LogWarning($"[DatabaseLoader] Invalid rarity '{unit.Rarity}' for unit '{unit.UnitID}'. Defaulting to Common.");
                unitDef.Rarity = UnitRarity.Common;
            }

            if (string.IsNullOrWhiteSpace(unitDef.UnitID))
            {
                Debug.LogWarning("[DatabaseLoader] Skipped unit with empty UnitID.");
                continue;
            }

            if (UnitLookup.ContainsKey(unitDef.UnitID))
            {
                Debug.LogWarning($"[DatabaseLoader] Duplicate UnitID '{unitDef.UnitID}' found. Overwriting previous entry.");
            }

            UnitLookup[unitDef.UnitID] = unitDef;
        }
    }
}
