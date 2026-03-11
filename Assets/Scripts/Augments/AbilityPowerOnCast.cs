using UnityEngine;

public class AbilityPowerOnCast : MonoBehaviour
{
    public BoardManager board;
    public Buffs buffs;

    private bool m_isSubscribed;

    public void AbilityPowerOnCastCall()
    {
        ResolveReferences();

        if (board == null || board.unitGrid == null || buffs == null)
        {
            Debug.LogError("AbilityPowerOnCast: Missing required references (board/unitGrid or buffs).");
            return;
        }

        if (m_isSubscribed)
        {
            return;
        }

        BaseUnit.AbilityUsed += HandleAbilityUsed;
        m_isSubscribed = true;
    }

    private void OnDisable()
    {
        if (!m_isSubscribed)
        {
            return;
        }

        BaseUnit.AbilityUsed -= HandleAbilityUsed;
        m_isSubscribed = false;
    }

    private void HandleAbilityUsed(BaseUnit unit)
    {
        GameLoopManager loop = GameLoopManager.Instance;
        if (loop == null || loop.CurrentState != GameLoopManager.GameState.Combat)
        {
            return;
        }

        if (board == null || board.unitGrid == null || buffs == null)
        {
            ResolveReferences();
            if (board == null || board.unitGrid == null || buffs == null)
            {
                return;
            }
        }

        int width = board.unitGrid.GetLength(0);
        int height = board.unitGrid.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                BaseUnit target = board.unitGrid[x, y];
                if (target == null)
                {
                    continue;
                }

                buffs.AddRoundBuff(
                    target,
                    attackSpeedMult: 0f, attackSpeedFlat: 0f,
                    attackDamageFlat: 0f, attackDamageMult: 0f,
                    abilityPowerFlat: 1f, abilityPowerMult: 0f,
                    rangeBuff: 0f,
                    OnHit: null, onHitModifier: 0f,
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
