using UnityEngine;

public class AugmentManager : MonoBehaviour
{
    public AugmentList augmentList = new AugmentList();

    void Start()
    {
        ApplyAllAugments();
    }

    public void ApplyAllAugments()
    {
        for (int i = 0; i < augmentList.activeAugments.Count; i++)
        {
            augmentList.activeAugments[i].Apply();
        }
    }
}
