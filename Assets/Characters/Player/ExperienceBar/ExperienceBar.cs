using Doozy.Runtime.Reactor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceBar : MonoBehaviour
{
    [SerializeField] private Progressor progressor;
    private float experiencePoints;
    private int currentPlayerLevel = 0;
    [SerializeField] private RectTransform progressBarRectTransform;
    public float curExpFill { private set; get; } = 0f;
    [SerializeField] private PlayerLevelScriptableObject[] playerlevels;
    public PlayerLevelScriptableObject[] Playerlevels
    {
        get
        {
            return playerlevels;
        }
        private set
        {
            playerlevels = value;
        }
    }

    private void Awake()
    {
        PlayerUI.ExperienceBar = this;
    }

    public void AddExperiencePoints(int amount)
    {
        experiencePoints += amount;
        curExpFill = experiencePoints / playerlevels[currentPlayerLevel].experienceRequirement;
        progressor.SetValueAt(curExpFill);

        if (experiencePoints >= playerlevels[currentPlayerLevel].experienceRequirement)
            GoToNextLevel();
    }

    private void GoToNextLevel()
    {
        PlayerUI.RewardScreen.GiveCardRewardOptions(playerlevels[currentPlayerLevel].possibleNewCardRewards);
        experiencePoints = 0;

        // Only advance levels when the player has not reached max level rewards yet
        // Player is still able to collect exp to repeatedly get the max level reward
        if (currentPlayerLevel < playerlevels.Length - 1)
            currentPlayerLevel += 1;
        // Correct xp bar visual progression with the new level requirement
        AddExperiencePoints(0);
    }

    public RectTransform GetProgressBarRectTransform()
    {
        return progressBarRectTransform;
    }
}
