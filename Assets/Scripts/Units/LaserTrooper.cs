using UnityEngine;
using System.Collections;
using System.Collections.Generic;


    public class LaserTrooper:BaseUnit
    {

        protected override void PerformBasicAttack()
            {
                if (currentTarget == null) return;
                    SpawnProjectile(LoadProjectilePrefab(), myData.GetModifiedDamage(), false);
                if (spawnedProjectiles != null && spawnedProjectiles.Count > 0)
                    {
                        GameObject last = spawnedProjectiles[spawnedProjectiles.Count - 1];
                        if (last != null)
                        {
                            Projectile p = last.GetComponentInChildren<Projectile>();
                            if (p != null)
                            {
                                p.passThroughEnemies = true;
                            }
                        }
                    }  

            }

        protected override void CastAbility()
            {
                Debug.Log("Laser Trooper uses ability");

                if (currentTarget == null) return;
                //Ability is a stronger projectile AD + AP
                SpawnProjectile(LoadProjectilePrefab(), myData.GetModifiedDamage() + myData.GetModifiedAbilityPower(), false);
                //Scale projectile size with ability power for fun!
                if (spawnedProjectiles != null && spawnedProjectiles.Count > 0)
                    {
                        GameObject last = spawnedProjectiles[spawnedProjectiles.Count - 1];
                        if (last != null)
                        {
                            Projectile p = last.GetComponentInChildren<Projectile>();
                            if (p != null)
                            {
                                p.transform.localScale *= 1f + (myData.GetModifiedAbilityPower() / 100f); // Scale size by ability power
                                p.passThroughEnemies = true;
                            }
                        }
                    }            
            }
   
    }