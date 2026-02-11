using UnityEngine;

public class AugmentManager : MonoBehaviour
{
    public AugmentList augmentList = new AugmentList();

  private float timer = 0f;   // internal timer
    private float interval = 1f; // 1 second interval

    public void ApplyAllAugments()
    {
        for (int i = 0; i < augmentList.activeAugments.Count; i++)
        {
            augmentList.activeAugments[i].Apply();
        }
    }
    public void AddAugment(Augment newAugment)
    {
        augmentList.activeAugments.Add(newAugment);
    }

        public void RemoveAugmentByIndex(int index)
    {
        if (index >= 0 && index < augmentList.activeAugments.Count)
        {
            augmentList.activeAugments.RemoveAt(index);
        }
    }
      void Update()
    {

        timer += Time.deltaTime;

        if (timer >= interval)
        {

            timer = 0f;
            ApplyAllAugments();
        }
    }

}

