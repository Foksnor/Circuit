using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class TransitionTurns : MonoBehaviour
{
    [SerializeField]
    private GameDataLoader gameDataLoader;
    private Character player;

    [SerializeField] private float maxTurnTime = 8;
    private float curTurnTime;
    public bool NeedsNewCardCalculation = false;
    private bool isPlayerTurnActive = false;
    private bool isEnemyTurnActive = false;

    // Turns before a new enemy spawns
    [SerializeField] private int enemySpawnEveryXRounds = 5;
    private int enemySpawnCooldown = 0;

    private void Awake()
    {
        TurnSequence.TransitionTurns = this;
    }

    private void Update()
    {
        // TEST FUNCTION
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Load the saved game state from a file
            gameDataLoader.RestartScene();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            gameDataLoader.SaveGameState();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            gameDataLoader.DeleteGameState();
        }

        // Turn timer disabled for now
        //SetTurnTimer();
        if (isPlayerTurnActive)
        {
            for (int i = 0; i < TurnSequence.TurnSequenceTriggerables.Count; i++)
            {
                // Invokes all player start triggers
                TurnSequence.TurnSequenceTriggerables[i].OnStartPlayerTurn();

                // Save the game state at the start of your turn
                // QQQ TODO: better save state moment
                //SaveGameState();
            }

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

            // Invokes all enemy start triggers
            for (int i = 0; i < TurnSequence.TurnSequenceTriggerables.Count; i++)
                TurnSequence.TurnSequenceTriggerables[i].OnStartEnemyTurn();

            isEnemyTurnActive = ProcessTeamCards(CharacterTeams._EnemyTeamCharacters);

            // When finishing processing all of the enemy cards; end turn
            if (!isEnemyTurnActive)
            {
                //decide if an enemy should spawn.
                DecideEnemySpawn();

                // Invokes all end turn triggers
                for (int i = 0; i < TurnSequence.TurnSequenceTriggerables.Count; i++)
                    TurnSequence.TurnSequenceTriggerables[i].OnEndTurn();
            }
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

    public void InitiateFirstTurn()
    {
        SpawnerFunctions.Instance.InitiateFirstChunk();
        player = SpawnerFunctions.Instance.SpawnPlayer();
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
            SpawnerFunctions.Instance.SpawnEnemyAtLevelEdge();
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
        if (player != null)
            player.CircuitBoard.PlayerDrawPhase();
    }
}

public static class TurnSequence
{
    public static TransitionTurns TransitionTurns = null;
    public static List<ITurnSequenceTriggerable> TurnSequenceTriggerables = new();
}