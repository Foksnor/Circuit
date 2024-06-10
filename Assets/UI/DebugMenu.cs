using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMenu : MonoBehaviour
{
    [SerializeField]
    private ExperiencePoint experiencePoint;

    private Character player;

    private void Start()
    {
        player = CharacterTeams._PlayerTeamCharacters[0];
    }

    public void SpawnXP()
    {
        Instantiate(experiencePoint, player.transform.position, transform.rotation);
    }

    public void ToggleDebugText()
    {
        for (int i = 0; i < GridPositions._GridCubes.Count; i++)
        {
            GridPositions._GridCubes[i].ToggleDebugText();
        }
    }
}
