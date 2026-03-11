using UnityEngine;

public class TimeSpiral : MonoBehaviour
{
    public BoardManager board;
    public Buffs buffs;

    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private float attackDamagePerTick = 2f;
    [SerializeField] private float abilityPowerPerTick = 2f;

    private bool m_isActive;
    private float m_timer;

    public void TimeSpiralCall()
    {
        ResolveReferences();

        if (board == null || board.unitGrid == null || buffs == null)
        {
            Debug.LogError("TimeSpiral: Missing required references (board/unitGrid or buffs).");
            return;
        }

        m_isActive = true;
        m_timer = 0f;
    }

    private void Update()
    {
        if (!m_isActive)
        {
            return;
        }

        GameLoopManager loop = GameLoopManager.Instance;
        if (loop == null || loop.CurrentState != GameLoopManager.GameState.Combat)
        {
            m_timer = 0f;
            return;
        }

        m_timer += Time.deltaTime;

        while (m_timer >= tickInterval)
        {
            m_timer -= tickInterval;
            ApplyTick();
        }
    }

    private void ApplyTick()
    {
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
                BaseUnit unit = board.unitGrid[x, y];
                if (unit == null)
                {
                    continue;
                }

                buffs.AddRoundBuff(
                    unit,
                    attackSpeedMult: 0f, attackSpeedFlat: 0f,
                    attackDamageFlat: attackDamagePerTick, attackDamageMult: 0f,
                    abilityPowerFlat: abilityPowerPerTick, abilityPowerMult: 0f,
                    rangeBuff: 0f,
                    OnHit: null, onHitModifier: 0f,
                    OnKill: null, onKillModifier: 0f,
                    calledFromAugment: true,
                    refreshOnPlacement: false
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
