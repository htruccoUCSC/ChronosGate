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
private float m_damage;

    public void Initialize( Transform theTarget , float damage, float lifeTime,float moveSpeed,float radius, LayerMask mask)
    {
        m_target=theTarget;
        m_lifeTime = lifeTime;
         m_damage=damage;
         m_moveSpeed=moveSpeed;
         m_position = m_target.transform.position;
         m_radius=radius;
         m_targetMask=mask;
        GameObject laser= orbit.getOrbitalLaser();
    }
    void Start()
    {
        
    }
    void Update()
    { 
        if (m_target == null)
        {
            Debug.Log("Needs new target(will implement later)");
        }
        if(m_position.x<m_target.transform.position.x)
        {
            m_position.x+=m_moveSpeed;
        }
         if(m_position.x>m_target.transform.position.x)
        {
            m_position.x-=m_moveSpeed;
        }
        if(m_position.y>m_target.transform.position.y)
        {
            m_position.y-=m_moveSpeed;
        }
         if(m_position.y<m_target.transform.position.y)
        {
            m_position.y+=m_moveSpeed;
        }
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, m_radius, m_targetMask);

        foreach (var hit in hits)
        {
            TargetDummyTest enemy = hit.GetComponent<TargetDummyTest>();
            if (enemy != null)
            {
                enemy.TakeDamage(Mathf.RoundToInt(m_damage));
            }
    }
    }
    public bool HandleEnemyTrigger(Collider2D other)
    {
         TargetDummyTest enemy = other.GetComponent<TargetDummyTest>();
        enemy.TakeDamage(Mathf.RoundToInt(m_damage));



        return true;
    }

 

 


}
