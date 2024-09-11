using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterHitFlash : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    private float tempAlphaValue = 1;
    private Color regularColor = Color.white;
    private bool isOpaque = true;
    private readonly float flashDuration = 10;
    private readonly float coroutineInterval = 0.2f;

    private void Awake()
    {
        regularColor = spriteRenderer.color;
    }

    private void Update()
    {
        Debug.Log($"Current Value: {(isOpaque ? 1 : 0)}");
    }

    public void PlayHitFlash()
    {
        StartCoroutine(AlternateSpriteAlphaValue());
    }

    private IEnumerator AlternateSpriteAlphaValue()
    {
        /*
        float elapsedTime = 0f;
        while (elapsedTime < flashDuration)
        {
            isOpaque = !isOpaque;
            tempAlphaValue = isOpaque ? regularColor.a : 0;
            Color newColor = new Color(regularColor.r, regularColor.g, regularColor.b, tempAlphaValue);
            spriteRenderer.color = newColor;

            //elapsedTime += Time.deltaTime;
            //yield return new WaitForSeconds(0.5f);



            // Wait for the specified interval before continuing
            yield return new WaitForSeconds(coroutineInterval);

            // Update the elapsed time
            elapsedTime += coroutineInterval;
        }
        */

        float elapsedTime = 0f;
        while (elapsedTime < flashDuration)
        {
            // Alternate the value
            isOpaque = !isOpaque;

            // Wait for the specified interval before continuing
            yield return new WaitForSeconds(coroutineInterval);

            // Update the elapsed time
            elapsedTime += coroutineInterval;
        }

        // Optional: Final log to indicate the end of the period
        Debug.Log("Value alternation ended.");
    }
}
