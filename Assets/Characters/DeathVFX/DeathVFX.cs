using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathVFX : MonoBehaviour
{
    [SerializeField] private Rigidbody VisualSplitTop;
    [SerializeField] private SpriteRenderer VisualRendererTop, VisualRendererBottom;

    public void SetDeathVFXCharacterVisual(Sprite sprite, bool isSimulation)
    {
        if (isSimulation)
        {
            // Sets the spawned particle in the "Simulation" layer
            // This is needed because the simulation camera renders all simulation based events different than the main camera
            int layerNumber = LayerMask.NameToLayer("Simulation");
            HelperFunctions.SetGameLayerRecursive(gameObject, layerNumber);
        }

        VisualRendererTop.sprite = sprite;
        VisualRendererBottom.sprite = sprite;

        // Adding force to fake animation to make it seem like the top of the character flies of the bottom
        Vector3 explosionLocation = new Vector3(transform.position.x + Random.Range(-0.75f, 0.75f), transform.position.y - 1);
        VisualSplitTop.AddExplosionForce(300, explosionLocation, 3);
        VisualSplitTop.AddTorque(explosionLocation);
        Invoke("(Destroy(gameObject)", 2);
    }
}
