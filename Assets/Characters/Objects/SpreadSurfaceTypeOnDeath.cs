using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class SpreadSurfaceTypeOnDeath : MonoBehaviour
{
    [SerializeField] private Character character = null;
    [SerializeField] private _SurfaceType surfaceEffectToSpread;
    [SerializeField] private int spreadDiameter = 5;


    private void OnDestroy()
    {
        SpreadSurfaceType();
    }

    private void SpreadSurfaceType()
    {
        // Starting position of the surface spread
        Vector2 startingPos = (Vector2)transform.position - (Vector2.one * Mathf.Round(spreadDiameter / 2));
        
        for (int width = 0; width < spreadDiameter; width++)
        {
            Vector2 gridPos = new Vector2(startingPos.x + width, startingPos.y);
            for (int length = 0; length < spreadDiameter; length++)
            {
                gridPos = new Vector2(gridPos.x, startingPos.y + length);
                GridCube affectedCube = GridPositions.GetGridByPosition(gridPos);
                if (affectedCube != null)
                {
                    // Small chance for grids on the outside of the spread to not appear
                    if (width == 0 || width == spreadDiameter - 1 ||
                        length == 0 || length == spreadDiameter - 1)
                    {
                        float rng = Random.Range(0, 100);
                            if (rng < 50)
                            affectedCube.ToggleSurface(character, surfaceEffectToSpread);
                    }
                    else
                        affectedCube.ToggleSurface(character, surfaceEffectToSpread);
                }
            }
        }
    }
}
