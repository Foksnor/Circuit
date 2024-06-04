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
    [SerializeField] private DeathVFX deathVFX = null;
    public CharacterSimulation CharacterSimulation = null;
    public CharacterSimulation InstancedCharacterSimulation { private set; get; } = null;
    [HideInInspector] public bool isSimulationMarkedForDeath;
    private GameObject cardPrevisBinder = null;
    private List<GameObject> ActiveCardPrevisTiles = new List<GameObject>();
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

    protected virtual void Die()
    {
        // Spawn death VFX
        bool isSimulation = InstancedCharacterSimulation == null;
        DeathVFX deathobj = Instantiate(deathVFX, transform.position, transform.rotation);
        deathobj.SetDeathVFXCharacterVisual(characterSpriteRenderer.sprite, isSimulation);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Remove character from their list before destroying it to prevent null references.
        CharacterTeams._PlayerTeamCharacters.Remove(this);
        CharacterTeams._EnemyTeamCharacters.Remove(this);
        GridPositions._GridCubes[PositionInGrid].RemoveCharacterOnGrid(this);
    }

    public virtual void RefreshCharacterSimulation()
    {
        DestroyCharacterSimulation();
        InstantiateCharacterSimulation();
        RemoveCardPrevis();
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
        InstancedCharacterSimulation.SetCharacterSimInfo(this, characterSpriteRenderer);
        InstancedCharacterSimulation.ChangeDestinationGridNumber(PositionInGrid);
    }

    public virtual void ToggleCardPrevis(bool isShowingPrevis, GameObject tilevisual, float angle)
    {
        if (cardPrevisBinder == null)
            cardPrevisBinder = new GameObject("CardPrevisBinder");

        if (isShowingPrevis)
        {
            if (!ActiveCardPrevisTiles.Contains(tilevisual))
            {
                ActiveCardPrevisTiles.Add(tilevisual);
                tilevisual = Instantiate(tilevisual, tilevisual.transform.position, transform.rotation);
                tilevisual.transform.eulerAngles = new Vector3(0, 0, angle);
                tilevisual.transform.parent = cardPrevisBinder.transform;
                tilevisual.SetActive(true);
            }
        }
        else
            RemoveCardPrevis();
    }

    private void RemoveCardPrevis()
    {
        Destroy(cardPrevisBinder);
        ActiveCardPrevisTiles.Clear();
    }

    public void ToggleCharacterSimulation(bool isSetupPhase)
    {
        if (isSetupPhase)
        {
            if (CharacterSimulation == null &&
                !isSimulationMarkedForDeath)
                InstantiateCharacterSimulation();
        }
        else
        {
            if (CharacterSimulation != null)
                DestroyCharacterSimulation();
            isSimulationMarkedForDeath = false;
        }
    }

    public bool IsCharacterRelatedToMe(Character comparisonCharacter)
    {
        if (comparisonCharacter.gameObject == gameObject)
            return true;

        if (comparisonCharacter.InstancedCharacterSimulation != null)
            if (comparisonCharacter.InstancedCharacterSimulation.gameObject == gameObject)
                return true;

        return false;
    }
}
