using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_ToggleAnimatorBool : MonoBehaviour
{
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private string parameterName;
    private bool isActive = false;

    public void ToggleAnimatorBool()
    {
        isActive = !isActive;
        targetAnimator.SetBool(parameterName, isActive);
    }
}
