using Unity.VisualScripting;
using UnityEngine;

public class LongGame : MonoBehaviour
{
public BoardManager board;

public ModifyUnitStats math;
public TileMapManager tileMapManager;
public Buffs buffs;
public float startingAmount;
public void LongGameCall()
{
startingAmount++;
   float buffAmount = startingAmount*0.1f;
for (int x = 0; x <tileMapManager.Height; x++)
{

    for (int y = 0; y <tileMapManager.Width; y++)
    {

        BaseUnit unit = board.unitGrid[x, y]; 

        if (unit == null) continue;
      if (unit.myData.Faction == "Future")
        {
           buffs.AddRoundBuff(unit,0,0,0,0,50,buffAmount,null);
        }
                else
                {
                     buffs.AddRoundBuff(unit,0,0,0,0,0,buffAmount,null);
                }


    }
      
}
}

}
