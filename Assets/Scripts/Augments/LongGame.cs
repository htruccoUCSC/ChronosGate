using Unity.VisualScripting;
using UnityEngine;

public class LongGame : MonoBehaviour
{
public BoardManager board;

public ModifyUnitStats math;
public TileMapManager tileMapManager;
public Buffs buffs;
public float startingAmount;
public void LongGameCall()
{
startingAmount++;
   float buffAmount = startingAmount*0.1f;
for (int x = 0; x < tileMapManager.Width; x++)
{
   for (int y = 0; y < tileMapManager.Height; y++)
   {
      BaseUnit unit = board.unitGrid[x, y];
      if (unit == null) continue;
      if (unit.myData.Faction == "Future")
      {
         buffs.AddRoundBuff(unit, attackSpeedMult: 0f, attackSpeedFlat: 0f, attackDamageFlat: 0f, attackDamageMult: 0f, abilityPowerFlat: 50f, abilityPowerMult: buffAmount, OnHit: null, onHitModifier: 0f);
      }
      else
      {
         buffs.AddRoundBuff(unit, attackSpeedMult: 0f, attackSpeedFlat: 0f, attackDamageFlat: 0f, attackDamageMult: 0f, abilityPowerFlat: 0f, abilityPowerMult: buffAmount, OnHit: null, onHitModifier: 0f);
      }
   }
}
}

}
