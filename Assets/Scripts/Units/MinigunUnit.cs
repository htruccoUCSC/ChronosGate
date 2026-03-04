using System.Collections;
using UnityEngine;

public class MinigunUnit : BaseUnit
{
    [Header("Minigun - Heat Settings")]
    [SerializeField] private float maxHeat = 100f;
    [SerializeField] private float heatPerShot = 8f;
    [SerializeField] private float coolPerSecond = 20f;
    [SerializeField] private float resumeHeatThreshold = 30f;

    [Header("Minigun - Ability (no heat)")]
    [SerializeField] private float abilityDuration = 5f;
    [SerializeField] private float abilityShotInterval = 0.08f; // ~12.5 shots/sec while ability is active

    [Header("Minigun - Projectile")]
    [SerializeField] private float projectileSpeed = 18f;
    [SerializeField] private bool ignoreRowCheck = false;

    private float heat = 0f;
    private bool overheated = false;

    private bool abilityFiring = false;
    private Coroutine coolRoutine;


    protected override void PerformBasicAttack()
    {
        // If ability is currently doing its own firing, don't double-fire.
        if (abilityFiring) return;

        // Start cooling logic
        EnsureCoolingRunning();

        // If overheated, don't shoot
        if (overheated) return;

        // Need a target
        if (currentTarget == null) return;

        float dmg = myData != null ? myData.GetModifiedDamage() : 0f;
        SpawnMinigunProjectile(dmg);

        // Add heat
        heat += heatPerShot;
        if (heat >= maxHeat)
        {
            heat = maxHeat;
            overheated = true;
        }
    }

    protected override void CastAbility()
    {
        // Ability: shoot for 5 seconds ignoring heat
        if (!abilityFiring)
        {
            EnsureCoolingRunning();
            StartCoroutine(AbilityBurstFire());
        }
    }

    //Ability

    private IEnumerator AbilityBurstFire()
    {
        abilityFiring = true;

        float t = 0f;
        while (t < abilityDuration)
        {
            // If there is a target, fire. If not, just wait and keep trying.
            if (currentTarget != null)
            {
                float dmg = myData != null ? myData.GetModifiedDamage() : 0f;
                SpawnMinigunProjectile(dmg);
            }

            yield return new WaitForSeconds(abilityShotInterval);
            t += abilityShotInterval;
        }

        abilityFiring = false;
    }

    private void SpawnMinigunProjectile(float damage)
    {
        if (currentTarget == null) return;

        GameObject projPrefab = LoadProjectilePrefab();
        if (projPrefab == null) return;

        GameObject projRoot = InstantiateAndSetupProjectile(projPrefab);
        if (projRoot == null) return;

        Projectile p = projRoot.GetComponentInChildren<Projectile>();
        if (p == null) return;

        // Force minigun bullets to be straight, fast shots
        p.speed = projectileSpeed;

        // Make sure the collider used for hits is a trigger
        Collider2D col = p.GetComponent<Collider2D>();
        if (col == null) col = p.GetComponentInChildren<Collider2D>();
        if (col != null) col.isTrigger = true;

        if (ignoreRowCheck)
            p.SetIgnoreRowCheck(true);

        Vector2 dir = (currentTarget.position - transform.position).normalized;
        if (dir == Vector2.zero) dir = Vector2.right;

        // Setup will assign rigidbody type and velocity
        p.Setup(damage, dir, 0f, transform.position, false, this);
    }

    // cooling logic

    private void EnsureCoolingRunning()
    {
        if (coolRoutine == null)
            coolRoutine = StartCoroutine(CoolHeatLoop());
    }

    private IEnumerator CoolHeatLoop()
    {
        while (true)
        {
            // cool continuously
            if (heat > 0f)
            {
                heat = Mathf.Max(0f, heat - coolPerSecond * Time.deltaTime);
            }

            // recover from overheated state once cooled enough
            if (overheated && heat <= resumeHeatThreshold)
            {
                overheated = false;
            }

            yield return null;
        }
    }
}