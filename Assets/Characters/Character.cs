using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class Character : MonoBehaviour
{
    public _TeamType TeamType;
    public enum _TeamType { Player, Enemy, Neutral };
    public CircuitBoard CircuitBoard;
    public GridCube AssignedGridCube { private set; get; }
    [SerializeField] public SpriteRenderer CharacterSpriteRenderer = null;
    [SerializeField] protected Animator characterAnimator = null;
    [SerializeField] private DeathVFX deathVFX = null;
    [SerializeField] private GameObject ExperiencePoint = null;
    [SerializeField] private int ExperienceAmountOnDeath = 3;
    public CharacterSimulation CharacterSimulation = null;
    public CharacterSimulation InstancedCharacterSimulation { get; private set; } = null;
    [HideInInspector] public bool isSimulationMarkedForDeath;
    private GameObject cardPrevisBinder = null;
    private List<GameObject> ActiveCardPrevisTiles = new List<GameObject>();
    public int Health { get { return health; } set { health = value; } }
    [SerializeField] protected int health;
    [SerializeField] private HealthPanel healthPanel = null;
    protected bool isInvulnerable = false;
    private float cardPlaySpeed = 1;
    
    public float cardSimulationSpeedModifier { private set; get; } = 2;

    private void Awake()
    {
        if (healthPanel != null)
            healthPanel.InstantiateHealthPanelVisuals(health);
    }

    private void Start()
    {
        InstantiateCharacterSimulation();
    }

    private void Update()
    {
        MoveCharacter();
    }

    public void ChangeDestinationGrid(GridCube newDestinationInGrid, float speedModifier)
    {
        if (AssignedGridCube != null)
            AssignedGridCube.RemoveCharacterOnGrid(this);
        newDestinationInGrid.SetCharacterOnGrid(this);
        AssignedGridCube = newDestinationInGrid;
        cardPlaySpeed = 1 / speedModifier;

        // If character is a simulation, up their card play speed
        if (CharacterSimulation == null)
            cardPlaySpeed *= cardSimulationSpeedModifier;
    }

    private void MoveCharacter()
    {
        if (Vector3.Distance(transform.position, AssignedGridCube.transform.position) > 0.01f)
        {
            characterAnimator.SetBool("isMoving", true);
            transform.position = Vector3.MoveTowards(transform.position, AssignedGridCube.transform.position, Time.deltaTime * cardPlaySpeed);
        }
        else
            characterAnimator.SetBool("isMoving", false);
    }

    public void ChangeHealth(int amount, Character instigator)
    {
        if (isInvulnerable)
            return;

        healthPanel?.UpdateHealthPanel(health, amount);
        health -= amount;
        if (health <= 0)
            Die();
    }

    protected virtual void Die()
    {
        // Spawn death VFX
        bool isSimulation = InstancedCharacterSimulation == null;
        DeathVFX deathobj = Instantiate(deathVFX, transform.position, transform.rotation);
        deathobj.SetDeathVFXCharacterVisual(CharacterSpriteRenderer.sprite, isSimulation);

        // Experience drop on death
        for (int i = ExperienceAmountOnDeath; i > 0; i--)
        {
            Instantiate(ExperiencePoint, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Remove character from their list before destroying it to prevent null references.
        CharacterTeams._PlayerTeamCharacters.Remove(this);
        CharacterTeams._EnemyTeamCharacters.Remove(this);
        AssignedGridCube.RemoveCharacterOnGrid(this);
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
        InstancedCharacterSimulation.SetCharacterSimInfo(this, CharacterSpriteRenderer);
        InstancedCharacterSimulation.ChangeDestinationGrid(AssignedGridCube, cardPlaySpeed * cardSimulationSpeedModifier);
        isSimulationMarkedForDeath = false;
    }

    public virtual void ToggleCardPrevis(bool isShowingPrevis, int cardNumber, GameObject tilevisual, float angle)
    {
        if (cardPrevisBinder == null)
            cardPrevisBinder = new GameObject("CardPrevisBinder");

        // Cannot show card previs when simulation is marked for death
        if (isSimulationMarkedForDeath)
            isShowingPrevis = false;

        if (isShowingPrevis)
        {
            if (!ActiveCardPrevisTiles.Contains(tilevisual))
            {
                if (cardNumber < ActiveCardPrevisTiles.Count && ActiveCardPrevisTiles.Count > 0)
                {
                    // Remove tile previs at index
                    // This can happen when a character changes the card calculation of someone else
                    // E.g. Player is walking on the same path as an enemy, preventing the enemy from reaching it's destination
                    Destroy(cardPrevisBinder.transform.GetChild(cardNumber).gameObject);
                    ActiveCardPrevisTiles[cardNumber] = tilevisual;
                }
                else
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
