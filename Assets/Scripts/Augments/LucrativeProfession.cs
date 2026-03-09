using UnityEngine;
using System.Collections.Generic;

public class LucrativeProfession : MonoBehaviour
{
    public BoardManager board;
    public TileMapManager tileMapManager;
    public CurrencyManager currency;
    public Buffs buffs;

    public int currencyAmount = 1;

    private int tracker;

    public void LucrativeProfessionCall()
    {
        for (int x = 0; x < tileMapManager.Width; x++)
        for (int y = 0; y < tileMapManager.Height; y++)
        {
            BaseUnit unit = board.unitGrid[x, y];
            if (unit == null) continue;
            if (unit.myData.Name != "Bounty Hunter") continue;


            buffs.AddRoundBuff(
                unit,
                attackSpeedMult: 0f, attackSpeedFlat: 0f,
                attackDamageFlat: 0f, attackDamageMult: 0f,
                abilityPowerFlat: 0f, abilityPowerMult: 0f,
                rangeBuff: 0f,
                OnHit: null, onHitModifier: 0f,
                OnKill: _ => LucrativeProfessionFunction(), onKillModifier: 0f
            );


        }
    }

    public void LucrativeProfessionFunction()
    {
        Debug.Log("lucrative called");
        tracker++;
        if (tracker >= 5)
        {
            tracker = 0;
            currency.AddCurrency(currencyAmount);
             Debug.Log("tracker worked");
        }
    }
}
