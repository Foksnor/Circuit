using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OverlayTurnPreview : DestroyAfterSceneLoad
{
    [SerializeField] private Animator animator;

    public override void OnNewScene()
    {
        base.OnNewScene();
        animator.SetBool("isActive", false);
        Destroy(gameObject, 1);
    }
}
