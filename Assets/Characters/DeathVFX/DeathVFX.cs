using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathVFX : MonoBehaviour
{
    [SerializeField] private Rigidbody VisualSplitTop;
    [SerializeField] private SpriteRenderer VisualRendererTop, VisualRendererBottom;

    public void SetDeathVFXCharacterVisual(Sprite sprite)
    {
        VisualRendererTop.sprite = sprite;
        VisualRendererBottom.sprite = sprite;

        // Adding force to fake animation to make it seem like the top of the character flies of the bottom
        Vector3 explosionLocation = new Vector3(transform.position.x + 1, transform.position.y + Random.Range(0, -1f));
        VisualSplitTop.AddExplosionForce(500, explosionLocation, 3);
        VisualSplitTop.AddTorque(explosionLocation);
        Invoke(nameof(Die), 2);
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
