using UnityEngine;

public class ModifyUnitStats : MonoBehaviour

{
public BoardManager board;


  public void AddAttackDamage(UnitInstance u,float amount)
    {
        u.DamageFlatMod+= amount;

    }
    }