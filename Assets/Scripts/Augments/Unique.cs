using UnityEngine;

public class Unique : MonoBehaviour
{
public BoardManager board;

public ModifyUnitStats math;
public TileMapManager tileMapManager;
public Buffs buffs;
public void UniqueCall()
{
    BaseUnit toCheck;
    for (int x = 0; x < tileMapManager.Width; x++)
    {
        for (int y = 0; y < tileMapManager.Height; y++)
        {
            BaseUnit unit = board.unitGrid[x, y];
            if (unit == null) continue;

            string faction = unit.myData.Faction;
            if (x >= 1)
            {
                toCheck = board.unitGrid[x - 1, y];
                if (toCheck != null && toCheck.myData.Faction == faction)
                {
                    break;
                }
            }
            if (y >= 1)
            {
                toCheck = board.unitGrid[x, y - 1];
                if (toCheck != null && toCheck.myData.Faction == faction)
                {
                    break;
                }
            }
            if (y < board.Height - 1)
            {
                toCheck = board.unitGrid[x, y + 1];
                if (toCheck != null && toCheck.myData.Faction == faction)
                {
                    break;
                }
            }
            if (x < board.Width - 1)
            {
                toCheck = board.unitGrid[x + 1, y];
                if (toCheck != null && toCheck.myData.Faction == faction)
                {
                    break;
                }
            }

            // Use named args to avoid accidental positional-mapping mistakes
            buffs.AddRoundBuff(
                unit,
                attackSpeedMult: 0f,
                attackSpeedFlat: 0f,
                attackDamageFlat: 3f,
                attackDamageMult: 0f,
                abilityPowerFlat: 50f,
                abilityPowerMult: 0f,
                rangeBuff: 0f,
                OnHit: null,
                onHitModifier: 0f,
                OnKill: null,
                onKillModifier: 0f,
                calledFromAugment: true,
                refreshOnPlacement: true);
        }
    }
}

}


