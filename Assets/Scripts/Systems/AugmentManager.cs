using UnityEngine;

public class AugmentManager : MonoBehaviour
{
    public AugmentList augmentList = new AugmentList();

  private float timer = 0f;   // internal timer
    private float interval = 1f; // 1 second interval

    public void ApplyAllActiveAugments()
    {
        for (int i = 0; i < augmentList.activeAugments.Count; i++)
        {
            augmentList.activeAugments[i].Apply();
        }
    }
    public void AddActiveAugment(Augment newAugment)
    {
        augmentList.activeAugments.Add(newAugment);
    }
    public void AddInactiveAugment(Augment newAugment)
    {
        augmentList.inactiveAugments.Add(newAugment);
    }
        public void RemoveActiveAugmentByIndex(int index)
    {
        if (index >= 0 && index < augmentList.activeAugments.Count)
        {
            augmentList.activeAugments.RemoveAt(index);
        }
    }
            public void RemoveInactiveAugmentByIndex(int index)
    {
        if (index >= 0 && index < augmentList.activeAugments.Count)
        {
            augmentList.inactiveAugments.RemoveAt(index);
        }
    }
      void Update()
    {

        timer += Time.deltaTime;

        if (timer >= interval)
        {

            timer = 0f;
            ApplyAllActiveAugments();
        }
    }

}

