using UnityEngine;

public class QuickStart : MonoBehaviour
{
    public BoardManager board;

    public void QuickStartCall()
    {
        ResolveReferences();

        if (board == null || board.unitGrid == null)
        {
            Debug.LogError("QuickStart: Missing required references (board/unitGrid).");
            return;
        }

        int width = board.unitGrid.GetLength(0);
        int height = board.unitGrid.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                BaseUnit unit = board.unitGrid[x, y];
                if (unit == null || unit.myData == null || unit.myData.BaseDef == null)
                {
                    continue;
                }

                float manaCost = unit.myData.BaseDef.AbilityManaCost;
                if (manaCost <= 0f)
                {
                    continue;
                }

                unit.myData.CurrentMana = manaCost;
            }
        }
    }

    private void ResolveReferences()
    {
        if (board == null)
        {
            board = FindFirstObjectByType<BoardManager>();
        }
    }
}
