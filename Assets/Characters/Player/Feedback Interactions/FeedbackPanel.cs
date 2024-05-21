using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public static class FeedbackUI
{
    public static FeedbackPanel FeedbackPanel = null;
}

public class FeedbackPanel : MonoBehaviour
{
    [SerializeField] private Animator animator = null;
    [SerializeField] private TextMeshProUGUI textMeshPro = null;
    [SerializeField] private FeedbackPanelScriptableObject[] feedbackPanelScriptableObjects;

    private float curTime = 0;

    private void Awake()
    {
        FeedbackUI.FeedbackPanel = this;
    }

    public void ShowFeedback(FeedbackPanelScriptableObject._FeedbackType feedbackType)
    {
        FeedbackPanelScriptableObject feedbackScriptObject = null;
        for (int i = 0; i < feedbackPanelScriptableObjects.Length; i++)
        {
            if (feedbackPanelScriptableObjects[i].FeedbackType == feedbackType)
                feedbackScriptObject = feedbackPanelScriptableObjects[i];
        }
        if (feedbackScriptObject == null)
        {
            Debug.LogError(feedbackType + " is not defefined in the feedback panel list");
        }
        else
        {
            animator.SetBool("isShowing", true);
            textMeshPro.text = feedbackScriptObject.FeedbackText;
            curTime = feedbackScriptObject.FeedbackDuration;
        }
    }

    private void Update()
    {
        if (curTime > 0)
            curTime -= Time.deltaTime;
        else
            HideFeedback();
    }

    private void HideFeedback()
    {
        animator.SetBool("isShowing", false);
    }
}
