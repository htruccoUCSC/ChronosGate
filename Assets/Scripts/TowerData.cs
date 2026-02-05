using UnityEngine;

[CreateAssetMenu(fileName = "New Tower", menuName = "Shop/Tower Data")]
public class TowerData : ScriptableObject
{
    public string towerName;
    public string era; // e.g., "Medieval", "Modern", "Future"
    public string type; 
    public Sprite icon;
    public int cost;
    [TextArea(3, 5)]
    public string description;
    
    // Add actual tower prefab or stats as needed
    // public GameObject towerPrefab;
    // public float damage;
    // public float attackSpeed;
    // public float range;
}