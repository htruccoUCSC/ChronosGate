using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

public class DatabaseLoader : MonoBehaviour
{
    public string fileName = "units.json";

    // where units definitions are stored after loading
    public Dictionary<string, UnitDefinition> UnitLookup = new Dictionary<string, UnitDefinition>();

    void Start()
    {
        LoadData();
    }

    public void LoadData()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (File.Exists(filePath))
        {
            string jsonText = File.ReadAllText(filePath);

            UnitDatabase dataBatch = JsonConvert.DeserializeObject<UnitDatabase>(jsonText);

            UnitLookup.Clear();

            foreach (var unit in dataBatch.AllUnits)
            {
                // creates unity scriptable object instance
                UnitDefinition unitDef = ScriptableObject.CreateInstance<UnitDefinition>();

                // c sharp reflection (some bullshit) to copy fields from raw data to scriptable object
                // this is done to write less repetitive code assigning each field one by one
                var rawFields = typeof(UnitRawData).GetFields();
                var defType = typeof(UnitDefinition);

                foreach (var field in rawFields)
                { 
                    var targetField = defType.GetField(field.Name);

                    if (targetField != null)
                    {
                        targetField.SetValue(unitDef, field.GetValue(unit));
                    }

                }
                
                UnitLookup.Add(unitDef.UnitID, unitDef);
            }


        }
        else
        {
            Debug.LogError("Cannot find JSON file at " + filePath);
        }
    }
}