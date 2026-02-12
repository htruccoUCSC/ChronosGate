using UnityEngine;

using System.Collections.Generic;
public abstract class BaseUnit : MonoBehaviour
{
    // our units unique data instance
    public UnitInstance myData;
    protected float attackTimer;
    protected Transform currentTarget;
     public List<Buff> roundBuffs = new List<Buff>();
    public List<Buff> activeBuffs = new List<Buff>();
    private Sprite _projectileSprite;
    private Vector3 _projectileScale = Vector3.one;

    // how much of the tile we want the unit to fill
    private const float TILE_FILL_RATIO = 1.0f;

    public virtual void Initialize(UnitInstance instance)
    {
        myData = instance;
        attackTimer = 1f / myData.GetModifiedAttackSpeed();

        // getting projectile sprite from prefab if applicable
        Transform template = transform.Find("ProjectileSprite");
        if (template != null)
        {
            SpriteRenderer sr = template.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                _projectileSprite = sr.sprite;
            }
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

        // scales the projectile template to match the unit's size
        // i'm not entirely happy with this implementation but it will do for now
        Transform projTemplate = transform.Find("ProjectileSprite");
        if (projTemplate != null)
        {
            SpriteRenderer projSR = projTemplate.GetComponent<SpriteRenderer>();
            if (projSR != null)
            {
                projTemplate.localScale = Vector3.one;
                Vector3 projSize = projSR.bounds.size;
                float projMax = Mathf.Max(projSize.x, projSize.y);

                // scale to be 40% of the unit's size
                if (projMax > 0)
                {
                    float targetSize = 0.4f;
                    float projScale = targetSize / projMax;

                    projTemplate.localScale = Vector3.one * projScale;

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
                // example melee attack logic doesn't do anything rn
                if (Vector2.Distance(transform.position, currentTarget.position) <= myData.BaseDef.Range + 0.5f)
                {
                    // ApplyDamage(currentTarget, damage);
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

    // spawns a generic projectile towards the current target
    private void SpawnGenericProjectile(float damage)
    {
        if (currentTarget == null) return;

        GameObject genericPrefab = Resources.Load<GameObject>("Prefabs/BaseProjectile");
        if (genericPrefab == null) return;

        GameObject proj = Instantiate(genericPrefab, transform.position, Quaternion.identity);

        if (_projectileSprite != null)
        {
            var sr = proj.GetComponentInChildren<SpriteRenderer>();
            sr.sprite = _projectileSprite;
            proj.transform.localScale = Vector3.Scale(transform.localScale, _projectileScale);
        }

        if (proj.TryGetComponent(out Projectile projScript))
        {
            // --- DIRECTION LOGIC ---
            Vector2 diff = currentTarget.position - transform.position;
            float distance = diff.magnitude;
            Vector2 direction = diff.normalized;
            float launchAngle = myData.BaseDef.LaunchAngle;

            // start with the default speed from the prefab settings
            float finalSpeed = projScript.speed;

            // for projectiles with a launch angle we need to calculate the required speed
            if (launchAngle > 0)
            {
                // scale gravity to match projectile.cs
                float gravity = Physics2D.gravity.y * 3f;
                // make gravity positive for calculation
                gravity = Mathf.Abs(gravity);

                finalSpeed = CalculateBallisticSpeed(distance, launchAngle, gravity);

                // overwrite the projectile speed for ballistic projectiles
                projScript.speed = finalSpeed;
            }

            // launch the projectile
            projScript.Setup(damage, direction, launchAngle);
        }
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
    private float CalculateBallisticSpeed(float distance, float angleDeg, float gravity)
    {
        // convert angle to radians
        float angleRad = angleDeg * Mathf.Deg2Rad;

        //v = Sqrt( (dist * g) / Sin(2 * theta) )
        float bottom = Mathf.Sin(2 * angleRad);

        // safety guard to avoid divide by zero
        if (Mathf.Abs(bottom) < 0.01f) return 10f;

        float v2 = (distance * gravity) / bottom;
        return Mathf.Sqrt(Mathf.Abs(v2));
    }
}