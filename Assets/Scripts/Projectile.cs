using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 10f;
    public float lifetime = 5f;

    // we still keep _damage because your existing system calls Setup(damage, ...)
    // BUT for this assignment requirement we will force hits to deal 5 damage on enemies
    private float _damage;
    private Rigidbody2D _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Setup(float damage, Vector2 direction, float angleInDegrees)
    {
        _damage = damage;
        Destroy(gameObject, lifetime);

        // straight shot
        if (angleInDegrees <= 0)
        {
            // no gravity
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.gravityScale = 0f;

            // fully horizontal velocity
            _rb.linearVelocity = direction * speed;

            // rotates the visual but we should just make our sprites face right by default and avoid this dumbness
            transform.rotation = Quaternion.identity;
        }
        // catapult shot
        else
        {
            // yes gravity
            _rb.bodyType = RigidbodyType2D.Dynamic;
            // very heavy gravity, it looks better this way
            _rb.gravityScale = 3f;

            // calculate launch direction with angle
            float directionSign = Mathf.Sign(direction.x);
            Vector2 launchDir = Quaternion.Euler(0, 0, angleInDegrees * directionSign) * Vector2.right;

            // another visual rotation
            transform.rotation = Quaternion.Euler(0, 0, angleInDegrees);

            // initial velocity is straight along the launch angle
            _rb.linearVelocity = launchDir * speed;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Deal 5 damage every time we hit an enemy
            TargetDummyTest enemy = other.GetComponent<TargetDummyTest>();
            if (enemy != null)
            {
                enemy.TakeDamage(5);
            }

            Destroy(gameObject);
        }
    }
}
