using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LifePanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lifeCounterText;
    [SerializeField] private Animator lifePanelAnimator;
    private int lifeCount = 3;

    private void Awake()
    {
        PlayerStats.LifePanel = this;
        VisualizeLifeCountUpdate(0);
    }

    public int AdjustLifeCount(int count)
    {
        lifeCount += count;
        VisualizeLifeCountUpdate(count);
        return lifeCount;
    }

    private void VisualizeLifeCountUpdate(int count)
    {
        // Play animation based on whether player gained or lost life
        string boolName = count > 0 ? "Gain" : "Lose";
        lifePanelAnimator.Play(boolName);
        lifeCounterText.text = lifeCount.ToString();
    }
}
