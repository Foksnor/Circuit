using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class Character : MonoBehaviour
{
    public CircuitBoard CircuitBoard;
    [HideInInspector] public int PositionInGrid { private set; get; }
    [SerializeField] protected SpriteRenderer characterSpriteRenderer = null;
    [SerializeField] protected Animator characterAnimator = null;
    public CharacterSimulation CharacterSimulation = null;
    public CharacterSimulation InstancedCharacterSimulation { private set; get; } = null;
    public int Health { private set; get; } = 1;
    protected bool isInvulnerable = false;
    private float speed = 1;

    private void Start()
    {
        InstantiateCharacterSimulation();
    }

    private void Update()
    {
        MoveCharacter();
    }

    public void ChangeDestinationGridNumber(int newDestinationInGrid)
    {
        GridPositions._GridCubes[PositionInGrid].RemoveCharacterOnGrid(this);
        GridPositions._GridCubes[newDestinationInGrid].SetCharacterOnGrid(this);
        PositionInGrid = newDestinationInGrid;
    }

    private void MoveCharacter()
    {
        if (Vector3.Distance(transform.position, GridPositions._GridCubes[PositionInGrid].transform.position) > 0.01f)
        {
            characterAnimator.SetBool("isMoving", true);
            transform.position = Vector3.MoveTowards(transform.position, GridPositions._GridCubes[PositionInGrid].transform.position, Time.deltaTime * speed);
        }
        else
            characterAnimator.SetBool("isMoving", false);
    }

    public void ChangeHealth(int amount, Character instigator)
    {
        if (isInvulnerable)
            return;

        Health -= amount;
        if (Health <= 0)
            Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Remove character from their list before destroying it to prevent null references.
        CharacterTeams._PlayerTeamCharacters.Remove(this);
        CharacterTeams._EnemyTeamCharacters.Remove(this);
    }

    public virtual void RefreshCharacterSimulation()
    {
        DestroyCharacterSimulation();
        InstantiateCharacterSimulation();
    }

    public void DestroyCharacterSimulation()
    {
        if (InstancedCharacterSimulation != null)
            if (InstancedCharacterSimulation.gameObject != null)
                Destroy(InstancedCharacterSimulation.gameObject);
    }

    public virtual void InstantiateCharacterSimulation()
    {
        InstancedCharacterSimulation = Instantiate(CharacterSimulation, transform);
        InstancedCharacterSimulation.SetCharacterSimInfo(characterSpriteRenderer);
        InstancedCharacterSimulation.ChangeDestinationGridNumber(PositionInGrid);
    }

    public void ToggleCharacterSimulation(bool isSetupPhase)
    {
        if (InstancedCharacterSimulation != null)
            if (InstancedCharacterSimulation.gameObject != null)
                InstancedCharacterSimulation.gameObject.SetActive(isSetupPhase);
    }
}
