using System.Collections.Generic;
using UnityEngine;

public class Diversity : MonoBehaviour
{
    public BoardManager board;
    public Buffs buffs;

    public void DiversityCall()
    {
        ResolveReferences();

        if (board == null || board.unitGrid == null || buffs == null)
        {
            Debug.LogError("Diversity: Missing required references (board/unitGrid or buffs).");
            return;
        }

        int width = board.unitGrid.GetLength(0);
        int height = board.unitGrid.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            HashSet<string> factions = new HashSet<string>();

            for (int x = 0; x < width; x++)
            {
                BaseUnit unit = board.unitGrid[x, y];
                if (unit == null || unit.myData == null)
                {
                    continue;
                }

                string faction = unit.myData.Faction;
                if (!string.IsNullOrWhiteSpace(faction))
                {
                    factions.Add(faction);
                }
            }

            if (factions.Count < 3)
            {
                continue;
            }

            for (int x = 0; x < width; x++)
            {
                BaseUnit unit = board.unitGrid[x, y];
                if (unit == null)
                {
                    continue;
                }

                buffs.AddRoundBuff(
                    unit,
                    attackSpeedMult: 0.1f, attackSpeedFlat: 0f,
                    attackDamageFlat: 0f, attackDamageMult: 0f,
                    abilityPowerFlat: 0f, abilityPowerMult: 0f,
                    rangeBuff: 0f,
                    OnHit: null, onHitModifier: 0f,
                    OnKill: null, onKillModifier: 0f
                );
            }
        }
    }

    private void ResolveReferences()
    {
        if (board == null)
        {
            board = FindFirstObjectByType<BoardManager>();
        }

        if (buffs == null)
        {
            buffs = FindFirstObjectByType<Buffs>();
        }
    }
}
