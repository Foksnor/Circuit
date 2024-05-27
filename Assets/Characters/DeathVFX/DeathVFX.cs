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
        Vector3 explosionLocation = new Vector3(transform.position.x + Random.Range(-0.75f, 0.75f), transform.position.y - 1);
        VisualSplitTop.AddExplosionForce(300, explosionLocation, 3);
        VisualSplitTop.AddTorque(explosionLocation);
        Invoke("(Destroy(gameObject)", 2);
    }
}
