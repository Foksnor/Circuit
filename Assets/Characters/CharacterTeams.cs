using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;

public class CharacterTeams : MonoBehaviour
{
    public List<Character> PlayerTeamCharacters { get; set; } = new();
    public List<Character> EnemyTeamCharacters { get; set; } = new();
    public Character PlayerTeamKing { get; private set; }

    private void Awake()
    {
        Teams.CharacterTeams = this;
    }

    public void ResetTeams()
    {
        PlayerTeamCharacters.Clear();
        EnemyTeamCharacters.Clear();
    }

    public void SetPlayerKingIfNoneActive(Character character)
    {
        if (PlayerTeamKing == null)
            SetPlayerKing(character);
    }

    public void SetPlayerKing(Character character)
    {
        PlayerTeamKing = character;
        MainCamera.CameraFollowTarget.SetCameraFollowTarget(PlayerTeamKing.transform.gameObject);
    }

    public void RemovePlayerKing()
    {
        PlayerTeamKing = null;
        MainCamera.CameraFollowTarget.SetCameraFollowTarget(null);
    }
}

public static class Teams
{
    public static CharacterTeams CharacterTeams = null;
}