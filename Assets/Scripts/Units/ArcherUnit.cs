using UnityEngine;

public class ArcherUnit : BaseUnit
{
    // commented out example code for how to handle projectiles
    // the prefab for this unit needs a prefab for its projectile assigned in inspector
    //[Header("Archer Specifics")]
    ///public GameObject arrowPrefab;
    float manaCost;

    float currentMana=0;
     void Start()
    {
        // Initialize manaCost here because myData is now available
        manaCost = myData.ManaCost;
    }
    protected override void PerformBasicAttack()
    {
        // gets the modified damage from the unit instance
        float finalDmg = myData.GetModifiedDamage();
        Debug.Log($"Archer {myData.GetInstanceID()} fires for {finalDmg}");
        currentMana=currentMana+10;
        //GameObject arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
        //if (arrow.TryGetComponent(out Projectile proj))
        //{
        //    proj.Setup(finalDmg, Vector2.right);
        //}
    }

    protected override void CastAbility()
    {
        if (currentMana >= manaCost)
        {
            PerformBasicAttack();
            PerformBasicAttack();
            Debug.Log($"Archer {myData.GetInstanceID()}casts ability");
            currentMana=0;
        }

        // implement basic ability logic here
    }
}
