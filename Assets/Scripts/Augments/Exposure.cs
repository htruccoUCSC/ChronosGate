using UnityEngine;

public class Exposure : MonoBehaviour
{
    // LuckyShotPerformAutoAttack is inside of UnitInstance becuase it needs to call Attack
public BoardManager board;
public ModifyUnitStats math;

public Buffs buffs;
public void ExposureCall()
{
for (int x = 0; x < board.unitGrid.GetLength(0); x++)
{

    for (int y = 0; y < board.unitGrid.GetLength(1); y++)
    {
        BaseUnit unit = board.unitGrid[x, y];
        if (unit == null) continue;

        buffs.AddRoundBuff(unit,0,0,0,0,0,0,0f,unit.ApplyAmp, 0.05f, null, 0f, calledFromAugment: true, refreshOnPlacement: true);

    }

}
}

}
