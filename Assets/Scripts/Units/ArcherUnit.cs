using UnityEngine;

public class ArcherUnit : BaseUnit
{
    // commented out example code for how to handle projectiles
    // the prefab for this unit needs a prefab for its projectile assigned in inspector
    //[Header("Archer Specifics")]
    ///public GameObject arrowPrefab;

    protected override void PerformBasicAttack()
    {
        // gets the modified damage from the unit instance
        float finalDmg = myData.GetModifiedDamage();
        Debug.Log($"Archer {myData.GetInstanceID()} fires for {finalDmg}");

        //GameObject arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
        //if (arrow.TryGetComponent(out Projectile proj))
        //{
        //    proj.Setup(finalDmg, Vector2.right);
        //}
    }

    protected override void CastAbility()
    {
        // implement basic ability logic here
    }
}
