using Doozy.Runtime.Reactor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceBar : MonoBehaviour
{
    [SerializeField]
    private Progressor progressor;
    private float experiencePoints;
    private int currentPlayerLevel = 0;
    public float curExpFill { private set; get; } = 0f;
    [SerializeField]
    private PlayerLevelScriptableObject[] playerlevels;

    private void Awake()
    {
        PlayerStats.ExperienceBar = this;
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
        PlayerStats.RewardScreen.GiveCardReward(playerlevels[currentPlayerLevel].possibleNewCardRewards);
        experiencePoints = 0;
        currentPlayerLevel += 1;
        // Correct xp bar visual progression with the new level requirement
        AddExperiencePoints(0);
    }
}
