using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class TransitionTurns : MonoBehaviour
{
    private Character player;

    [SerializeField] private float maxTurnTime = 8;
    private float curTurnTime;
    public bool NeedsNewCardCalculation = false;
    private bool hasPlayerTurnStarted = false;
    private bool isPlayerTurnActive = false;
    private bool isEnemyTurnActive = false;

    // Timers
    private float upkeepDuration = 0.5f;
    private float upkeepTime = 0;
    private float endstepDuration = 0.5f;
    private float endstepTime = 0;

    // Turns before a new enemy spawns
    [SerializeField] private int enemySpawnEveryXRounds = 5;
    private int enemySpawnCooldown = 0;

    // Keeping track of which scripts needs to trigger a function during a specific turn sequence
    public List<ITurnSequenceTriggerable> TurnSequenceTriggerables = new();

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
            GameData.Loader.RestartScene();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            GameData.Loader.SaveGameState();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            GameData.Loader.DeleteGameState();
        }

        // Turn timer disabled for now
        //SetTurnTimer();
        if (isPlayerTurnActive)
        {
            // Call nesecary start of turn events
            OnStartTurn();

            upkeepTime += Time.deltaTime;
            if (upkeepTime <= upkeepDuration)
                return;

            CalculateTeamCards(Teams.CharacterTeams.PlayerTeamCharacters, false);
            //CalculateTeamCards(CharacterTeams._EnemyTeamCharacters, true);
            isPlayerTurnActive = ProcessTeamCards(Teams.CharacterTeams.PlayerTeamCharacters);

            // This sets the enemy turn active as soon as the function ProcessTeamCards returns false when finishing processing player cards.
            isEnemyTurnActive = !isPlayerTurnActive;
        }
        else if (isEnemyTurnActive)
        {
            CalculateTeamCards(Teams.CharacterTeams.PlayerTeamCharacters, false);
            CalculateTeamCards(Teams.CharacterTeams.EnemyTeamCharacters, false);

            // Invokes all enemy start triggers
            for (int i = 0; i < TurnSequenceTriggerables.Count; i++)
                TurnSequenceTriggerables[i].OnStartEnemyTurn();

            isEnemyTurnActive = ProcessTeamCards(Teams.CharacterTeams.EnemyTeamCharacters);
            if (!isEnemyTurnActive)
                OnEndstep();
        }
        else
        {
            endstepTime += Time.deltaTime;
            if (endstepTime <= endstepDuration)
                return;

            // Call nesecary end of turn events
            OnEndTurn();

            // Player set up phase
            PlayerDrawPhase();

            CalculateTeamCards(Teams.CharacterTeams.PlayerTeamCharacters, true);
            CalculateTeamCards(Teams.CharacterTeams.EnemyTeamCharacters, true);
        }
    }

    private void SetTurnTimer()
    {
        if (isPlayerTurnActive || isEnemyTurnActive)
        {
            // Pause the turn timer visuals
            Teams.CharacterTeams.PlayerTeamCharacters[0].CircuitBoard.SetCircuitDisplayTimer("| |", maxTurnTime);
        }
        else
        {
            // Set the graphical feedback of the turn timer button
            Teams.CharacterTeams.PlayerTeamCharacters[0].CircuitBoard.SetCircuitDisplayTimer(Mathf.RoundToInt(curTurnTime).ToString(), curTurnTime / maxTurnTime);

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
        Teams.CharacterTeams.PlayerCircuitboard.SetUpPlayerDeck();
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
        ForceResetCardProcessing(Teams.CharacterTeams.PlayerTeamCharacters);
        ForceResetCardProcessing(Teams.CharacterTeams.EnemyTeamCharacters);
    }

    private void OnStartTurn()
    {
        // Reset endstep timer
        endstepTime = 0;

        if (!hasPlayerTurnStarted)
        {
            hasPlayerTurnStarted = true;

            // Save the game state at the start of your turn
            GameData.Loader.SaveGameState();

            for (int i = 0; i < TurnSequenceTriggerables.Count; i++)
            {
                // Invokes all player start triggers
                TurnSequenceTriggerables[i].OnStartPlayerTurn();
            }
        }
    }

    private void OnEndstep()
    {
        // Invokes all endstep triggers
        for (int i = 0; i < TurnSequenceTriggerables.Count; i++)
            TurnSequenceTriggerables[i].OnEndstep();
    }

    private void OnEndTurn()
    {
        // Reset turn start trigger and timer
        hasPlayerTurnStarted = false;
        upkeepTime = 0;

        // Decide if an enemy should spawn
        DecideEnemySpawn();

        // Invokes all end turn triggers
        for (int i = 0; i < TurnSequenceTriggerables.Count; i++)
            TurnSequenceTriggerables[i].OnEndTurn();
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
}