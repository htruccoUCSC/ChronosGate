using UnityEngine;

public class FaceHuggerNest : BaseUnit
{
    [Header("Face Hugger Settings")]
    [SerializeField] private float chargeTime = 5f;
    [SerializeField] private float facehuggerSpeed = 3f;
    [SerializeField] private float facehuggerSlowPercent = 0.6f;
    [SerializeField] private float facehuggerDamagePerSecond = 2f;
    [SerializeField] private LayerMask enemyMask;

    private float currentCharge = 0f;
    private bool isCharged = false;
    private bool facehuggerActive = false;

    private void Awake()
    {
        if (enemyMask.value == 0)
        {
            enemyMask = LayerMask.GetMask("Enemies");
        }
    }

    // Updtate override to handle charging seperately from the base unit's update logic since it needs to be able to charge even when not in combat
    // This is an alternative method to the implementation in Generator.cs which targets itself constalty and casts using the generic mana system in BaseUnit.cs
    protected override void Update()
    {
        if (GameLoopManagerOld.Instance != null)
        {
            if (GameLoopManagerOld.Instance.CurrentState != GameLoopManagerOld.GameState.Combat)
            {
                return;
            }
        }
        else if (GameLoopManager.Instance != null && GameLoopManager.Instance.CurrentState != GameLoopManager.GameState.Combat)
        {
            return;
        }

        if (myData == null) return;

        ScanTargeting();

        if (!isCharged && !facehuggerActive && currentTarget != null)
        {
            currentCharge += Time.deltaTime;

            if (currentCharge >= chargeTime)
            {
                isCharged = true;
                currentCharge = chargeTime;
                CastAbility();
                NotifyAbilityUsed();
            }
        }
    }

    protected override void ScanTargeting()
    {
        // Raycast to find the first enemy in the same row and send the facehugger towards it
        int layerMask = enemyMask.value;
        Vector2 direction = Vector2.right;
        float range = 20f;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, range, layerMask);

        if (hit.collider != null)
        {
            BaseEnemy enemy = hit.collider.GetComponentInParent<BaseEnemy>();
            if (enemy != null && IsSameRow(enemy.transform))
            {
                currentTarget = hit.transform;
                return;
            }
        }

        currentTarget = null;
    }

    // Double check that the target is in the same row as the nest to prevent facehuggers from going to the wrong row
    private bool IsSameRow(Transform target)
    {
        if (WaveManager.Instance == null || WaveManager.Instance.tilemap == null)
        {
            return true;
        }

        var tilemap = WaveManager.Instance.tilemap;
        int myRow = tilemap.WorldToCell(transform.position).y;
        int targetRow = tilemap.WorldToCell(target.position).y;
        
        return myRow == targetRow;
    }

    protected override void CastAbility()
    {
        if (currentTarget == null || !isCharged)
        {
            return;
        }

        //  Spawn the facehugger projectile
        GameObject facehuggerObj = InstantiateAndSetupProjectile(LoadProjectilePrefab());
        if (facehuggerObj == null)
        {
            Debug.LogError("[FaceHuggerNest] Failed to instantiate facehugger!");
            return;
        }

        //  Initialize the facehugger projectile
        FaceHuggerProjectile facehugger = facehuggerObj.GetComponent<FaceHuggerProjectile>();
        if (facehugger == null)
        {
            facehugger = facehuggerObj.AddComponent<FaceHuggerProjectile>();
        }

        facehugger.Initialize(
            myData.GetModifiedAbilityPower(),
            facehuggerSpeed,
            facehuggerSlowPercent,
            facehuggerDamagePerSecond,
            transform.position,
            enemyMask,
            this
        );

        facehugger.OnFaceHuggerDestroyed += OnFaceHuggerDestroyed;

        isCharged = false;
        currentCharge = 0f;
        facehuggerActive = true;
    }

    private void OnFaceHuggerDestroyed()
    {
        facehuggerActive = false;
    }

    // Debug visuals
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = isCharged ? Color.green : Color.yellow;
        float chargePercent = currentCharge / chargeTime;
        Vector3 barStart = transform.position + Vector3.up * 0.6f;
        Vector3 barEnd = barStart + Vector3.right * chargePercent * 0.5f;
        Gizmos.DrawLine(barStart, barEnd);

        if (facehuggerActive)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
}
