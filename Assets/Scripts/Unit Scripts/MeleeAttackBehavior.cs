using UnityEngine;

public class MeleeAttackBehavior : MonoBehaviour
{
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private float m_RangePadding = 0.5f;
    [SerializeField] private bool m_RequireEnemyTag = true;
    [SerializeField] private float m_SlashLifetime = 0.2f;
    [SerializeField] private float m_SlashForwardOffset = 0.5f;
    [SerializeField] private Vector3 m_SlashScaleMultiplier = Vector3.one;

    public bool TryPerformAttack(Transform attacker, Transform target, float range, float damage)
    {
        if (attacker == null || target == null) return false;

        float maxRange = Mathf.Max(0f, range + m_RangePadding);
        float sqrDistance = (target.position - attacker.position).sqrMagnitude;
        if (sqrDistance > maxRange * maxRange)
        {
            return false;
        }

        if (m_RequireEnemyTag && !IsEnemyTarget(target))
        {
            return false;
        }

        if (m_TargetMask.value != 0)
        {
            Collider2D targetCollider = target.GetComponentInParent<Collider2D>();
            if (targetCollider == null) targetCollider = target.GetComponentInChildren<Collider2D>();
            if (targetCollider == null) return false;

            int layerBit = 1 << targetCollider.gameObject.layer;
            if ((m_TargetMask.value & layerBit) == 0)
            {
                return false;
            }
        }

        int finalDamage = Mathf.Max(0, Mathf.RoundToInt(damage));
        bool canDealDamage = finalDamage > 0;

        if (target.TryGetComponent(out TargetDummyTest dummy))
        {
            if (canDealDamage)
            {
                dummy.TakeDamage(finalDamage);
            }
            SpawnSlashOnHit(attacker, target);
            return true;
        }

        TargetDummyTest parentDummy = target.GetComponentInParent<TargetDummyTest>();
        if (parentDummy != null)
        {
            if (canDealDamage)
            {
                parentDummy.TakeDamage(finalDamage);
            }
            SpawnSlashOnHit(attacker, target);
            return true;
        }

        if (canDealDamage)
        {
            target.gameObject.SendMessage("TakeDamage", finalDamage, SendMessageOptions.DontRequireReceiver);
        }
        SpawnSlashOnHit(attacker, target);
        return true;
    }

    private void SpawnSlashOnHit(Transform attacker, Transform target)
    {
        Transform slashTemplate = GetSlashChild(attacker);
        if (slashTemplate == null)
        {
            return;
        }

        Vector3 direction = target.position - attacker.position;
        direction.z = 0f;
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = attacker.right;
            direction.z = 0f;
        }

        Vector3 normalizedDirection = direction.normalized;
        Vector3 spawnPosition = attacker.position + (normalizedDirection * m_SlashForwardOffset);
        float angle = Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) * Mathf.Rad2Deg;

        GameObject slashObject = Instantiate(slashTemplate.gameObject, spawnPosition, Quaternion.Euler(0f, 0f, angle));
        slashObject.name = "SlashHitVfx";
        slashObject.SetActive(true);
        slashObject.transform.localScale = Vector3.Scale(slashTemplate.lossyScale, m_SlashScaleMultiplier);
        // Set Animator speed to 2x if present
        Animator animator = slashObject.GetComponent<Animator>();
        if (animator != null)
        {
            animator.speed = 2f;
        }
        float slashLifetime = Mathf.Max(0.01f, m_SlashLifetime);
        Destroy(slashObject, slashLifetime);
    }

    private Transform GetSlashChild(Transform attacker)
    {
        Transform directChild = attacker.Find("Slash");
        if (directChild != null)
        {
            return directChild;
        }

        Transform[] children = attacker.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name == "Slash")
            {
                return children[i];
            }
        }

        return null;
    }

    private bool IsEnemyTarget(Transform target)
    {
        if (target == null)
        {
            return false;
        }

        if (target.CompareTag("Enemy"))
        {
            return true;
        }

        Transform parent = target.parent;
        while (parent != null)
        {
            if (parent.CompareTag("Enemy"))
            {
                return true;
            }

            parent = parent.parent;
        }

        return false;
    }
}
