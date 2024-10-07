using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DitheringEffect : MonoBehaviour
{
    public Material ditheringMaterial;  // Assign the material with the dithering shader

    // This function is automatically called by Unity after the camera renders the scene
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (ditheringMaterial != null)
        {
            // Apply the dithering shader
            Graphics.Blit(src, dest, ditheringMaterial);
        }
        else
        {
            // Just pass through if no material is set
            Graphics.Blit(src, dest);
        }
    }
}