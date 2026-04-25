using UnityEngine;

public enum ItemEffectType
{
    AreaDamage = 0,
    TowerLevelUp = 1,
    None = 2,
    TowerPoolRestock = 3
}

[CreateAssetMenu(fileName = "ItemDefinition", menuName = "ChronosGate/Items/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Header("Item Info")]
    [SerializeField] private string m_DisplayName;
    [SerializeField] private string m_Description;
    [SerializeField] private string m_PrefabPath;

    [Header("Item Stats")]
    [SerializeField] private ItemEffectType m_EffectType = ItemEffectType.AreaDamage;
    [SerializeField] private int m_AreaSizeInTiles = 3;
    [SerializeField] private int m_DamageValue;
    [SerializeField] private int m_cost;
    // using same icon caching logic as UnitDefinition 
    private Sprite m_CachedIcon;

    public string DisplayName => m_DisplayName;
    public string Description => m_Description;
    public ItemEffectType EffectType => m_EffectType;
    public int AreaSizeInTiles => m_AreaSizeInTiles > 0 ? m_AreaSizeInTiles : 3;
    public int DamageValue => m_DamageValue;
    public string PrefabPath => m_PrefabPath;
    public int Cost => m_cost;
    // Icon logic
    public Sprite Icon
    { 
        get
        {
            if (m_CachedIcon != null) return m_CachedIcon;

            if (string.IsNullOrEmpty(m_PrefabPath)){return null;}
            // load prefab
            GameObject prefab = Resources.Load<GameObject>(m_PrefabPath);

            if (prefab == null) {return null;}
            
            SpriteRenderer sr = prefab.GetComponentInChildren<SpriteRenderer>(true);
            if (sr != null)
            {
                m_CachedIcon = sr.sprite;
                return m_CachedIcon;
            }

            return null;
        }
    }
}
