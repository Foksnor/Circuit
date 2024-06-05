using Doozy.Runtime.Reactor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceBar : MonoBehaviour
{
    [SerializeField]
    private Progressor progressor;
    private float experiencePoints;
    private int currentPlayerLevel = 0;
    public float curExpFill { private set; get; }
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
        currentPlayerLevel += 1;
    }
}
