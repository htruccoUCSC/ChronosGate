using UnityEngine;
using System.Collections.Generic;

public class Clastrophobic : MonoBehaviour
{
    public BoardManager board;
    public TileMapManager tileMapManager;
    public CurrencyManager currency;
    public Buffs buffs;
    public WaveManager round;


    private HashSet<BaseUnit> hooked = new HashSet<BaseUnit>();

    public void ClastrophobicCall()
    {
        float buffAmount=0;
        for (int x = 0; x < tileMapManager.Width; x++){
        int tracker=0;
        for (int y = 0; y < tileMapManager.Height; y++)
        {
            BaseUnit unit = board.unitGrid[x, y];
             if (unit == null){
                tracker++;

             }
            
        }
            if (tracker >= tileMapManager.Height)
            {
                buffAmount+=round.currentWave;
            }
        }
        for (int x = 0; x < tileMapManager.Width; x++){
        
        for (int y = 0; y < tileMapManager.Height; y++)
        {
            BaseUnit unit = board.unitGrid[x, y];
             if (unit == null){
               continue;
             }
            Debug.Log("clastrophobic added + " + buffAmount);
              buffs.AddRoundBuff(
                unit,
                attackSpeedMult: 0f, attackSpeedFlat: buffAmount,
                attackDamageFlat: buffAmount, attackDamageMult: 0f,
                abilityPowerFlat: buffAmount, abilityPowerMult: 0f,
                OnHit: null, onHitModifier: 0f,
                OnKill: null, onKillModifier: 0f
            );
            
        }

        }
    }
    
}

