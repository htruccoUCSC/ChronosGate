using UnityEngine;

public class ApeTogetherStrong : MonoBehaviour
{
public BoardManager board;

public ModifyUnitStats math;
public TileMapManager tileMapManager;
public Buffs buffs;
public void ApeTogetherStrongCall()
{
for (int x = 0; x <tileMapManager.Height; x++)
{
    int buffAmount = 0;
    for (int y = 0; y <tileMapManager.Width; y++)
    {

        BaseUnit unit = board.unitGrid[x, y]; 

        if (unit == null) continue;

        if (unit.myData.Faction == "Prehistoric")
        {
            buffAmount += 20;
        }
    }
       for (int y = 0; y < tileMapManager.Width; y++)
    {
        BaseUnit unit = board.unitGrid[x, y];
        if (unit == null) continue;
         if (unit.myData.Faction == "Prehistoric")
        {
            buffs.AddRoundBuff(unit,0,0,buffAmount,0,0,null);
            // Debug.Log(buffAmount);
        }
    }
}
}

}
