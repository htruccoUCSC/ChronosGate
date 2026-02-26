using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class HitTint : MonoBehaviour
{
    public void Flash()
    {
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null) return;
        StartCoroutine(FlashRed(spriteRenderer));
    }

    IEnumerator FlashRed(SpriteRenderer spriteRenderer)
    {
        Debug.Log("Flashing red");
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        // Flash red for 0.1 seconds
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
}