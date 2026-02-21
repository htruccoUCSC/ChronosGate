using UnityEngine;

public class BuffManager : MonoBehaviour
{
    public Buffs buffs;
    public BoardManager board;

    private float oneSecondTimer = 0f;

    void Update()
    {
        oneSecondTimer += Time.deltaTime;

        if (oneSecondTimer >= 1f)
        {   
            oneSecondTimer -= 1f;
            UpdateAllTempUnitBuffs();
        }
    }

    private void UpdateAllTempUnitBuffs()
    {
        // Iterate through all units and update their temp buffs
        foreach (BaseUnit unit in board.unitList)
        {
           foreach (Buff buff in unit.activeBuffs.ToArray())
            {
                buff.duration -= 1f;
                if (buff.duration <= 0f)
                {
                    Debug.Log($"Removing expired buff from {unit.name}");
                    buffs.RemoveTempBuff(unit, buff);
                }
            }
        }
    }
    private void NewRoundBuffUpdate()
    {
        // Iterate through all units and update their round buffs
        foreach (BaseUnit unit in board.unitList)
        {
            foreach (Buff buff in unit.roundBuffs.ToArray())
            {
                buffs.RemoveRoundBuff(unit, buff); // Remove buff from unit
            }
        }
    }
}
