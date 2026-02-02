using UnityEngine;

public class ArcherUnit : BaseUnit
{
    // If you want to handle projectiles manually, uncomment and use this:
    // [Header("Archer Specifics")]
    // public GameObject arrowPrefab;

    protected override void PerformBasicAttack()
    {
        // gets the modified damage from the unit instance
        float finalDmg = myData.GetModifiedDamage();
        Debug.Log($"Archer {myData.GetInstanceID()} fires for {finalDmg}");

        // Example projectile firing logic:
        // GameObject arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
        // if (arrow.TryGetComponent(out Projectile proj))
        // {
        //     proj.Setup(finalDmg, Vector2.right);
        // }
    }

    protected override void CastAbility()
    {
        Debug.Log("Archer uses ability"); 
        
        // implement ability logic here
    }
}
