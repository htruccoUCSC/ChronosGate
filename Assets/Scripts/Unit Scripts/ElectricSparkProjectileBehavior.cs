using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ElectricSparkProjectileBehavior - Handles the electric spark projectile mechanics.
/// 
/// The spark travels forward through enemies, then moves backward one row at a time.
/// This creates a "spark going back" effect where the projectile:
/// 1. Travels forward and hits the front enemy
/// 2. Passes through and continues forward through the row
/// 3. Once it exits forward enemies, it reverses and moves backward through the same row
/// 4. Can potentially move to adjacent rows as well for multi-directional spark effects
///
/// NOTE: This behavior is attached to the projectile prefab and works with Projectile.cs
/// </summary>
[RequireComponent(typeof(Projectile))]
public class ElectricSparkProjectileBehavior : MonoBehaviour
{
    private Projectile m_Projectile;
    private bool m_IsReturning = false;
    private Vector3 m_ForwardOrigin;
    private float m_ReturnSpeed = 15f;
    private readonly HashSet<int> m_ForwardHits = new HashSet<int>();
    private readonly HashSet<int> m_ReturnHits = new HashSet<int>();

    [Header("Spark Reversal")]
    [SerializeField] private float m_TimeBeforeReturn = 0.5f;
    [SerializeField] private bool m_ReverseDirection = true;

    private float m_ForwardTimer = 0f;

    public void Initialize(Projectile projectile)
    {
        m_Projectile = projectile;
        m_ForwardOrigin = transform.position;
        m_IsReturning = false;
        m_ForwardHits.Clear();
        m_ReturnHits.Clear();
        m_ForwardTimer = m_TimeBeforeReturn;
    }

    void Update()
    {
        if (m_Projectile == null || m_Projectile.Body == null) return;

        if (m_IsReturning)
        {
            UpdateReturnPhase();
        }
        else
        {
            UpdateForwardPhase();
        }
    }

    private void UpdateForwardPhase()
    {
        // After time expires, switch to return phase
        m_ForwardTimer -= Time.deltaTime;
        if (m_ForwardTimer <= 0)
        {
            BeginReturnPhase();
        }
    }

    private void UpdateReturnPhase()
    {
        // Move the projectile backward (negative x direction)
        if (m_ReverseDirection)
        {
            Vector2 currentVelocity = m_Projectile.Body.linearVelocity;
            // Reverse direction for return journey
            m_Projectile.Body.linearVelocity = new Vector2(-m_ReturnSpeed, currentVelocity.y);
        }
    }

    private void BeginReturnPhase()
    {
        m_IsReturning = true;
        Debug.Log("ElectricSpark: Beginning return phase");
    }

    /// <summary>
    /// Called by OnTriggerEnter2D in Projectile to handle enemy hits.
    /// Tracks which enemies were hit in forward vs return phases.
    /// </summary>
    public bool HandleEnemyTrigger(Collider2D other)
    {
        TargetDummyTest enemy = other.GetComponent<TargetDummyTest>();
        if (enemy == null) return false;

        int targetId = other.GetInstanceID();
        HashSet<int> currentPhaseHits = m_IsReturning ? m_ReturnHits : m_ForwardHits;
        
        // If already hit in this phase, don't hit again
        if (currentPhaseHits.Contains(targetId)) return true;

        // Allow hitting enemies in the opposite phase
        HashSet<int> otherPhaseHits = m_IsReturning ? m_ForwardHits : m_ReturnHits;
        if (otherPhaseHits.Contains(targetId))
        {
            // Can hit again in return phase if bouncing back through
            currentPhaseHits.Add(targetId);
            return false; // Let projectile cause damage
        }

        // First hit in this phase
        currentPhaseHits.Add(targetId);
        return false; // Let projectile cause damage
    }

    /// <summary>
    /// Alternative trigger handler for BaseEnemy-based enemies
    /// </summary>
    public bool HandleEnemyTrigger(BaseEnemy enemy)
    {
        if (enemy == null) return false;

        int targetId = enemy.GetInstanceID();
        HashSet<int> currentPhaseHits = m_IsReturning ? m_ReturnHits : m_ForwardHits;
        
        if (currentPhaseHits.Contains(targetId)) return true;

        HashSet<int> otherPhaseHits = m_IsReturning ? m_ForwardHits : m_ReturnHits;
        if (otherPhaseHits.Contains(targetId))
        {
            currentPhaseHits.Add(targetId);
            return false;
        }

        currentPhaseHits.Add(targetId);
        return false;
    }
}
