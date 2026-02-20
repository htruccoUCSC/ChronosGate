using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class CurrencyPickup : MonoBehaviour, IPointerClickHandler
{
    public static event Action<int> Collected;

    [SerializeField] private int amount = 1;

    public void Configure(int newAmount)
    {
        amount = newAmount;
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
}
