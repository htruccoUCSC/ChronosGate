using UnityEngine;

public class TargetDummyTest : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 0.5f;
    public float moveDistance = 0.75f;
    private Vector3 _startPos;

    void Start()
    {
        _startPos = transform.position;
    }

    void Update()
    {
        float newX = Mathf.Sin(Time.time * moveSpeed) * moveDistance;

        transform.position = _startPos + new Vector3(newX, 0, 0);
    }
}