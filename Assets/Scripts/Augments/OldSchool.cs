using UnityEngine;

public class OldSchool : MonoBehaviour
{
public BoardManager board;

public ModifyUnitStats math;
public TileMapManager tileMapManager;
public Buffs buffs;
public void OldSchoolCall()
{
    BaseUnit toCheck;
for (int x = 0; x <tileMapManager.Height; x++)
{
    for (int y = 0; y <tileMapManager.Width; y++)
    {
        int buffAmount = 0;
        BaseUnit unit = board.unitGrid[x, y]; 

        if (unit == null) continue;
        if (unit.myData.Faction == "Prehistoric"||unit.myData.Faction == "Medieval"||unit.myData.Faction == "Mystic")
        {
       
        
        string faction = unit.myData.Faction;
        if (x>= 0)
                {
                    toCheck = board.unitGrid[x-1, y];
                    if (toCheck != null )
                    {
                         if (toCheck.myData.Faction == "Prehistoric"||toCheck.myData.Faction == "Medieval"||toCheck.myData.Faction == "Mystic")
                            {
                                buffAmount += 30;
                            }

                    }
                }
                if (y>= 0)
                {
                    toCheck = board.unitGrid[x, y-1];
                   if (toCheck != null )
                    {
                          if (toCheck.myData.Faction == "Prehistoric"||toCheck.myData.Faction == "Medieval"||toCheck.myData.Faction == "Mystic")
                            {
                                buffAmount += 30;
                            }

                    }
                }
                }
                if (y< board.Height)
                {
                    toCheck = board.unitGrid[x, y+1];
                   if (toCheck != null )
                    {
                          if (toCheck.myData.Faction == "Prehistoric"||toCheck.myData.Faction == "Medieval"||toCheck.myData.Faction == "Mystic")
                            {
                                buffAmount += 30;
                            }

                    }
                }
                 if (x< board.Width)
                {
                    toCheck = board.unitGrid[x+1, y];
                   if (toCheck != null )
                    {
                          if (toCheck.myData.Faction == "Prehistoric"||toCheck.myData.Faction == "Medieval"||toCheck.myData.Faction == "Mystic")
                            {
                                buffAmount += 30;
                            }

                    }
                }
               // Debug.Log("OldSchool added" +buffAmount);
                 buffs.AddRoundBuff(unit,0,0,0,0,buffAmount,null);
    
    }
    }
}
}




