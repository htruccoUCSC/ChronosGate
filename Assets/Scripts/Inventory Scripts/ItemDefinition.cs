using UnityEngine;

[CreateAssetMenu(fileName = "ItemDefinition", menuName = "ChronosGate/Items/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Header("Item Info")]
    [SerializeField] private string m_DisplayName;
    [SerializeField] private string m_Description;
    [SerializeField] private string m_PrefabPath;

    [Header("Item Stats")]
    [SerializeField] private int m_DamageValue;
    // using same icon caching logic as UnitDefinition 
    private Sprite m_CachedIcon;

    public string DisplayName => m_DisplayName;
    public string Description => m_Description;
    public int DamageValue => m_DamageValue;
    public string PrefabPath => m_PrefabPath;
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
