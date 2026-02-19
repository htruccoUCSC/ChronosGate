using UnityEngine;

public class Unique : MonoBehaviour
{
public BoardManager board;

public ModifyUnitStats math;
public TileMapManager tileMapManager;
public Buffs buffs;
public void UniqueCall()
{
    BaseUnit toCheck;
for (int x = 0; x <tileMapManager.Height; x++)
{
    for (int y = 0; y <tileMapManager.Width; y++)
    {

        BaseUnit unit = board.unitGrid[x, y]; 

        if (unit == null) continue;

        string faction = unit.myData.Faction;
        if (x>= 1)
                {
                    toCheck = board.unitGrid[x-1, y];
                    if (toCheck != null && toCheck.myData.Faction == faction)
                    {
                        break;
                    }
                }
                if (y>= 1)
                {
                    toCheck = board.unitGrid[x, y-1];
                    if (toCheck != null && toCheck.myData.Faction == faction)
                    {
                        break;
                    }
                }
                if (y< board.Height)
                {
                    toCheck = board.unitGrid[x, y+1];
                    if (toCheck != null && toCheck.myData.Faction == faction)
                    {
                        break;
                    }
                }
                 if (x< board.Width)
                {
                    toCheck = board.unitGrid[x+1, y];
                    if (toCheck != null && toCheck.myData.Faction == faction)
                    {
                        break;
                    }
                }
                //Debug.Log("Unique activated on " + x+","+y);
                 buffs.AddRoundBuff(unit,3,0,3,0,50,0,null);
    }

    }
}

}


