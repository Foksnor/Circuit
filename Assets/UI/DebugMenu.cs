using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMenu : MonoBehaviour
{
    [SerializeField]
    private ExperiencePoint experiencePoint;

    public void SpawnXP()
    {
        Instantiate(experiencePoint, Teams.CharacterTeams.PlayerTeamKing.transform.position, transform.rotation);
    }

    public void ToggleDebugText()
    {
        for (int i = 0; i < Grid.GridPositions.GridCubes.Count; i++)
        {
            Grid.GridPositions.GridCubes[i].ToggleDebugText();
        }
    }
}
