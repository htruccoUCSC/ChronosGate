using UnityEngine;

public class FootTrooperUnit : BaseUnit
{
    // Makes grenades deal more damage than the basic attack.
    [SerializeField] private float m_AbilityDamageMultiplier = 1.5f;

    // Bullet speed for the rifle shot.
    [SerializeField] private float m_BulletSpeed = 25f;

    // Changes the launch arc angle for the grenade.
    [SerializeField] private float m_GrenadeLaunchAngle = 60f;
    // Makes grenade bigger than bullet so it is easier to see.
    [SerializeField] private float m_GrenadeScaleMultiplier = 1.75f;


    protected override void PerformBasicAttack()
    {
        // Allows basic damage to be modified from BaseUnit.
        RifleBullet(myData.GetModifiedDamage());
    }

    protected override void CastAbility()
    {
        // Changes ability damage from BaseUnit.
        float grenadeDamage = myData.GetModifiedDamage() * m_AbilityDamageMultiplier;
        // Spawns the Grenade Projectile.
        GrenadeProjectile(grenadeDamage);
        Debug.Log("Foot Trooper throws grenade.");
    }

    private void RifleBullet(float damage)
    {
        // If there is no target, do nothing.
        if (currentTarget == null) return;

        // Reuse the projectile child object from this unit's prefab.
        GameObject projectilePrefab = LoadProjectilePrefab();
        if (projectilePrefab == null) return;

        // Spawn a runtime projectile instance and apply default sprite/scale setup.
        GameObject proj = InstantiateAndSetupProjectile(projectilePrefab);
        if (proj == null) return;

        // Projectile script is the logic component that moves and damages enemies.
        Projectile projScript = proj.GetComponentInChildren<Projectile>();
        if (projScript == null) return;

        // Direction from shooter to target for straight bullet travel.
        Vector2 direction = (currentTarget.position - transform.position).normalized;

        // Bullet uses a fixed high speed so it feels like a rifle shot.
        projScript.speed = m_BulletSpeed;

        // angle = 0f -> no arc, straight shot.
        // isAOE = false -> single-target hit behavior.
        projScript.Setup(damage, direction, 0f, transform.position, false, this);
    }

    private void GrenadeProjectile(float damage)
    {
        // Checks if there is a target.
        if (currentTarget == null) return;

        // Reuse this unit's projectile child as the grenade base object.
        GameObject projectilePrefab = LoadProjectilePrefab();
        if (projectilePrefab == null) return;

        // Spawn and initialize projectile visuals/owner data.
        GameObject proj = InstantiateAndSetupProjectile(projectilePrefab);
        if (proj == null) return;

        // Projectile component handles movement and damage on collision.
        Projectile projScript = proj.GetComponentInChildren<Projectile>();
        if (projScript == null) return;

        // If ability sprite exists, swap projectile art so grenade looks distinct.
        if (_abilitySprite != null)
        {
            SpriteRenderer sr = proj.GetComponentInChildren<SpriteRenderer>();
            Animator animator = proj.GetComponentInChildren<Animator>();

            // If animator is not driving visuals, safely assign sprite directly.
            if (sr != null && animator == null)
            {
                sr.sprite = _abilitySprite;
            }

            // Scale grenade visuals up a bit to read clearly as an ability projectile.
            proj.transform.localScale = Vector3.Scale(transform.localScale, _abilityScale) * m_GrenadeScaleMultiplier;
        }

        // Offset from unit to target. We use this for both direction and ballistic math.
        Vector2 diff = currentTarget.position - transform.position;
        Vector2 direction = diff.normalized;

        // Prevent invalid/flat arc values by forcing minimum angle.
        float launchAngle = Mathf.Max(1f, m_GrenadeLaunchAngle);

        // Matches gravity assumptions used by other arcing projectiles in this project.
        float gravity = Mathf.Abs(Physics2D.gravity.y * 3f);

        // Solve launch speed so grenade lands correctly at target position.
        projScript.speed = CalculateBallisticSpeed(diff, launchAngle, gravity);

        // angle > 0 -> arc shot.
        // isAOE = true -> splash behavior on impact.
        projScript.Setup(damage, direction, launchAngle, transform.position, true, this);
    }
}
