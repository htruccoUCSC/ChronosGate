using UnityEngine;

public class UnitRuntime : MonoBehaviour
{
    public UnitInstance unit; // points at the UnitInstance for THIS placed tower

    public void TakeDamage(float damage)
    {
        if (unit == null) return;

        unit.CurrentHP -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage, HP now {unit.CurrentHP}");

        if (unit.CurrentHP <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
