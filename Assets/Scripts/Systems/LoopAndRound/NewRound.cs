using UnityEngine;

public class NewRound : MonoBehaviour
{
  public  Buffs buffs;
   public BoardManager board;
   public CurrencyManager currency;
    public TileMapManager tileMapManager;



        public void startNewRound()
    {
        RemoveAllBuffs();
        currency.newRound();
        BaseUnitNewRoundCalls();
    }
    public void RemoveAllBuffs()
    {
        int sizeX = board.unitGrid.GetLength(0);
        int sizeY = board.unitGrid.GetLength(1);

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                BaseUnit unit = board.unitGrid[x, y];
                if (unit == null) continue;

                for (int i = unit.activeBuffs.Count - 1; i >= 0; i--)
                {
                    buffs.RemoveTempBuff(unit, unit.activeBuffs[i]);
                }

                for (int i = unit.roundBuffs.Count - 1; i >= 0; i--)
                {
                    buffs.RemoveRoundBuff(unit, unit.roundBuffs[i]);
                }
            }
        }
    }

    public void BaseUnitNewRoundCalls()
    {
        int sizeX = board.unitGrid.GetLength(0);
        int sizeY = board.unitGrid.GetLength(1);

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                BaseUnit unit = board.unitGrid[x, y];
                if (unit == null) continue;
                unit.DestroyAllProjectiles();
                unit.ResetHealth();
                unit.ResetMana();
            }
        }
    }



}
