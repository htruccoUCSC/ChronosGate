using System.Collections.Generic;
using UnityEngine;

public static class UnitDragPreviewUtility
{
    private static readonly Dictionary<string, BaseUnit> s_PreviewProviders = new Dictionary<string, BaseUnit>();

    public static BaseUnit GetPreviewProvider(UnitDefinition def)
    {
        if (def == null || string.IsNullOrWhiteSpace(def.PrefabPath))
        {
            return null;
        }

        if (s_PreviewProviders.TryGetValue(def.PrefabPath, out BaseUnit cached) && cached != null)
        {
            return cached;
        }

        GameObject prefab = Resources.Load<GameObject>(def.PrefabPath);
        if (prefab == null)
        {
            return null;
        }

        BaseUnit unit = prefab.GetComponent<BaseUnit>();
        if (unit == null)
        {
            return null;
        }

        s_PreviewProviders[def.PrefabPath] = unit;
        return unit;
    }
}
