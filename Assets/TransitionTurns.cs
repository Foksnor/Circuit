using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionTurns : MonoBehaviour
{
    [SerializeField] private Entity_Spawner entitySpawner;

    [SerializeField] private float maxTurnTime = 8;
    private float curTurnTime;
    private bool isPlayerTurnActive = false;
    private bool isEnemyTurnActive = false;

    // Turns before a new enemy spawns.
    [SerializeField] private int enemySpawnEveryXRounds = 5;
    private int enemySpawnCooldown = 0;


    private void Awake()
    {
        InitiateFirstTurn();
    }

    private void Update()
    {
        SetTurnTimer();
        if (isPlayerTurnActive)
        {
            CalculateTeamCards(CharacterTeams._PlayerTeamCharacters, false);
            //CalculateTeamCards(CharacterTeams._EnemyTeamCharacters, true);
            isPlayerTurnActive = ProcessTeamCards(CharacterTeams._PlayerTeamCharacters);

            // This sets the enemy turn active as soon as the function ProcessTeamCards returns false when finishing processing player cards.
            isEnemyTurnActive = !isPlayerTurnActive;
        }
        else if (isEnemyTurnActive)
        {
            CalculateTeamCards(CharacterTeams._PlayerTeamCharacters, false);
            CalculateTeamCards(CharacterTeams._EnemyTeamCharacters, false);
            isEnemyTurnActive = ProcessTeamCards(CharacterTeams._EnemyTeamCharacters);

            // When finishing processing all of th enemy cards; decide if an enemy should spawn.
            if (!isEnemyTurnActive)
                DecideEnemySpawn();
        }
        else
        {
            // Player set up phase
            PlayerDrawPhase();

            CalculateTeamCards(CharacterTeams._PlayerTeamCharacters, true);
            CalculateTeamCards(CharacterTeams._EnemyTeamCharacters, true);
        }
    }

    private void SetTurnTimer()
    {
        if (isPlayerTurnActive || isEnemyTurnActive)
        {
            // Pause the turn timer visuals
            CharacterTeams._PlayerTeamCharacters[0].CircuitBoard.SetCircuitDisplayTimer("| |", maxTurnTime);
        }
        else
        {
            // Set the graphical feedback of the turn timer button
            CharacterTeams._PlayerTeamCharacters[0].CircuitBoard.SetCircuitDisplayTimer(Mathf.RoundToInt(curTurnTime).ToString(), curTurnTime / maxTurnTime);

            // Transitions the turn when timer reaches 0
            curTurnTime -= Time.deltaTime;
            if (curTurnTime <= 0)
            {
                TransitionTurn();
            }
        }
    }

    private void InitiateFirstTurn()
    {
        entitySpawner.InitiateGrid();
        CharacterTeams._PlayerTeamCharacters.Add(entitySpawner.SpawnPlayer());
        curTurnTime = maxTurnTime;
    }

    public void TransitionTurn()
    {
        // Cannot tansition the turn if the player- or enemy turn is still busy
        if (isPlayerTurnActive || isEnemyTurnActive)
            return;

        // When this event is called, reset timer
        curTurnTime = maxTurnTime;

        // Process player first, then the enemies
        isPlayerTurnActive = true;

        // Characters and their simulations use the same circuit board
        // Reseting the cooldown during a turn transition makes sure that the character plays their cards in the correct order and tempo
        ForceResetCardProcessing(CharacterTeams._PlayerTeamCharacters);
        ForceResetCardProcessing(CharacterTeams._EnemyTeamCharacters);
    }

    private void DecideEnemySpawn()
    {
        enemySpawnCooldown--;
        if (enemySpawnCooldown < 0)
        {
            enemySpawnCooldown = enemySpawnEveryXRounds;
            CharacterTeams._EnemyTeamCharacters.Add(entitySpawner.SpawnEnemy());
        }
    }

    private bool ProcessTeamCards(List<Character> characterFromTeam)
    {
        bool isTeamTurnActive = false;
        for (int i = 0; i < characterFromTeam.Count; i++)
        {
            isTeamTurnActive = characterFromTeam[i].CircuitBoard.IsProcessingCards(characterFromTeam[i]);
        }
        return isTeamTurnActive;
    }

    private void ForceResetCardProcessing(List<Character> characterFromTeam)
    {
        for (int i = 0; i < characterFromTeam.Count; i++)
        {
            characterFromTeam[i].CircuitBoard.ResetCardProcessing();
        }
    }

    private void CalculateTeamCards(List<Character> characterFromTeam, bool isSetupPhase)
    {
        for (int i = 0; i < characterFromTeam.Count; i++)
        {
            characterFromTeam[i].CircuitBoard.CalculateAllCards(characterFromTeam[i], isSetupPhase);
        }
    }

    private void PlayerDrawPhase()
    {
        CharacterTeams._PlayerTeamCharacters[0].CircuitBoard.PlayerDrawPhase();
    }
}
