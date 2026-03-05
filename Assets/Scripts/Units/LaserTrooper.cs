using UnityEngine;
using System.Collections;
using System.Collections.Generic;


    public class LaserTrooper:BaseUnit
    {
        private void SpawnStraightLaser(float damage, float scaleMultiplier = 1f)
        {
            if (currentTarget == null) return;

            GameObject projectilePrefab = LoadProjectilePrefab();
            if (projectilePrefab == null) return;

            GameObject proj = InstantiateAndSetupProjectile(projectilePrefab);
            if (proj == null) return;

            Projectile p = proj.GetComponentInChildren<Projectile>();
            if (p == null) return;

            // Force straight horizontal travel so laser does not arc like a catapult shot.
            p.Setup(damage, Vector2.right, 0f, transform.position, false, this);
            p.passThroughEnemies = true;

            if (scaleMultiplier != 1f)
            {
                p.transform.localScale *= scaleMultiplier;
            }
        }

        protected override void PerformBasicAttack()
            {
                SpawnStraightLaser(myData.GetModifiedDamage());

            }

        protected override void CastAbility()
            {
                Debug.Log("Laser Trooper uses ability");

                // Ability is a stronger straight projectile, scaled by ability power.
                float abilityScale = 1f + (myData.GetModifiedAbilityPower() / 100f);
                SpawnStraightLaser(myData.GetModifiedDamage() + myData.GetModifiedAbilityPower(), abilityScale);
            }
   
    }
