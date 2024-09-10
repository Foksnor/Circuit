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
    [SerializeField] private GameObject spawnObjectOnDeath = null;
    [SerializeField] private GameObject ExperiencePoint = null;
    [SerializeField] private int ExperienceAmountOnDeath = 3;
    public CharacterSimulation CharacterSimulation = null;
    public CharacterSimulation InstancedCharacterSimulation { get; private set; } = null;
    public bool isSimulation { get; protected set; } = false;
    
    [HideInInspector]
    public bool isSimulationMarkedForDeath { get; set; }
    public bool isPotentialKill { get; private set; }
    private GameObject cardPrevisBinder = null;
    private List<GameObject> ActiveCardPrevisTiles = new List<GameObject>();
    public int Health { get { return health; } set { health = value; } }
    [SerializeField] protected int health;
    [SerializeField] protected int maxHealth;
    [SerializeField] private HealthBar healthBar = null;
    protected bool isInvulnerable = false;
    private float cardPlaySpeed = 1;
    
    public float cardSimulationSpeedModifier { private set; get; } = 2;

    private void Awake()
    {
        maxHealth = health;
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
        // Cannot change destination if no destination is passed
        if (newDestinationInGrid == null)
            return;

        // Remove character reference on grid
        // First time this function has passed, the character hasn't been assigned a gridcube yet
        if (AssignedGridCube != null)
            AssignedGridCube.RemoveCharacterOnGrid(this);

        // Place character reference on the new grid
        newDestinationInGrid.SetCharacterOnGrid(this);
        AssignedGridCube = newDestinationInGrid;

        // cardplayspeed is used for things such as time required for the character to reach it's destination cube
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

    public bool MarkPotentialKillIfDamageWouldKill(int damageValue)
    {
        if (health <= damageValue)
        {
            isPotentialKill = true;
            return true;
        }
        return false;
    }

    public void RemovePotentialKillMark()
    {    
        isPotentialKill = false; 
    }

    public void ApplyStatus(_StatusType status)
    {
        switch (status)
        {
            default:
            case _StatusType.None:
                break;
            case _StatusType.Fire:
                break;
            case _StatusType.Shocked:
                break;
        }
    }

    public void ChangeHealth(int amount, Character instigator)
    {
        if (isInvulnerable)
            return;

        health -= amount;
        healthBar?.UpdateHealthBar(maxHealth, health, amount);
        if (health <= 0)
            Die(instigator);
    }

    protected virtual void Die(Character instigator)
    {
        // Spawn death VFX
        DeathVFX deathobj = Instantiate(deathVFX, transform.position, transform.rotation);
        deathobj.SetDeathVFXCharacterVisual(CharacterSpriteRenderer.sprite, isSimulation);

        // Spawn extra game objects on death
        if (spawnObjectOnDeath != null)
            Instantiate(spawnObjectOnDeath, transform.position, transform.rotation);        

        // Experience drop on death, if killed by player or environment
        if (instigator.TeamType == _TeamType.Player || instigator == null)
        {
            for (int i = ExperienceAmountOnDeath; i > 0; i--)
            {
                Instantiate(ExperiencePoint, transform.position, transform.rotation);
            }
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

    public virtual GameObject ToggleTilePrevis(bool isShowingPrevis, int cardNumber, GameObject tilevisual, float angle)
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

        return tilevisual;
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
