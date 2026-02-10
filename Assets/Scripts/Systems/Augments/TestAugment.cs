using UnityEngine;

public class TestAugment : MonoBehaviour
{
public BoardManager board;
public ModifyUnitStats math;

public Buffs buffs;
public void Test(){
foreach (BaseUnit unit in board.unitList)
{
if (unit.myData.BaseDef.UnitID == "Archer")
            {
                Debug.Log("Test Augment Called");
                // buffs.AddRoundBuff(unit,0,0,20,0);
               // buffs.attackSpeedBuff(unit.myData,5,10);
            }
}
}
}