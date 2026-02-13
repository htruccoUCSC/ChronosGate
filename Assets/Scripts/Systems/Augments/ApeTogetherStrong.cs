using UnityEngine;

public class ApeTogetherStrong : MonoBehaviour
{
public BoardManager board;
public ModifyUnitStats math;

public Buffs buffs;
public void ApeTogetherStrongCall()
{
for (int x = 0; x < board.unitGrid.GetLength(0); x++)
{
    int buffAmount = 0;
    for (int y = 0; y < board.unitGrid.GetLength(1); y++)
    {
        BaseUnit unit = board.unitGrid[x, y];
        if (unit == null) continue;

        if (unit.myData.BaseDef.UnitID == "Archer")
        {
            buffAmount += 20;
        }
    }
      for (int y = 0; y < board.unitGrid.GetLength(1); y++)
    {
        BaseUnit unit = board.unitGrid[x, y];
        if (unit == null) continue;

        if (unit.myData.BaseDef.UnitID == "Archer")
        {
            buffs.AddRoundBuff(unit,0,0,buffAmount,0);
        }
    }
}
}

}
