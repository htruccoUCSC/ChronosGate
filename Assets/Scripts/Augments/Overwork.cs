using UnityEngine;

public class Overwork : MonoBehaviour
{
    public BoardManager board;
    public Buffs buffs;

    public void OverworkCall()
    {
        ResolveReferences();

        if (board == null || board.unitGrid == null || buffs == null)
        {
            Debug.LogError("Overwork: Missing required references (board/unitGrid or buffs).");
            return;
        }

        int width = board.unitGrid.GetLength(0);
        int height = board.unitGrid.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                BaseUnit unit = board.unitGrid[x, y];
                if (unit == null)
                {
                    continue;
                }

                buffs.AddRoundBuff(
                    unit,
                    attackSpeedMult: 0f, attackSpeedFlat: 0f,
                    attackDamageFlat: 0f, attackDamageMult: 2f,
                    abilityPowerFlat: 0f, abilityPowerMult: 0f,
                    rangeBuff: 0f,
                    OnHit: _ => unit.TakeDamage(1), onHitModifier: 0f,
                    OnKill: null, onKillModifier: 0f,
                    calledFromAugment: true,
                    refreshOnPlacement: true
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
