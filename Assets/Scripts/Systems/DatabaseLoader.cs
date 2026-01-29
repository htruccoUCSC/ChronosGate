using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

public class DatabaseLoader : MonoBehaviour
{
    public string fileName = "units.json";

    // This is where we will store the units for easy access
    public Dictionary<string, UnitData> UnitLookup = new Dictionary<string, UnitData>();

    void Start()
    {
        LoadData();
    }

    public void LoadData()
    {
        // 1. Find the file path
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (File.Exists(filePath))
        {
            // 2. Read the raw text from the file
            string jsonText = File.ReadAllText(filePath);

            // 3. The Magic: Convert text into C# objects
            // We wrap it in our UnitDatabase class
            UnitDatabase db = JsonConvert.DeserializeObject<UnitDatabase>(jsonText);

            // 4. Organize it into a Dictionary for fast searching
            UnitLookup.Clear();
            foreach (var unit in db.AllUnits)
            {
                UnitLookup.Add(unit.UnitID, unit);
                Debug.Log($"Imported: {unit.Name}");
            }
        }
        else
        {
            Debug.LogError("Cannot find JSON file at " + filePath);
        }
    }
}