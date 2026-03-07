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

    [Header("Ellipse Flight")]
    [SerializeField] private bool m_UseEllipseFlight = true;
    [SerializeField] private float m_EllipseMinorAxisRatio = 0.6f;
    [SerializeField] private float m_MinEllipseMajor = 0.2f;
    [SerializeField] private float m_MinEllipseDuration = 0.2f;

    private Vector2 m_EllipseCenter;
    private Vector2 m_EllipseAxisX;
    private Vector2 m_EllipseAxisY;
    private float m_EllipseMajor;
    private float m_EllipseMinor;
    private float m_EllipseDuration;
    private float m_EllipseElapsed;
    private float m_EllipseAngleStart;
    private float m_EllipseAngleDelta;

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

        if (m_UseEllipseFlight)
        {
            SetupEllipseFlight();
        }
    }

    void Update()
    {
        if (m_Projectile == null || m_Projectile.Body == null) return;

        if (m_UseEllipseFlight)
        {
            UpdateEllipseFlight();
            return;
        }

        if (m_IsReturning)
        {
            if (TryCatchAtReturnTarget())
            {
                return;
            }

            UpdateReturnArc();
            TryCatchAtReturnTarget();
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
        enemy.TakeDamage(Mathf.RoundToInt(m_Projectile.Damage), m_Projectile.Owner);

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

    private void SetupEllipseFlight()
    {
        Vector2 returnPos = GetReturnPoint();
        Vector2 targetPos = m_IntendedTarget != null ? (Vector2)m_IntendedTarget.position : returnPos + Vector2.right;
        Vector2 toTarget = targetPos - returnPos;
        float distance = toTarget.magnitude;

        if (distance <= 0.001f)
        {
            toTarget = Vector2.right;
            distance = 1f;
            targetPos = returnPos + toTarget;
        }

        m_EllipseAxisX = toTarget / distance;
        m_EllipseAxisY = new Vector2(-m_EllipseAxisX.y, m_EllipseAxisX.x);

        if (m_Projectile != null && m_Projectile.Body != null)
        {
            Vector2 velocity = m_Projectile.Body.linearVelocity;
            if (velocity.sqrMagnitude > 0.0001f)
            {
                if (Vector2.Dot(-m_EllipseAxisY, velocity.normalized) < 0f)
                {
                    m_EllipseAxisY = -m_EllipseAxisY;
                }
            }
        }

        m_EllipseMajor = Mathf.Max(m_MinEllipseMajor, distance * 0.5f);
        float ratio = Mathf.Clamp01(m_EllipseMinorAxisRatio);
        m_EllipseMinor = Mathf.Max(0.01f, m_EllipseMajor * Mathf.Max(0.05f, ratio));
        m_EllipseCenter = returnPos + m_EllipseAxisX * (distance * 0.5f);

        float speed = m_Projectile != null ? Mathf.Max(0.1f, m_Projectile.speed) : 1f;
        float circumference = Mathf.PI * (3f * (m_EllipseMajor + m_EllipseMinor) - Mathf.Sqrt((3f * m_EllipseMajor + m_EllipseMinor) * (m_EllipseMajor + 3f * m_EllipseMinor)));
        m_EllipseDuration = Mathf.Max(m_MinEllipseDuration, circumference / speed);
        m_EllipseElapsed = 0f;
        m_EllipseAngleStart = Mathf.PI;
        m_EllipseAngleDelta = Mathf.PI * 2f;

        if (m_Projectile != null && m_Projectile.Body != null)
        {
            m_Projectile.DisableApexRetarget();
            m_Projectile.Body.bodyType = RigidbodyType2D.Kinematic;
            m_Projectile.Body.gravityScale = 0f;
            m_Projectile.Body.linearVelocity = Vector2.zero;
        }

        transform.position = EvaluateEllipse(m_EllipseAngleStart);
    }

    private void UpdateEllipseFlight()
    {
        m_EllipseElapsed += Time.deltaTime;
        float t = m_EllipseDuration <= 0f ? 1f : Mathf.Clamp01(m_EllipseElapsed / m_EllipseDuration);
        float angle = m_EllipseAngleStart + (m_EllipseAngleDelta * t);

        transform.position = EvaluateEllipse(angle);
        m_IsReturning = t >= 0.5f;

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }

    private Vector2 EvaluateEllipse(float angle)
    {
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        return m_EllipseCenter + (m_EllipseAxisX * (m_EllipseMajor * cos)) + (m_EllipseAxisY * (m_EllipseMinor * sin));
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

    private bool TryCatchAtReturnTarget()
    {
        if (!m_IsReturning)
        {
            return false;
        }

        Vector2 returnCenter = GetReturnPoint();
        float catchDistanceSqr = m_CatchRadius * m_CatchRadius;
        if (((Vector2)transform.position - returnCenter).sqrMagnitude <= catchDistanceSqr)
        {
            Destroy(gameObject);
            return true;
        }

        return false;
    }
}
