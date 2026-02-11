using UnityEngine;

public class AugmentManager : MonoBehaviour
{
    public AugmentList augmentList = new AugmentList();

<<<<<<< Updated upstream
  private float timer = 0f;   // internal timer
    private float interval = 1f; // 1 second interval
=======
    // Temp timer logic
    private float timer = 0f;
    public float interval = 1f;

    void Start()
    {
        ApplyAllActiveAugments();
    }
>>>>>>> Stashed changes

    public void ApplyAllAugments()
    {
        for (int i = 0; i < augmentList.activeAugments.Count; i++)
        {
            augmentList.activeAugments[i].Apply();
        }
    }
<<<<<<< Updated upstream
    public void AddAugment(Augment newAugment)
=======

    public void ApplyAllRoundStartAugments()
    {
        for (int i = 0; i < augmentList.RoundStartAugments.Count; i++)
        {
            augmentList.RoundStartAugments[i].Apply();
        }
    }

    public void AddActiveAugment(Augment newAugment)
>>>>>>> Stashed changes
    {
        augmentList.activeAugments.Add(newAugment);
    }

<<<<<<< Updated upstream
        public void RemoveAugmentByIndex(int index)
=======
    public void RemoveActiveAugmentByIndex(int index)
>>>>>>> Stashed changes
    {
        if (index >= 0 && index < augmentList.activeAugments.Count)
        {
            augmentList.activeAugments.RemoveAt(index);
        }
    }
<<<<<<< Updated upstream
      void Update()
    {

=======

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
>>>>>>> Stashed changes
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;
            ApplyAllAugments();
        }
    }
<<<<<<< Updated upstream

=======
>>>>>>> Stashed changes
}
