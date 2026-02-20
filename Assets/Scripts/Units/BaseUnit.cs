using UnityEngine;

using System.Collections.Generic;
public abstract class BaseUnit : MonoBehaviour
{
    // our units unique data instance
    public UnitInstance myData;
    public float attackTimer;
    protected Transform currentTarget;
     public List<Buff> roundBuffs = new List<Buff>();
    public List<Buff> activeBuffs = new List<Buff>();
    protected List<GameObject> spawnedProjectiles = new List<GameObject>();
    protected Sprite _abilitySprite;
protected Vector3 _abilityScale = Vector3.one;
    protected Sprite _projectileSprite;
    protected Vector3 _projectileScale = Vector3.one;
    private MeleeAttackBehavior m_MeleeAttackBehavior;

    // how much of the tile we want the unit to fill
    private const float TILE_FILL_RATIO = 1.0f;

    public virtual void Initialize(UnitInstance instance)
    {
        myData = instance;
        attackTimer = 1f / myData.GetModifiedAttackSpeed();

        // getting projectile sprite and scale from the Projectile child
        Transform projectileChild = transform.Find("Projectile");
        if (projectileChild != null)
        {
            SpriteRenderer sr = projectileChild.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                _projectileSprite = sr.sprite;
            }
        }
         Transform abilityChild = transform.Find("Ability");
    if (abilityChild != null)
    {
        SpriteRenderer sr = abilityChild.GetComponent<SpriteRenderer>();
        if (sr != null)
            _abilitySprite = sr.sprite;

        _abilityScale = abilityChild.localScale;
    }

        NormalizeSpriteSize();
    }

    // scales the unit to fit nicely in a tile based on its sprite size
    private void NormalizeSpriteSize()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null) return;
        transform.localScale = Vector3.one;
        if (sr.transform != transform) sr.transform.localScale = Vector3.one;
        Vector3 spriteSize = sr.bounds.size;
        float maxDimension = Mathf.Max(spriteSize.x, spriteSize.y);
        if (maxDimension > 0)
        {
            float scaleFactor = TILE_FILL_RATIO / maxDimension;
            transform.localScale = Vector3.one * scaleFactor;
        }

        // scales the projectile to match the unit's size
        Transform projectileChild = transform.Find("Projectile");
        if (projectileChild != null)
        {
            SpriteRenderer projSR = projectileChild.GetComponent<SpriteRenderer>();
            if (projSR != null)
            {
                projectileChild.localScale = Vector3.one;
                Vector3 projSize = projSR.bounds.size;
                float projMax = Mathf.Max(projSize.x, projSize.y);

                // scale to be 40% of the unit's size
                if (projMax > 0)
                {
                    float targetSize = 0.4f;
                    float projScale = targetSize / projMax;

                    projectileChild.localScale = Vector3.one * projScale;

                    // store for later use when spawning projectiles
                    _projectileScale = Vector3.one * projScale;
                }
            }
        }
    }

    // example update loop which will probably be entirely scrapped later
    protected virtual void Update()
    {
        if (myData == null) return;

        ScanTargeting();

        if (currentTarget != null)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                // cast ability if mana is full, otherwise do basic attack
                if (myData.CurrentMana >= myData.BaseDef.AbilityManaCost)
                {
                    CastAbility();
                    myData.CurrentMana = 0;
                }
                else
                {
                    PerformBasicAttack();
                    myData.CurrentMana += 10;
                }
                attackTimer = 1f / myData.GetModifiedAttackSpeed();
            }
        }
    }

    protected virtual void ScanTargeting()
    {
        // uses a layer mask to find enemies
        // this is a placeholder implementation and should be replaced with something else, probably involving the tilemap
        int layerMask = LayerMask.GetMask("Enemies");

        Vector2 direction = Vector2.right;
        float range = myData.BaseDef.Range;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, range, layerMask);

        if (hit.collider != null)
        {
            Debug.DrawRay(transform.position, direction * hit.distance, Color.green);
            currentTarget = hit.transform;
        }
        else
        {
            Debug.DrawRay(transform.position, direction * range, Color.red);
            currentTarget = null;
        }
    }

    // "abstract" methods that need to be implemented on a unit to unit basis
    // we now have a basic implementation for basic attacks
    protected virtual void PerformBasicAttack()
    {
        float damage = myData.GetModifiedDamage();

        switch (myData.BaseDef.AttackFunction)
        {
            case BasicAttackType.Melee:
                if (m_MeleeAttackBehavior == null)
                {
                    m_MeleeAttackBehavior = GetComponent<MeleeAttackBehavior>();
                }

                if (m_MeleeAttackBehavior != null)
                {
                    TryPerformMeleeAttack(damage);
                }
                break;

            // projectile attack logic
            case BasicAttackType.Projectile:
                SpawnGenericProjectile(damage);
                break;

            // no basic attack probably used by generators 
            // most units will override rather than using this
            case BasicAttackType.None:
                break;
        }
    }

    protected bool TryPerformMeleeAttack(float damage)
    {
        if (myData == null || myData.BaseDef == null)
        {
            return false;
        }

        return TryPerformMeleeAttack(damage, myData.BaseDef.Range);
    }

    protected bool TryPerformMeleeAttack(float damage,float meleerange)
    {
        if (currentTarget == null || myData == null || myData.BaseDef == null)
        {
            return false;
        }

        if (m_MeleeAttackBehavior == null)
        {
            m_MeleeAttackBehavior = GetComponent<MeleeAttackBehavior>();
        }

        if (m_MeleeAttackBehavior == null)
        {
            return false;
        }

        float range = myData.BaseDef.Range;
        bool didHit = m_MeleeAttackBehavior.TryPerformAttack(transform, currentTarget, meleerange, damage);

        if (didHit)
        {
            onHit();
        }

        return didHit;
    }

    protected virtual GameObject LoadProjectilePrefab()
    {
        Transform projectileChild = transform.Find("Projectile");
        if (projectileChild != null)
        {
            return projectileChild.gameObject;
        }
        
        Debug.LogError($"{gameObject.name} does not have a 'Projectile' child object. Please add a Projectile prefab as a child in the prefab editor.");
        return null;
    }

    protected GameObject InstantiateAndSetupProjectile(GameObject prefab)
    {
        if (prefab == null) return null;

        GameObject proj = Instantiate(prefab, transform.position, Quaternion.identity);
        proj.SetActive(true);   
        //List of spawnedProjectiles so can wipe for new Round
         spawnedProjectiles.Add(proj);
        // Apply projectile sprite and scale
        if (_projectileSprite != null)
        {
            var sr = proj.GetComponentInChildren<SpriteRenderer>();
            var animator = proj.GetComponentInChildren<Animator>();
            if (sr != null && animator == null) sr.sprite = _projectileSprite;
            proj.transform.localScale = Vector3.Scale(transform.localScale, _projectileScale);
        }

        return proj;
    }

    protected void SpawnProjectile(GameObject prefab, float damage, bool isAOE)
    {
        if (currentTarget == null || prefab == null) return;

        GameObject proj = InstantiateAndSetupProjectile(prefab);
        if (proj == null) return;

        Projectile projScript = proj.GetComponentInChildren<Projectile>();
        if (projScript == null) return;

        // Build a vector from shooter to target.
        // X controls forward travel distance, Y captures lane height difference.
        Vector2 diff = currentTarget.position - transform.position;
        Vector2 direction = diff.normalized;
        float launchAngle = myData.BaseDef.LaunchAngle;

        float finalSpeed = projScript.speed;

        if (launchAngle > 0)
        {
            float gravity = Physics2D.gravity.y * 3f;
            gravity = Mathf.Abs(gravity);

            // Solve launch speed using both horizontal distance and vertical offset.
            // This is what lets arc shots land correctly on enemies in different lanes.
            finalSpeed = CalculateBallisticSpeed(diff, launchAngle, gravity);
            projScript.speed = finalSpeed;
        }

        projScript.Setup(damage, direction, launchAngle, transform.position, isAOE, this);
    }
protected void SpawnSniperProjectile(GameObject prefab, float damage, bool isAOE)
{
    if (currentTarget == null || prefab == null) return;

    GameObject projRoot = InstantiateAndSetupProjectile(prefab);
    if (projRoot == null) return;

    Projectile p = projRoot.GetComponentInChildren<Projectile>();
    if (p == null) return;

    Vector2 dir = (currentTarget.position - transform.position).normalized;

    // sniper tuning
    p.speed = 25f;

    // IMPORTANT: make sure we flip trigger on the same collider that will collide
    Collider2D col = p.GetComponent<Collider2D>();
    if (col == null) col = p.GetComponentInChildren<Collider2D>();
    if (col != null) col.isTrigger = true;

    // optionally ignore lane/row checks for sniper
    p.SetIgnoreRowCheck(true);

    // Require sniper projectile to only collide with its assigned target.
    p.SetDesignatedTarget(currentTarget);

    // Let Projectile.Setup set RB type + velocity
    p.Setup(damage, dir, 0f, transform.position, isAOE, this);
}



    // spawns a generic projectile towards the current target
    private void SpawnGenericProjectile(float damage)
    {
        SpawnProjectile(LoadProjectilePrefab(), damage, false);
    }

    // ability needs to be implemented by each unit type
    protected abstract void CastAbility();
    public void AddTempBuff(Buff buff)
    {
        activeBuffs.Add(buff);
    }

    public void RemoveTempBuff(Buff buff)
    {
        activeBuffs.Remove(buff);
    }
        public void AddRoundBuff(Buff buff)
    {
        roundBuffs.Add(buff);
    }

    public void RemoveRoundBuff(Buff buff)
    {
        roundBuffs.Remove(buff);
    }
    // solves standard projectile motion equation for Velocity
    protected float CalculateBallisticSpeed(float distance, float angleDeg, float gravity)
    {
        // Legacy overload kept for compatibility.
        // Treats target as same height by using Y = 0.
        return CalculateBallisticSpeed(new Vector2(distance, 0f), angleDeg, gravity);
    }

    protected float CalculateBallisticSpeed(Vector2 targetOffset, float angleDeg, float gravity)
    {
        // Decompose target offset into horizontal travel and vertical difference.
        // Example: target one lane above means verticalOffset > 0.
        float horizontalDistance = Mathf.Abs(targetOffset.x);
        float verticalOffset = targetOffset.y;

        if (horizontalDistance < 0.01f || gravity <= 0f)
        {
            return 10f;
        }

        float angleRad = angleDeg * Mathf.Deg2Rad;
        float cosAngle = Mathf.Cos(angleRad);
        float tanAngle = Mathf.Tan(angleRad);

        // Rearranged projectile-motion equation for speed squared:
        // v^2 = (g * x^2) / (2 * cos^2(theta) * (x * tan(theta) - y)).
        // If denominator <= 0, this angle cannot reach that target point.
        float denominator = 2f * cosAngle * cosAngle * ((horizontalDistance * tanAngle) - verticalOffset);
        if (denominator <= 0.01f)
        {
            return 10f;
        }

        float v2 = (gravity * horizontalDistance * horizontalDistance) / denominator;
        if (v2 <= 0f)
        {
            return 10f;
        }

        // Convert speed squared into launch speed magnitude.
        return Mathf.Sqrt(v2);
    }

    public void onHit()
    {
        for(int i = 0; i < roundBuffs.Count; i++){
            roundBuffs[i].OnHit?.Invoke();
        }
    }
    public void DestroyAllProjectiles()
{
    for (int i = 0; i < spawnedProjectiles.Count; i++)
    {
        if (spawnedProjectiles[i] != null)
            Destroy(spawnedProjectiles[i]);
    }

    spawnedProjectiles.Clear();
}
    public void ResetMana()
    {
        myData.CurrentMana=myData.StartingMana;
    }
        public void ResetHealth()
    {
        myData.CurrentMana=myData.StartingMana;
    }
    //TODO REMOVE THIS TO NEW FILE
    public void LuckyShotPerformAutoAttack()
    {
        int randomChance = Random.Range(0, 2);
        if(randomChance == 1){
        Debug.Log("Lucky Shot Activated! Unit performs an immediate basic attack.");
        PerformBasicAttack();
        }
    }
}
