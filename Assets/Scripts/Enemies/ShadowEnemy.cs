using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class ShadowEnemy : BaseEnemy
{  
     //Moves faster than base enemy, regenerates health over time, and splits into a baseenemy upon death
    [SerializeField] private float m_SpeedMultiplier = 1.5f;
    [SerializeField] private float m_HealthRegenRate = 5f; // Health per second
    [SerializeField] private GameObject m_SplitPrefab; // Prefab for the base enemy to spawn upon death
    protected override void Update()
    {
        base.Update();

        // Regenerate health over time
        if (currentHealth < maxHealth)
        {
            currentHealth = Mathf.RoundToInt(Mathf.Min(currentHealth + m_HealthRegenRate * Time.deltaTime, maxHealth));
        }

      
    }

    protected override void Awake()
    {
        base.Awake();
        maxHealth = Mathf.RoundToInt(maxHealth * 2f);
        currentHealth = maxHealth;
        moveSpeed *= m_SpeedMultiplier;
    }
    protected override void Die()
    {
        base.Die();
        SplitIntoBaseEnemy();//Turn into base enemy upon death
    }

    private void SplitIntoBaseEnemy()
    {
        if (m_SplitPrefab == null) return;
        GameObject splitEnemy = Instantiate(m_SplitPrefab, transform.position, Quaternion.identity);
        BaseEnemy splitEnemyScript = splitEnemy.GetComponent<BaseEnemy>();
        if (splitEnemyScript != null)
        {
            // Intentionally empty; BaseEnemy.Start initializes its own health.
        }
    }

}