using UnityEngine;

public class Renovations : MonoBehaviour
{
    private const int MinSafeBoardHeight = 3;

    public BoardManager board;
    public TileMapManager tileMap;

    public bool CanApplySafely()
    {
        if (tileMap == null)
        {
            tileMap = FindFirstObjectByType<TileMapManager>();
        }

        if (tileMap == null)
        {
            Debug.LogWarning("[Renovations] TileMapManager not found. Blocking augment.");
            return false;
        }

        return tileMap.Height - 2 >= MinSafeBoardHeight;
    }

    public void RenovationsCall()
    {
        if (tileMap == null)
        {
            tileMap = FindFirstObjectByType<TileMapManager>();
        }

        if (!CanApplySafely())
        {
            Debug.LogWarning($"[Renovations] Blocked. Current height {tileMap?.Height ?? -1} would shrink below the safe minimum of {MinSafeBoardHeight}.");
            return;
        }

        tileMap.SetHeight(tileMap.Height - 2);
        tileMap.SetWidth(tileMap.Width + 2);
    }
}
