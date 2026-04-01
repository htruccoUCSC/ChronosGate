using System.Collections.Generic;
using UnityEngine;

public class OrbitalLaserBehavior : MonoBehaviour
{
    
   private Transform m_target;
    private float m_lifeTime;
    OrbitalLaser orbit;
    private float m_moveSpeed;
    private Vector3 m_position;
    private  float m_radius;

    private LayerMask m_targetMask;
    private float m_damageTimer = 0f;
private float m_damageInterval = 1f;
private float m_moveInterval = 0.1f;
private float m_movementTimer =0f;
private float m_damage;

public void Initialize(
    Transform theTarget,
    float damage,
    float lifeTime,
    float moveSpeed,
    float radius,
    LayerMask mask,
    OrbitalLaser owner
)
{
    m_target = theTarget;
    m_lifeTime = lifeTime;
    m_damage = damage;
    m_moveSpeed = moveSpeed;
    m_position = m_target.position;
    m_radius = radius;
    m_targetMask = mask;

    orbit = owner;
    
    transform.position = m_position;
}

    void Start()
    {
        transform.localScale = new Vector3(4f, 4f, 1f); 
        m_position.z = -1f;
    }
 void Update()
{
    if (orbit == null)
    {
        Destroy(gameObject);
        return;
    }

    m_movementTimer += Time.deltaTime;
    if (m_movementTimer >= m_moveInterval)
    {
        m_movementTimer = 0f;
        if (m_target == null || !m_target.gameObject.activeInHierarchy)
        {
            List<Transform> nearest = orbit.GetNearestTargets(1);
            m_target = nearest.Count > 0 ? nearest[0] : null;
        }
        else
        {
            Vector3 direction = (m_target.position - m_position).normalized;
            m_position += direction * m_moveSpeed * m_moveInterval;
        }
        transform.position = m_position;
    }

    m_damageTimer += Time.deltaTime;
    if (m_damageTimer >= m_damageInterval)
    {
        m_damageTimer = 0f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            m_radius,
            m_targetMask
        );

        foreach (var hit in hits)
        {
            BaseEnemy enemy = hit.GetComponentInParent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(orbit, Mathf.RoundToInt(m_damage));
            }
        }
    }

    // Lifetime countdown happens every frame
    m_lifeTime -= Time.deltaTime;
    if (m_lifeTime <= 0f)
        Destroy(gameObject);
}


    public bool HandleEnemyTrigger(Collider2D other)
    {
        if (orbit == null)
            return false;

        BaseEnemy enemy = other.GetComponentInParent<BaseEnemy>();
        if (enemy == null)
            return false;

        enemy.TakeDamage(orbit, Mathf.RoundToInt(m_damage));



        return true;
    }

    private void OnDestroy()
    {
        m_target = null;
        orbit = null;
    }

 

 


}
