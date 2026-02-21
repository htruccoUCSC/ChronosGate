using UnityEditor;
using UnityEngine;

public static class DatabaseLoaderEditor
{
    [MenuItem("Tools/Reload Unit DB")]
    public static void ReloadUnitDB()
    {
        var loader = Object.FindObjectOfType<DatabaseLoader>();
        if (loader != null)
        {
            loader.LoadData();
            Debug.Log("[DatabaseLoaderEditor] Reloaded Unit DB via existing DatabaseLoader instance.");
            return;
        }

        // fallback: create temporary loader to run LoadData
        var go = new GameObject("TempDatabaseLoader");
        var tmp = go.AddComponent<DatabaseLoader>();
        tmp.LoadData();
        Object.DestroyImmediate(go);
        Debug.Log("[DatabaseLoaderEditor] Reloaded Unit DB via temporary DatabaseLoader.");
    }
}
