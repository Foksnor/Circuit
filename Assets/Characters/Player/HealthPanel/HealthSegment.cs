using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSegment : MonoBehaviour
{
    [SerializeField] private Animator fillCurrentAnimator, fillLostAnimator;

    public void UpdateAnimator(int currentHealth, int lostHealth)
    {
        fillCurrentAnimator.SetFloat("CurrentParts", lostHealth);
        fillLostAnimator.SetFloat("CurrentParts", currentHealth);
        fillLostAnimator.SetFloat("LostParts", lostHealth);
    }
}
