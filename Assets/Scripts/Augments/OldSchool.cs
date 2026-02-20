using UnityEngine;

public class OldSchool : MonoBehaviour
{
public BoardManager board;

public ModifyUnitStats math;
public TileMapManager tileMapManager;
public Buffs buffs;
public void OldSchoolCall()
{
    BaseUnit toCheck;

    // ✅ Always use the actual array size you're indexing
    int w = board.unitGrid.GetLength(0);
    int h = board.unitGrid.GetLength(1);

    for (int x = 0; x < w; x++)
    {
        for (int y = 0; y < h; y++)
        {
            int buffAmount = 0;

            BaseUnit unit = board.unitGrid[x, y];
            if (unit == null) continue;

            bool isOldSchool =
                unit.myData.Faction == "Prehistoric" ||
                unit.myData.Faction == "Medieval" ||
                unit.myData.Faction == "Mystic";

            if (!isOldSchool) continue;

            // left
            if (x > 0)
            {
                toCheck = board.unitGrid[x - 1, y];
                if (toCheck != null && (
                    toCheck.myData.Faction == "Prehistoric" ||
                    toCheck.myData.Faction == "Medieval" ||
                    toCheck.myData.Faction == "Mystic"))
                {
                    buffAmount += 30;
                }
            }

            // down
            if (y > 0)
            {
                toCheck = board.unitGrid[x, y - 1];
                if (toCheck != null && (
                    toCheck.myData.Faction == "Prehistoric" ||
                    toCheck.myData.Faction == "Medieval" ||
                    toCheck.myData.Faction == "Mystic"))
                {
                    buffAmount += 30;
                }
            }

            // up
            if (y < h - 1)
            {
                toCheck = board.unitGrid[x, y + 1];
                if (toCheck != null && (
                    toCheck.myData.Faction == "Prehistoric" ||
                    toCheck.myData.Faction == "Medieval" ||
                    toCheck.myData.Faction == "Mystic"))
                {
                    buffAmount += 30;
                }
            }

            // right
            if (x < w - 1)
            {
                toCheck = board.unitGrid[x + 1, y];
                if (toCheck != null && (
                    toCheck.myData.Faction == "Prehistoric" ||
                    toCheck.myData.Faction == "Medieval" ||
                    toCheck.myData.Faction == "Mystic"))
                {
                    buffAmount += 30;
                }
            }

            buffs.AddRoundBuff(unit, 0, 0, 0, 0, buffAmount, 0, null);
        }
    }
}

}