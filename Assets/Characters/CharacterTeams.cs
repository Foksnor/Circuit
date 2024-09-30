using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterTeams : MonoBehaviour
{
    public List<Character> PlayerTeamCharacters { get; set; }
    public Character PlayerTeamKing { get; private set; }
    public List<Character> EnemyTeamCharacters { get; set; }

    private void Awake()
    {
        Teams.CharacterTeams = this;
        PlayerTeamCharacters = new();
        EnemyTeamCharacters = new();
    }

    public void SetKing(Character character)
    {
        PlayerTeamKing = character;
    }
}

public static class Teams
{
    public static CharacterTeams CharacterTeams = null;
}