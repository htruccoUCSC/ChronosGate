using UnityEngine;

public class LuckyShot : MonoBehaviour
{
public BoardManager board;
public ModifyUnitStats math;

public Buffs buffs;
public void LuckyShotCall()
{
for (int x = 0; x < board.unitGrid.GetLength(0); x++)
{

    for (int y = 0; y < board.unitGrid.GetLength(1); y++)
    {
        BaseUnit unit = board.unitGrid[x, y];
        if (unit == null) continue;

        buffs.AddRoundBuff(unit,0,0,0,0,unit.LuckyShotPerformAutoAttack);

    }

}
}

}
