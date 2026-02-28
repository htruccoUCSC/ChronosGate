using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles a single portal option's visuals and click behavior.
/// </summary>
public class PortalOption : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image m_portalImage;
    [SerializeField] private TMP_Text m_descriptionText;
    [SerializeField] private Button m_button;

    private Action<int> m_onSelected;
    private int m_portalIndex = -1;
    private bool m_isInteractable = true;
    private bool m_hasSelected = false;

    public void Initialize(int index, string description, Color tint, Action<int> onSelectedCallback)
    {
        m_portalIndex = index;
        m_onSelected = onSelectedCallback;
        m_hasSelected = false;
        m_isInteractable = true;

        if (m_descriptionText != null)
        {
            m_descriptionText.text = description;
        }

        if (m_portalImage != null)
        {
            m_portalImage.color = tint;
        }

        if (m_button != null)
        {
            m_button.interactable = true;
        }
    }

    public void SetInteractable(bool interactable)
    {
        m_isInteractable = interactable;
        m_hasSelected = false;

        if (m_button != null)
        {
            m_button.interactable = interactable;
        }
    }

    private void TriggerSelection()
    {
        if (!m_isInteractable || m_hasSelected)
        {
            return;
        }

        m_hasSelected = true;
        m_onSelected?.Invoke(m_portalIndex);
    }

    private void Awake()
    {
        if (m_button == null)
        {
            m_button = GetComponent<Button>();
        }

        if (m_button != null)
        {
            m_button.onClick.AddListener(TriggerSelection);
        }
    }
    
    private void OnDestroy()
    {
        if (m_button != null)
        {
            m_button.onClick.RemoveListener(TriggerSelection);
        }
    }

}