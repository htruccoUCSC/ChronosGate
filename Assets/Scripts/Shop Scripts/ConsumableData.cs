using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Shop/Consumable Data")]
public class ConsumableData : ScriptableObject
{
    public string consumableName;
    public Sprite icon;
    public int cost;
    [TextArea(2, 4)]
    public string description;
    
    // Add consumable effect data as needed
    // public enum ConsumableType { Heal, Buff, Debuff, Resource }
    // public ConsumableType type;
    // public float effectValue;
}