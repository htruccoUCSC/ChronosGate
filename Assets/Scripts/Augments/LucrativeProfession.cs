using UnityEngine;


public class LucrativeProfession : MonoBehaviour
{
public BoardManager board;

public ModifyUnitStats math;
public TileMapManager tileMapManager;
public Buffs buffs;
public void LucrativeProfessionCall()
{
    for (int x = 0; x < tileMapManager.Width; x++)
    {
        for (int y = 0; y < tileMapManager.Height; y++)
        {
            BaseUnit unit = board.unitGrid[x, y];
            if (unit == null) continue;
            if (unit.myData.Name == "Bounty Hunter")
            {
                buffs.AddRoundBuff(unit, attackSpeedMult: 0f, attackSpeedFlat: 0f, attackDamageFlat: 0f, attackDamageMult: 0f, abilityPowerFlat: 0f, abilityPowerMult: 0f, OnHit: null, onHitModifier: 0f, OnKill: null, onKillModifier: 0f);
            }
        }
       
    }
}

}
