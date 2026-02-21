using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DatabaseLoader))]
public class DatabaseLoaderInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DatabaseLoader loader = (DatabaseLoader)target;

        GUILayout.Space(6);
        if (GUILayout.Button("Reload Database"))
        {
            loader.LoadData();
            Debug.Log("[DatabaseLoaderInspector] Reloaded Unit DB via inspector button.");
        }
    }
}
