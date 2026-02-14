using UnityEngine;

public class basicAugmentTest : MonoBehaviour
{
public BoardManager board;
public ModifyUnitStats math;

public Buffs buffs;
public void allByName(){
foreach (BaseUnit unit in board.unitList)
{
if (unit.myData.BaseDef.UnitID == "Archer")
            {
                buffs.AddRoundBuff(unit,0,0,20,0,null);
               // buffs.attackSpeedBuff(unit.myData,5,10);
            }
}
}
}