using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Projectile))]
public class BoomerangProjectileBehavior : MonoBehaviour
{
    private Projectile m_Projectile;
    private Transform m_IntendedTarget;
    private Transform m_ReturnTarget;
    private Vector3 m_FallbackReturnPoint;
    private float m_CatchRadius;
    private bool m_IsReturning;
    private float m_ReturnElapsed;
    private float m_ReturnDuration;
    private float m_ReturnStartRadius;
    private float m_ReturnStartAngle;
    private float m_ReturnTotalAngle;

    private const float RETURN_LOOP_DEGREES = 320f;

    private readonly HashSet<int> m_OutboundHits = new HashSet<int>();
    private readonly HashSet<int> m_ReturnHits = new HashSet<int>();

    public void Initialize(Projectile projectile, Transform intendedTarget, Transform returnTarget, float catchRadius = 0.15f)
    {
        m_Projectile = projectile;
        m_IntendedTarget = intendedTarget;
        m_ReturnTarget = returnTarget;
        m_FallbackReturnPoint = returnTarget != null ? returnTarget.position : transform.position;
        m_CatchRadius = Mathf.Max(0.05f, catchRadius);

        m_IsReturning = false;
        m_OutboundHits.Clear();
        m_ReturnHits.Clear();
    }

    void Update()
    {
        if (m_Projectile == null || m_Projectile.Body == null) return;

        if (m_IsReturning)
        {
            UpdateReturnArc();
            return;
        }

        if (m_IntendedTarget == null)
        {
            BeginReturnFlight();
            return;
        }

        if ((transform.position - m_IntendedTarget.position).sqrMagnitude <= 0.09f)
        {
            BeginReturnFlight();
        }
    }

    public bool HandleEnemyTrigger(Collider2D other)
    {
        TargetDummyTest enemy = other.GetComponent<TargetDummyTest>();
        if (enemy == null) return false;

        int targetId = other.GetInstanceID();
        HashSet<int> currentPhaseHits = m_IsReturning ? m_ReturnHits : m_OutboundHits;
        if (currentPhaseHits.Contains(targetId)) return true;

        currentPhaseHits.Add(targetId);
        enemy.TakeDamage(Mathf.RoundToInt(m_Projectile.Damage));

        if (!m_IsReturning && m_IntendedTarget != null && other.transform == m_IntendedTarget)
        {
            BeginReturnFlight();
        }

        return true;
    }

    private void BeginReturnFlight()
    {
        if (m_IsReturning || m_Projectile == null || m_Projectile.Body == null) return;

        m_IsReturning = true;
        m_Projectile.DisableApexRetarget();

        Vector2 returnCenter = GetReturnPoint();
        Vector2 fromCenter = (Vector2)transform.position - returnCenter;
        float startRadius = Mathf.Max(fromCenter.magnitude, m_CatchRadius + 0.05f);
        Vector2 normalizedFromCenter = fromCenter.sqrMagnitude > 0.0001f ? fromCenter.normalized : Vector2.right;

        float turnSign = 1f;
        if (m_Projectile.Body.linearVelocity.sqrMagnitude > 0.0001f)
        {
            float cross = Vector3.Cross(normalizedFromCenter, m_Projectile.Body.linearVelocity.normalized).z;
            if (Mathf.Abs(cross) > 0.001f)
            {
                turnSign = Mathf.Sign(cross);
            }
        }

        m_ReturnStartRadius = startRadius;
        m_ReturnStartAngle = Mathf.Atan2(normalizedFromCenter.y, normalizedFromCenter.x);
        m_ReturnTotalAngle = RETURN_LOOP_DEGREES * Mathf.Deg2Rad * turnSign;
        m_ReturnElapsed = 0f;

        float arcLength = startRadius * Mathf.Abs(m_ReturnTotalAngle);
        float inwardLength = startRadius;
        float totalPathLength = arcLength + inwardLength;
        float speed = Mathf.Max(0.1f, m_Projectile.speed);
        m_ReturnDuration = Mathf.Max(0.1f, totalPathLength / speed);

        m_Projectile.Body.bodyType = RigidbodyType2D.Kinematic;
        m_Projectile.Body.gravityScale = 0f;
        m_Projectile.Body.linearVelocity = Vector2.zero;
    }

    private void UpdateReturnArc()
    {
        Vector2 returnCenter = GetReturnPoint();
        m_ReturnElapsed += Time.deltaTime;

        float t = m_ReturnDuration <= 0f ? 1f : Mathf.Clamp01(m_ReturnElapsed / m_ReturnDuration);
        float radius = Mathf.Lerp(m_ReturnStartRadius, 0f, t);
        float angle = m_ReturnStartAngle + (m_ReturnTotalAngle * t);

        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        transform.position = returnCenter + offset;

        float catchDistanceSqr = m_CatchRadius * m_CatchRadius;
        if (((Vector2)transform.position - returnCenter).sqrMagnitude <= catchDistanceSqr || t >= 1f)
        {
            Destroy(gameObject);
        }
    }

    private Vector2 GetReturnPoint()
    {
        if (m_ReturnTarget != null)
        {
            m_FallbackReturnPoint = m_ReturnTarget.position;
        }

        return m_FallbackReturnPoint;
    }
}
