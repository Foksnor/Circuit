using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterHitFlash : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    private bool isOpaque = true;
    private readonly float flashDuration = 0.5f;
    private readonly float coroutineInterval = 0.1f;

    public void PlayHitFlash()
    {
        StartCoroutine(AlternateSpriteAlphaValue());
    }

    private IEnumerator AlternateSpriteAlphaValue()
    {
        float elapsedTime = 0f;
        while (elapsedTime < flashDuration)
        {
            // Alternate the value
            isOpaque = !isOpaque;

            spriteRenderer.enabled = isOpaque;

            yield return new WaitForSeconds(coroutineInterval);

            // Update the elapsed time
            elapsedTime += coroutineInterval;
        }

        // Enable sprite visual after hitflash
        spriteRenderer.enabled = true;
    }
}
