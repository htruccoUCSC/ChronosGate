using UnityEngine;

public class basicAugmentTest : MonoBehaviour
{
public BoardManager board;
public ModifyUnitStats math;

public void allByName(){
foreach (BaseUnit unit in board.unitList)
{
if (unit.myData.BaseDef.UnitID == "Archer")
            {
                math.AddAttackDamage(unit.myData,5);
            }
}
}
}