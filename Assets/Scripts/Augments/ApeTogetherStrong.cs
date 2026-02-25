using UnityEngine;

public class ApeTogetherStrong : MonoBehaviour
{
public BoardManager board;

public ModifyUnitStats math;
public TileMapManager tileMapManager;
public Buffs buffs;
public void ApeTogetherStrongCall()
{
    for (int x = 0; x < tileMapManager.Width; x++)
    {
        int buffAmount = 0;
        for (int y = 0; y < tileMapManager.Height; y++)
        {
            BaseUnit unit = board.unitGrid[x, y];
            if (unit == null) continue;
            if (unit.myData.Faction == "Prehistoric")
            {
                buffAmount += 20;
            }
        }
        for (int y = 0; y < tileMapManager.Height; y++)
        {
            BaseUnit unit = board.unitGrid[x, y];
            if (unit == null) continue;
            if (unit.myData.Faction == "Prehistoric")
            {
                buffs.AddRoundBuff(unit, attackSpeedMult: 0f, attackSpeedFlat: 0f, attackDamageFlat: buffAmount, attackDamageMult: 0f, abilityPowerFlat: 0f, abilityPowerMult: 0f, OnHit: null, onHitModifier: 0f, OnKill: null, onKillModifier: 0f);
            }
        }
    }
}

}
