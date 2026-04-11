using UnityEngine;

public class SpawnableManager : MonoBehaviour
{
    [Header("Default Spawnables")]
    [SerializeField] private GameObject m_EnemyPrefab;
    [SerializeField] private GameObject m_EnemyRedPrefab;
    [SerializeField] private GameObject m_EnemyYellowPrefab;
    [SerializeField] private GameObject m_EnemyGreenPrefab;

    [Header("Unlock Rounds")]
    [SerializeField] private int m_EnemyRedUnlockRound = 2;
    [SerializeField] private int m_EnemyYellowUnlockRound = 3;
    [SerializeField] private int m_EnemyGreenUnlockRound = 4;

    [Header("Runtime List")]
    [SerializeField] private SpawnableList m_SpawnableList = new SpawnableList();

    public SpawnableList SpawnableList => m_SpawnableList;

    private void Awake()
    {
        UpdateSpawnablesForRound(1);
    }

    public void RebuildSpawnableList()
    {
        m_SpawnableList.activeSpawnables.Clear();

        AddSpawnable(m_EnemyPrefab);
    }

    public void UpdateSpawnablesForRound(int roundNumber)
    {
        m_SpawnableList.activeSpawnables.Clear();
        AddSpawnable(m_EnemyPrefab);

        switch (roundNumber)
        {
            case int n when n >= m_EnemyGreenUnlockRound:
                AddSpawnable(m_EnemyRedPrefab);
                AddSpawnable(m_EnemyYellowPrefab);
                AddSpawnable(m_EnemyGreenPrefab);
                break;
            case int n when n >= m_EnemyYellowUnlockRound:
                AddSpawnable(m_EnemyRedPrefab);
                AddSpawnable(m_EnemyYellowPrefab);
                break;
            case int n when n >= m_EnemyRedUnlockRound:
                AddSpawnable(m_EnemyRedPrefab);
                break;
        }
    }

    public bool AddSpawnable(GameObject spawnable)
    {
        if (spawnable == null)
        {
            return false;
        }

        if (m_SpawnableList.activeSpawnables.Contains(spawnable))
        {
            return false;
        }

        m_SpawnableList.activeSpawnables.Add(spawnable);
        return true;
    }

    public bool RemoveSpawnable(GameObject spawnable)
    {
        if (spawnable == null)
        {
            return false;
        }

        return m_SpawnableList.activeSpawnables.Remove(spawnable);
    }

    public GameObject GetRandomSpawnable()
    {
        if (m_SpawnableList.activeSpawnables.Count == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, m_SpawnableList.activeSpawnables.Count);
        return m_SpawnableList.activeSpawnables[randomIndex];
    }
}
