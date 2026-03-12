using UnityEngine;

public class ReserveAS : MonoBehaviour
{
public BoardManager board;
public ModifyUnitStats math;
public Buffs buffs;
public void ReserveASCall()
{
        int buffAmount = CurrencyManager.Instance.GetCurrency() / 5;
for (int x = 0; x < board.unitGrid.GetLength(0); x++)
{

    for (int y = 0; y < board.unitGrid.GetLength(1); y++)
    {
        BaseUnit unit = board.unitGrid[x, y];
        if (unit == null) continue;

        buffs.AddRoundBuff(unit,0,buffAmount,0,0,0,0,0f,null, 0f, null, 0f);
    }

}
}

}
