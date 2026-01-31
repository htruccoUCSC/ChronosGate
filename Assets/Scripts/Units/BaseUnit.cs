using UnityEngine;

public abstract class BaseUnit : MonoBehaviour
{
    // our units unique data instance
    public UnitInstance myData;
    protected float attackTimer;
    protected Transform currentTarget;

    // how much of the tile we want the unit to fill
    private const float TILE_FILL_RATIO = 1.0f;

    public virtual void Initialize(UnitInstance instance)
    {
        myData = instance;
        attackTimer = 1f / myData.GetModifiedAttackSpeed();
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
                if (myData.CurrentMana >= myData.BaseDef.MaxMana)
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
        // Raycast based lane scanning that needs to be updated to use the tilemap later
        int layerMask = LayerMask.GetMask("Enemies");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, myData.BaseDef.Range, layerMask);
        currentTarget = hit.collider ? hit.transform : null;

        Debug.DrawRay(transform.position, Vector2.right * myData.BaseDef.Range, Color.red);
    }

    // "abstract" methods that need to be implemented on a unit to unit basis
    // we may look into having some predefined basic attack scripts later
    protected abstract void PerformBasicAttack();
    protected abstract void CastAbility();
}