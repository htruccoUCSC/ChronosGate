using UnityEngine;

public class ModifyUnitStats : MonoBehaviour

{
public BoardManager board;


  public void AddAttackDamage(UnitInstance u,float amount)
    {
        u.DamageFlatMod+= amount;

    }

      public void AddAttackSpeed(UnitInstance u,float amount)
    {
        u.SpeedFlatMod+= amount;

    }

      public void SubAttackDamage(UnitInstance u,float amount)
    {
        u.DamageFlatMod-= amount;

    }

      public void SubAttackSpeed(UnitInstance u,float amount)
    {
      // Debug.Log("remove" + amount +" "+ u.SpeedFlatMod);
        u.SpeedFlatMod-= amount;
 
    }
  public void AddAttackMult(UnitInstance u,float amount)
    {
        u.DamageMultMod+= amount;

    }

      public void AddSpeedMult(UnitInstance u,float amount)
    {
        if(amount>0){
        Debug.Log("add" + amount +" from "+ u.SpeedFlatMod);
        u.SpeedMultMod+= amount;
        Debug.Log("after add" + u.SpeedFlatMod);
        }

    }
      public void SubAttackMult(UnitInstance u,float amount)
    {
        u.DamageMultMod-= amount;

    }

      public void SubSpeedMult(UnitInstance u,float amount)
    {
        if (amount >0){
        Debug.Log("remove" + amount +" from "+ u.SpeedFlatMod);
        u.SpeedMultMod-= amount;
        Debug.Log("after remove" + u.SpeedFlatMod);
        }

    }
          public void SubAbilityPower(UnitInstance u,float amount)
    {
        u.AbilityPowerFlatMod-= amount;

    }
          public void AddAbilityPower(UnitInstance u,float amount)
    {
        u.AbilityPowerFlatMod+= amount;

    }
             public void SubAbilityPowerMult(UnitInstance u,float amount)
    {
        u.AbilityPowerMult-= amount;

    }
          public void AddAbilityPowerMult(UnitInstance u,float amount)
    {
        u.AbilityPowerMult+= amount;

    }
}
    