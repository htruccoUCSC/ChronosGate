using UnityEngine;

public class BaseUnit : MonoBehaviour
{
    protected UnitData stats;
    protected float currentMana;
    protected float attackTimer;
    protected Transform currentTarget;

    // Initialize is called by the Spawner
    public virtual void Initialize(UnitData data)
    {
        stats = data;
        currentMana = stats.StartingMana;
        // Convert "Attacks Per Second" to a delay
        attackTimer = 1f / stats.AttackSpeed;
    }

    void Update()
    {
        if (stats == null) return;

        ScanTargeting();

        if (currentTarget != null)
        {
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0)
            {
                if (currentMana >= stats.MaxMana)
                {
                    CastAbility();
                    currentMana = 0;
                }
                else
                {
                    PerformBasicAttack();
                    currentMana += 10;
                }

                attackTimer = 1f / stats.AttackSpeed;
            }
        }
    }

    // Default targeting scan (can be overridden by children)
    // uess a raycast to find enemies in front of the unit 
    // we should replace this with a system using our tilemap later
    protected virtual void ScanTargeting()
    {
        int layerMask = LayerMask.GetMask("Enemies");

        // Default behavior: Look straight ahead in one lane
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, stats.Range, layerMask);

        if (hit.collider != null)
        {
            currentTarget = hit.transform;
        }
        else
        {
            currentTarget = null;
        }

        // Debugging the scan visually
        Debug.DrawRay(transform.position, Vector2.right * stats.Range, Color.red);
    }

    // --- Virtual Methods for Children --- 
    // These can be overridden by child classes to provide specific behavior
    // We should implement default behavior here for generic units
    protected virtual void PerformBasicAttack() { Debug.Log("Base Attack"); }
    // It might not be possible to make a generic ability, but well see lol
    protected virtual void CastAbility() { Debug.Log("Base Ability"); }
}