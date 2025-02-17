using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPanel : MonoBehaviour
{
    [SerializeField] private HealthSegment healthPointObject;
    private List<HealthSegment> healthSegments = new List<HealthSegment>();

    public void InstantiateHealthPanelVisuals(int Health)
    {
        // Each segment is 4 health
        int maxSegments = Mathf.CeilToInt(Health / 4);
        for (int i = 0; i < maxSegments; i++)
        {
            HealthSegment segment = Instantiate(healthPointObject);
            segment.transform.SetParent(transform);
            healthSegments.Add(segment);
        }
    }

    public void UpdateHealthPanel(int Health, int HealthLost)
    {
        // Update the sprite animator values when health changes in current segment
        int curSegment = Mathf.CeilToInt(Health / 4);
        int curHealthInSegment = Health - ((curSegment - 1) * 4);
        healthSegments[curSegment - 1].UpdateAnimator(curHealthInSegment, Mathf.Max(0, HealthLost));

        // Checking to see if the health lost results in changes in a different segment
        int newHealth = Health - HealthLost;
        int newSegment = Mathf.CeilToInt(newHealth / 4);

        // When last statement is true; continue this process for the next health segment
        if (newSegment < curSegment)
        {
            curHealthInSegment = newHealth - ((newSegment - 1) * 4);
            healthSegments[curSegment - 1].UpdateAnimator(curHealthInSegment, curHealthInSegment);
        }
    }
}
