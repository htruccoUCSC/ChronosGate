using UnityEngine;

public class Clastrophobic : MonoBehaviour
{
    public BoardManager board;
    public TileMapManager tileMapManager;
    public CurrencyManager currency;
    public Buffs buffs;
    public WaveManager round;

    public void ClastrophobicCall()
    {
        ResolveReferences();

        if (board == null || board.unitGrid == null || buffs == null || round == null)
        {
            Debug.LogError("Clastrophobic: Missing required references (board/unitGrid, buffs, or wave manager).");
            return;
        }

        int width = board.unitGrid.GetLength(0);
        int height = board.unitGrid.GetLength(1);

        float buffAmount = 0f;

        for (int x = 0; x < width; x++)
        {
            int emptyCount = 0;
            for (int y = 0; y < height; y++)
            {
                BaseUnit unit = board.unitGrid[x, y];
                if (unit == null)
                {
                    emptyCount++;
                }
            }

            if (emptyCount >= height)
            {
                buffAmount += round.currentWave;
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                BaseUnit unit = board.unitGrid[x, y];
                if (unit == null)
                {
                    continue;
                }

                Debug.Log("clastrophobic added + " + buffAmount);
                buffs.AddRoundBuff(
                    unit,
                    attackSpeedMult: 0f, attackSpeedFlat: buffAmount,
                    attackDamageFlat: buffAmount, attackDamageMult: 0f,
                    abilityPowerFlat: buffAmount, abilityPowerMult: 0f,
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

        if (tileMapManager == null && board != null)
        {
            tileMapManager = board.TileMapManager;
        }

        if (buffs == null)
        {
            buffs = FindFirstObjectByType<Buffs>();
        }

        if (round == null)
        {
            round = WaveManager.Instance;
            if (round == null)
            {
                round = FindFirstObjectByType<WaveManager>();
            }
        }
    }
}

