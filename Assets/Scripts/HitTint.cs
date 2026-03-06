using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class HitTint : MonoBehaviour
{
    private SpriteRenderer m_SpriteRenderer;
    private Coroutine m_FlashCoroutine;
    private Color m_DefaultColor = Color.white;

    private void Awake()
    {
        m_SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (m_SpriteRenderer != null)
        {
            m_DefaultColor = m_SpriteRenderer.color;
        }
    }

    public void Flash()
    {
        if (m_SpriteRenderer == null)
        {
            m_SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (m_SpriteRenderer == null) return;
            m_DefaultColor = m_SpriteRenderer.color;
        }

        if (m_FlashCoroutine != null)
        {
            StopCoroutine(m_FlashCoroutine);
        }

        m_FlashCoroutine = StartCoroutine(FlashRed());
    }

    public void ResetTint()
    {
        if (m_FlashCoroutine != null)
        {
            StopCoroutine(m_FlashCoroutine);
            m_FlashCoroutine = null;
        }

        if (m_SpriteRenderer != null)
        {
            m_SpriteRenderer.color = m_DefaultColor;
        }
    }

    IEnumerator FlashRed()
    {
        m_SpriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        m_SpriteRenderer.color = m_DefaultColor;
        m_FlashCoroutine = null;
    }
}
