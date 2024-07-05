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
        int curHealthInSegment = (curSegment * 4) - Health;
        healthSegments[curSegment].UpdateAnimator(curHealthInSegment, HealthLost);

        // Checking to see if the health lost results in changes in a different segment
        int newHealth = Health - HealthLost;
        int newSegment = Mathf.CeilToInt(newHealth / 4);

        // When last statement is true; continue this process for the next health segment
        if (newSegment < curSegment)
        {
            curHealthInSegment = (newSegment * 4) - newHealth;
            int remainingHealthLost = 4 - curHealthInSegment;
            healthSegments[curSegment].UpdateAnimator(curHealthInSegment, remainingHealthLost);
        }
    }
}
