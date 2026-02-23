using UnityEngine;
using System;

public class RoundSystem : MonoBehaviour
{
    public enum GamePhase
    {
        Combat,
        Shop,
        AugmentSelection
    }

    [Header("References")]
    [SerializeField] private BoardManager m_Board;
    [SerializeField] private GameObject m_AugmentSelectionPanel;
    [Header("Round Settings")]
    [SerializeField] private int m_CurrentRound;
    [SerializeField] private int m_AugmentEveryNRounds = 3;

    public int CurrentRound => m_CurrentRound;
    public GamePhase Phase { get; private set; } = GamePhase.Combat;

    /// <summary>
    /// Event fired when the round number changes
    /// Allows UI systems to update the round display
    /// </summary>
    public event Action<int> OnRoundChanged;

    public void TestAdvanceRound()
    {
        ClearEnemies();
        AdvanceRound();
    }

    public void CompleteAugmentSelection()
    {
        Phase = GamePhase.Shop;
    }

    private void AdvanceRound()
    {
        m_CurrentRound++;
        
        // Fire the OnRoundChanged event so UI systems can update
        OnRoundChanged?.Invoke(m_CurrentRound);

        if (m_AugmentEveryNRounds > 0 && m_CurrentRound % m_AugmentEveryNRounds == 0)
        {
            Phase = GamePhase.AugmentSelection;
            ToggleAugmentSelection(true);
        }
        else
        {
            Phase = GamePhase.Shop;
        }
    }

    private void ToggleAugmentSelection(bool isActive)
    {
        if (m_AugmentSelectionPanel != null)
        {
            m_AugmentSelectionPanel.SetActive(isActive);
        }
    }

    private void ClearEnemies()
    {
        if (m_Board == null || m_Board.EnemyParent == null)
        {
            return;
        }

        for (int i = m_Board.EnemyParent.childCount - 1; i >= 0; i--)
        {
            Destroy(m_Board.EnemyParent.GetChild(i).gameObject);
        }
    }
}
