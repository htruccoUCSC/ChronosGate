using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class CurrencyPickup : MonoBehaviour, IPointerClickHandler
{
    public static event Action<int> Collected;

    [SerializeField] private int amount = 1;

    private void Awake()
    {
        EnsureNonBlockingColliders();
    }

    private void OnEnable()
    {
        EnsureNonBlockingColliders();
    }

    public void Configure(int newAmount)
    {
        amount = newAmount;
        EnsureNonBlockingColliders();
        gameObject.SetActive(true);
    }

    private void OnMouseDown()
    {
        Collect();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Collect();
    }

    public void Collect()
    {
        Collected?.Invoke(amount);
        Destroy(gameObject);
    }

    private void EnsureNonBlockingColliders()
    {
        Collider2D[] pickupColliders = GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < pickupColliders.Length; i++)
        {
            if (pickupColliders[i] != null)
            {
                pickupColliders[i].isTrigger = true;
            }
        }
    }
}
