using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionTurns : MonoBehaviour
{
    [SerializeField] private Entity_Spawner entitySpawner;
    private Character player;

    [SerializeField] private float maxTurnTime = 8;
    private float curTurnTime;
    private bool isPlayerTurnActive = false;
    private bool isPlayerSimulationTurnActive = true;
    private bool isEnemyTurnActive = false;
    private bool isEnemySimulationTurnActive = false;

    // Turns before a new enemy spawns
    [SerializeField] private int enemySpawnEveryXRounds = 5;
    private int enemySpawnCooldown = 0;

    public static class TurnCalculation
    {
        public static bool needsNewSimulationCalculation = false;
    }


    private void Awake()
    {
        InitiateFirstTurn();
    }

    private void Update()
    {
        // Turn timer disabled for now
        //SetTurnTimer();
        if (isPlayerTurnActive)
        {
            ResetSimulationProcessing(CharacterTeams._PlayerTeamCharacters);
            ResetSimulationProcessing(CharacterTeams._EnemyTeamCharacters);

            CalculateTeamCards(CharacterTeams._PlayerTeamCharacters, false);
            //CalculateTeamCards(CharacterTeams._EnemyTeamCharacters, true);
            isPlayerTurnActive = ProcessTeamCards(CharacterTeams._PlayerTeamCharacters, false);

            // This sets the enemy turn active as soon as the function ProcessTeamCards returns false when finishing processing player cards.
            isEnemyTurnActive = !isPlayerTurnActive;
        }
        else if (isEnemyTurnActive)
        {
            CalculateTeamCards(CharacterTeams._PlayerTeamCharacters, false);
            CalculateTeamCards(CharacterTeams._EnemyTeamCharacters, false);
            isEnemyTurnActive = ProcessTeamCards(CharacterTeams._EnemyTeamCharacters, false);

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
            
            if (isPlayerSimulationTurnActive)
            {
                isPlayerSimulationTurnActive = ProcessTeamCards(CharacterTeams._PlayerTeamCharacters, true);
                // This sets the enemy simulation active as soon as the function ProcessTeamCards returns false when finishing processing player simulations.
                isEnemySimulationTurnActive = !isPlayerSimulationTurnActive;
            }
            else if (isEnemySimulationTurnActive)
            {
                isEnemySimulationTurnActive = ProcessTeamCards(CharacterTeams._EnemyTeamCharacters, true);
                // This sets the player simulation active as soon as the function ProcessTeamCards returns false when finishing processing enemy simulations.
                isPlayerSimulationTurnActive = !isEnemySimulationTurnActive;

                // At the end of both player and enemy simulation, reset them to repeat the simulations
                if (!isEnemySimulationTurnActive)
                {
                    ResetSimulationProcessing(CharacterTeams._PlayerTeamCharacters);
                    ResetSimulationProcessing(CharacterTeams._EnemyTeamCharacters);
                }
            }

            // When a new simulation is needed
            // E.g. player re-orders their cards in circuit
            if (TurnCalculation.needsNewSimulationCalculation)
            {                
                isPlayerSimulationTurnActive = true;
                ResetSimulationProcessing(CharacterTeams._PlayerTeamCharacters);
                ResetSimulationProcessing(CharacterTeams._EnemyTeamCharacters);
                TurnCalculation.needsNewSimulationCalculation = false;
            }
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
        entitySpawner.InitiateFirstChunk();
        player = entitySpawner.SpawnPlayer();
        CharacterTeams._PlayerTeamCharacters.Add(player);
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

        // QQQ TODO: number of rows being added needs to be the same as the amount of Y steps the player took this turn
        entitySpawner.AddRow(4);
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

    private bool ProcessTeamCards(List<Character> characterFromTeam, bool isSimulation)
    {
        bool isTeamTurnActive = false;
        for (int i = 0; i < characterFromTeam.Count; i++)
        {
            if (isSimulation)
                isTeamTurnActive = characterFromTeam[i].CircuitBoard.IsProcessingSimulation(characterFromTeam[i]);
            else
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

    private void ResetSimulationProcessing(List<Character> characterFromTeam)
    {
        for (int i = 0; i < characterFromTeam.Count; i++)
        {
            characterFromTeam[i].CircuitBoard.ResetSimulationProcessing(characterFromTeam[i]);
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
        player.CircuitBoard.PlayerDrawPhase();
    }
}
