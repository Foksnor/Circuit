using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayTurnPreview : DestroyAfterSceneLoad
{
    [SerializeField] private Animator animator;

    public override void OnNewScene()
    {
        animator.SetBool("isActive", false);
        Destroy(gameObject, 1);
    }
}
