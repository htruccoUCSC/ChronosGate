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
        u.SpeedFlatMod-= amount;

    }
  public void AddAttackMult(UnitInstance u,float amount)
    {
        u.DamageMultMod+= amount;

    }

      public void AddSpeedMult(UnitInstance u,float amount)
    {
        u.SpeedMultMod+= amount;

    }
      public void SubAttackMult(UnitInstance u,float amount)
    {
        u.DamageMultMod-= amount;

    }

      public void SubSpeedMult(UnitInstance u,float amount)
    {
        u.SpeedMultMod-= amount;

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
    