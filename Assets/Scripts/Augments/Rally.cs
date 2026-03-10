using UnityEngine;

public class Rally : MonoBehaviour
{
public BoardManager board;

public ModifyUnitStats math;
public TileMapManager tileMapManager;
public Buffs buffs;
public void RallyCall()
{
    Debug.Log("RallyCall started.");

    for (int x = 0; x < tileMapManager.Width; x++)
    {
        for (int y = 0; y < tileMapManager.Height; y++)
        {
            BaseUnit unit = board.unitGrid[x, y];
            if (unit == null || unit.myData == null || unit.myData.BaseDef == null) continue;
            if (unit.AttackType != "Melee") continue;
             Debug.Log("RallyCall unit found");
            int buffAmount = 0;
            for (int row = x + 1; row < tileMapManager.Width; row++)
            {
                BaseUnit toCheck = board.unitGrid[row, y];
                if (toCheck == null || toCheck.myData == null || toCheck.myData.BaseDef == null || toCheck.AttackType != "Melee")
                {
                    break;
                }

                buffAmount += 1;
            }

            buffs.AddRoundBuff(unit, 0, 0, 0, 0, 0, 0, buffAmount, null, 0f, null, 0f);
            Debug.Log("Rally " + unit.myData.Name + " receives " + buffAmount + " range.");
        }
    }
}

}
