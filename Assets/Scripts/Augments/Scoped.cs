using UnityEngine;

public class Scoped : MonoBehaviour
{
    public BoardManager board;
    public Buffs buffs;

    [SerializeField] private bool m_Debug;
    [SerializeField] private bool m_DebugPerUnit;
    [SerializeField] private int m_MaxPerUnitLogs = 12;

    public void ScopedCall()
    {
        ResolveReferences();

        if (board == null || board.unitGrid == null || buffs == null)
        {
            Debug.LogError("Scoped: Missing required references (board/unitGrid or buffs).");
            return;
        }

        int width = board.unitGrid.GetLength(0);
        int height = board.unitGrid.GetLength(1);

        int buffedCount = 0;
        int meleeCount = 0;
        int emptyCount = 0;
        int perUnitLogs = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                BaseUnit unit = board.unitGrid[x, y];
                if (unit == null)
                {
                    emptyCount++;
                    continue;
                }

                if (unit.TryGetComponent(out MeleeAttackBehavior _))
                {
                    meleeCount++;
                    continue;
                }

                float rangeBefore = unit.myData != null ? unit.myData.GetModifiedRange() : 0f;

                buffs.AddRoundBuff(
                    unit,
                    attackSpeedMult: 0f, attackSpeedFlat: 0f,
                    attackDamageFlat: 0f, attackDamageMult: 0f,
                    abilityPowerFlat: 0f, abilityPowerMult: 0f,
                    rangeBuff: 1f,
                    OnHit: null, onHitModifier: 0f,
                    OnKill: null, onKillModifier: 0f,
                    calledFromAugment: true,
                    refreshOnPlacement: true
                );

                buffedCount++;

                if (m_DebugPerUnit && perUnitLogs < m_MaxPerUnitLogs)
                {
                    float rangeAfter = unit.myData != null ? unit.myData.GetModifiedRange() : 0f;
                    Debug.Log($"[Scoped] Buffed {unit.name} range {rangeBefore:F1} -> {rangeAfter:F1}");
                    perUnitLogs++;
                }
            }
        }

        if (m_Debug)
        {
            Debug.Log($"[Scoped] Applied to {buffedCount} units. Melee skipped: {meleeCount}. Empty: {emptyCount}.");
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
