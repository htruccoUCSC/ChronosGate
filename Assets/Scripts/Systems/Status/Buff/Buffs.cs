using UnityEngine;
using System;
public class Buffs : MonoBehaviour
{
    public BoardManager board;
    public ModifyUnitStats math;
    [Header("Status Overlay Vfx")]
    [SerializeField] private Sprite m_BuffOverlaySprite;
    [SerializeField] private Vector3 m_BuffOverlayOffset = new Vector3(0f, 0.6f, 0f);
    [SerializeField] private Vector3 m_BuffOverlaySpawnOffset = new Vector3(0f, -0.2f, 0f);
    [SerializeField] private float m_BuffOverlayFloatDistance = 0.4f;
    [SerializeField] private float m_BuffOverlayDuration = 0.6f;
    [SerializeField] private float m_BuffOverlaySpawnMoveDuration = 0.12f;
    [SerializeField] private float m_BuffOverlayScale = 1f;
    [SerializeField] private int m_BuffOverlaySortingOrderOffset = 5;

    public void AddTempBuff(
        BaseUnit unit,
        float attackSpeedMult,
        float attackSpeedFlat,
        float attackDamageFlat,
        float attackDamageMult,
        float abilityPowerFlat,
        float abilityPowerMult,
        float rangeBuff,
        int duration,
        Action<float> OnHit,
        float onHitModifier = 0f,
        Action<float> OnKill = null,
        float onKillModifier = 0f,
        bool calledFromAugment = false,
        bool refreshOnPlacement = false)
    {
        Buff buff = new Buff
        {
            AttackSpeedMult = attackSpeedMult,
            AttackSpeedFlat = attackSpeedFlat,
            AttackDamageFlat = attackDamageFlat,
            AttackDamageMult = attackDamageMult,
            AbilityPowerFlat = abilityPowerFlat,
            RangeBuff = rangeBuff,

            duration = duration,
            OnHit = OnHit,
            OnhitModifier = onHitModifier,
            OnKill = OnKill,
            OnKillModifier = onKillModifier,
            AbilityPowerMult = abilityPowerMult,
            CalledFromAugment = calledFromAugment,
            RefreshOnPlacement = refreshOnPlacement,
        };

        math.AddAttackDamage(unit.myData, attackDamageFlat);
        math.AddAttackSpeed(unit.myData, attackSpeedFlat);
        math.AddAttackMult(unit.myData, attackDamageMult);
        math.AddSpeedMult(unit.myData, attackSpeedMult);
        math.AddAbilityPower(unit.myData, abilityPowerFlat);
        math.AddAbilityPowerMult(unit.myData,abilityPowerMult);
        math.AddRange(unit.myData, rangeBuff);
        unit.AddTempBuff(buff);
        SpawnBuffOverlay(unit);
        
        Debug.Log($"[Buffs.AddTempBuff] {unit.name} SpeedFlatMod: {unit.myData.SpeedFlatMod}, SpeedMultMod: {unit.myData.SpeedMultMod} TOTAL: {unit.myData.GetModifiedAttackSpeed()}");
    }

    public void AddRoundBuff(
        BaseUnit unit,
        float attackSpeedMult,
        float attackSpeedFlat,
        float attackDamageFlat,
        float attackDamageMult,
        float abilityPowerFlat,
        float abilityPowerMult,
        float rangeBuff,
        Action<float> OnHit,
        float onHitModifier = 0f,
        Action<float> OnKill = null,
        float onKillModifier = 0f,
        bool calledFromAugment = false,
        bool refreshOnPlacement = false)
    {
        Buff buff = new Buff
        {
            AttackSpeedMult = attackSpeedMult,
            AttackSpeedFlat = attackSpeedFlat,
            AttackDamageFlat = attackDamageFlat,
            AttackDamageMult = attackDamageMult,
            AbilityPowerFlat = abilityPowerFlat,
            RangeBuff = rangeBuff,

            duration = 0f,
            OnHit = OnHit,
            OnhitModifier = onHitModifier,
            OnKill = OnKill,
            OnKillModifier = onKillModifier,
            AbilityPowerMult = abilityPowerMult,
            CalledFromAugment = calledFromAugment,
            RefreshOnPlacement = refreshOnPlacement,
        };

        math.AddAttackDamage(unit.myData, attackDamageFlat);
        math.AddAttackSpeed(unit.myData, attackSpeedFlat);
        math.AddAttackMult(unit.myData, attackDamageMult);
        math.AddSpeedMult(unit.myData, attackSpeedMult);
        math.AddAbilityPower(unit.myData, abilityPowerFlat);
        math.AddAbilityPowerMult(unit.myData,abilityPowerMult);
        math.AddRange(unit.myData, rangeBuff);

        unit.AddRoundBuff(buff);
        SpawnBuffOverlay(unit);
    }

    public void RemoveTempBuff(BaseUnit unit, Buff buff)
    {
        math.SubAttackDamage(unit.myData, buff.AttackDamageFlat);
        math.SubAttackSpeed(unit.myData, buff.AttackSpeedFlat);
        math.SubAttackMult(unit.myData, buff.AttackDamageMult);
        math.SubSpeedMult(unit.myData, buff.AttackSpeedMult);
        math.SubAbilityPower(unit.myData, buff.AbilityPowerFlat);
        math.SubAbilityPowerMult(unit.myData,buff.AbilityPowerMult);
        math.SubRange(unit.myData, buff.RangeBuff);

        unit.RemoveTempBuff(buff);
        Debug.Log($"[Buffs.RemoveTempBuff] {unit.name} SpeedFlatMod: {unit.myData.SpeedFlatMod}, SpeedMultMod: {unit.myData.SpeedMultMod} TOTAL: {unit.myData.GetModifiedAttackSpeed()}");
    }

    public void RemoveRoundBuff(BaseUnit unit, Buff buff)
    {
        math.SubAttackDamage(unit.myData, buff.AttackDamageFlat);
        math.SubAttackSpeed(unit.myData, buff.AttackSpeedFlat);
        math.SubAttackMult(unit.myData, buff.AttackDamageMult);
        math.SubSpeedMult(unit.myData, buff.AttackSpeedMult);
        math.SubAbilityPower(unit.myData, buff.AbilityPowerFlat);
        math.SubAbilityPowerMult(unit.myData,buff.AbilityPowerMult);
        math.SubRange(unit.myData, buff.RangeBuff);

        unit.RemoveRoundBuff(buff);
    }

    public void RemoveAllBuffs(BaseUnit unit)
    {
        if (unit == null || unit.myData == null)
        {
            return;
        }

        for (int i = unit.activeBuffs.Count - 1; i >= 0; i--)
        {
            RemoveTempBuff(unit, unit.activeBuffs[i]);
        }

        for (int i = unit.roundBuffs.Count - 1; i >= 0; i--)
        {
            RemoveRoundBuff(unit, unit.roundBuffs[i]);
        }
    }

    public void RemovePlacementRefreshBuffs(BoardManager targetBoard)
    {
        if (targetBoard == null || targetBoard.unitGrid == null)
        {
            return;
        }

        int width = targetBoard.unitGrid.GetLength(0);
        int height = targetBoard.unitGrid.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                BaseUnit unit = targetBoard.unitGrid[x, y];
                if (unit == null)
                {
                    continue;
                }

                for (int i = unit.activeBuffs.Count - 1; i >= 0; i--)
                {
                    Buff buff = unit.activeBuffs[i];
                    if (buff != null && buff.RefreshOnPlacement)
                    {
                        RemoveTempBuff(unit, buff);
                    }
                }

                for (int i = unit.roundBuffs.Count - 1; i >= 0; i--)
                {
                    Buff buff = unit.roundBuffs[i];
                    if (buff != null && buff.RefreshOnPlacement)
                    {
                        RemoveRoundBuff(unit, buff);
                    }
                }
            }
        }
    }

    public void PlayBuffOverlay(BaseUnit unit)
    {
        SpawnBuffOverlay(unit);
    }

    private void SpawnBuffOverlay(BaseUnit unit)
    {
        if (unit == null || m_BuffOverlaySprite == null)
        {
            return;
        }

        StatusOverlayVfx.Spawn(
            unit.transform,
            m_BuffOverlaySprite,
            m_BuffOverlayOffset,
            m_BuffOverlaySpawnOffset,
            m_BuffOverlayFloatDistance,
            m_BuffOverlayDuration,
            m_BuffOverlaySpawnMoveDuration,
            m_BuffOverlayScale,
            m_BuffOverlaySortingOrderOffset);
    }
}
