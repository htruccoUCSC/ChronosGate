using UnityEngine;

public class ReserveAD : MonoBehaviour
{
public BoardManager board;
public ModifyUnitStats math;
public CurrencyManager currency;
public Buffs buffs;
public void ReserveADCall()
{
        int buffAmount = currency.GetCurrency() / 5;
for (int x = 0; x < board.unitGrid.GetLength(0); x++)
{

    for (int y = 0; y < board.unitGrid.GetLength(1); y++)
    {
        BaseUnit unit = board.unitGrid[x, y];
        if (unit == null) continue;

        buffs.AddRoundBuff(unit,buffAmount,0,0,0);
    }

}
}

}
