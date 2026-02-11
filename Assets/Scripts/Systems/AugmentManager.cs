using UnityEngine;

public class AugmentManager : MonoBehaviour
{
    public AugmentList augmentList = new AugmentList();

    // Temp timer logic
    private float timer = 0f;
    public float interval = 1f;

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

    public void ApplyAllRoundStartAugments()
    {
        for (int i = 0; i < augmentList.RoundStartAugments.Count; i++)
        {
            augmentList.RoundStartAugments[i].Apply();
        }
    }

    public void AddActiveAugment(Augment newAugment)
    {
        augmentList.activeAugments.Add(newAugment);
    }

    public void RemoveActiveAugmentByIndex(int index)
    {
        if (index >= 0 && index < augmentList.activeAugments.Count)
        {
            augmentList.activeAugments.RemoveAt(index);
        }
    }

    public void AddRoundStartAugment(Augment newAugment)
    {
        augmentList.RoundStartAugments.Add(newAugment);
    }

    public void RemoveRoundStartAugmentByIndex(int index)
    {
        if (index >= 0 && index < augmentList.RoundStartAugments.Count)
        {
            augmentList.RoundStartAugments.RemoveAt(index);
        }
    }

    public void AddShopAugment(Augment newAugment)
    {
        augmentList.ShopAugments.Add(newAugment);
    }

    public void RemoveShopAugmentByIndex(int index)
    {
        if (index >= 0 && index < augmentList.ShopAugments.Count)
        {
            augmentList.ShopAugments.RemoveAt(index);
        }
    }

    public void AddAllAugment(Augment newAugment)
    {
        augmentList.AllAugments.Add(newAugment);
    }

    public void RemoveAllAugmentByIndex(int index)
    {
        if (index >= 0 && index < augmentList.AllAugments.Count)
        {
            augmentList.AllAugments.RemoveAt(index);
        }
    }

    void Update()
    {
        // Temp logic REMOVE LATER WHEN INROUND LOGIC IS THERE
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;
            ApplyAllAugments();
        }
    }
}
