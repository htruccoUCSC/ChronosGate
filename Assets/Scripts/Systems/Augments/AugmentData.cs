using UnityEngine;

[CreateAssetMenu(fileName = "NewAugmentData", menuName = "Game/Augment Data")]
public class AugmentData : ScriptableObject
{
    public string augmentID;
    public string augmentName;
    public string description;
    public int cost = 10;
    
    // Reference to the actual Augment (or create it on demand)
    public Augment CreateAugment()
    {
        return new Augment(() => { }, augmentName, description);
    }
}