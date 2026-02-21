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
        for (int x = 0; x <tileMapManager.Height; x++){
    for (int y = 0; y <tileMapManager.Width; y++){
        BaseUnit unit = board.unitGrid[x, y]; 
         if (unit == null) continue;

         foreach (Buff buff in unit.activeBuffs.ToArray())
         {
             buffs.RemoveTempBuff(unit, buff);
         }
         foreach (Buff buff in unit.roundBuffs.ToArray())
         {
             buffs.RemoveRoundBuff(unit, buff);
         }
    }
}
    }

    public void BaseUnitNewRoundCalls()
    {
        for (int x = 0; x <tileMapManager.Height; x++){
    for (int y = 0; y <tileMapManager.Width; y++){
        BaseUnit unit = board.unitGrid[x, y]; 
         if (unit == null) continue;
        unit.DestroyAllProjectiles();
        unit.ResetHealth();
        unit.ResetMana();
       
    }
}
    }



}
