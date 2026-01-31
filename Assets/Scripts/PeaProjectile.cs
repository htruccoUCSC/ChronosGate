using UnityEngine;

public class PeaProjectile : MonoBehaviour
{
    //making everything float is good i think
    public float speed = 8f;     // units per second
    public int damage = 1;       // stored for later
    public float lifeTime = 5f;  // destroy after time

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += Vector3.right * speed * Time.deltaTime;
    }
}

