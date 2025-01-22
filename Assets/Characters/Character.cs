using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class Character : MonoBehaviour, IDamageable, ITurnSequenceTriggerable
{
    public _TeamType TeamType;
    public enum _TeamType { Player, Enemy, Neutral };
    public _BrainType BrainType;
    public enum _BrainType { Independent, Listener };
    public CircuitBoard CircuitBoard;
    public GridCube AssignedGridCube { private set; get; }
    [SerializeField] public SpriteRenderer CharacterSpriteRenderer = null;
    [SerializeField] protected Animator characterAnimator = null;

    // Status Effects
    [SerializeField] private StatusBar statusBar = null;

    // Health
    public int Health { get { return health; } set { health = value; } }
    [SerializeField] protected int health;
    [SerializeField] protected int maxHealth;
    [SerializeField] private HealthBar healthBar = null;
    [SerializeField] private CharacterHitFlash hitFlashComponent = null;
    protected bool isInvulnerable = false;
    public _StatusType StatusType { get; private set; }

    // Death
    [SerializeField] private DeathVFX deathVFX = null;
    [SerializeField] private GameObject corpseObject = null;
    [SerializeField] private GameObject spawnObjectOnDeath = null;
    [SerializeField] private GameObject ExperiencePoint = null;
    [SerializeField] private int ExperienceAmountOnDeath = 3;

    // Cards
    private GameObject cardPrevisBinder = null;
    private List<GameObject> ActiveCardPrevisTiles = new List<GameObject>();
    private float cardPlaySpeed = 1;

    private void Awake()
    {
        maxHealth = health;
    }

    public void PlaySummonAnimations()
    {
        characterAnimator.Play("A_Spawn");
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
            AssignedGridCube.RemoveCharacterReferenceOnGrid(this);

        // Place character reference on the new grid
        AssignedGridCube = newDestinationInGrid;
        AssignedGridCube.SetCharacterReferenceOnGrid(this);

        // cardplayspeed is used for things such as time required for the character to reach it's destination cube
        cardPlaySpeed = 1 / speedModifier;
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

    public void SetStatus(_StatusType status, bool isSetDuringOwnTurn)
    {
        statusBar.ToggleStatusWidget(status, true);
        HelperFunctions.AddToTurnTrigger(this);

        switch (status)
        {
            default:
            case _StatusType.None:
                statusBar.ResetStatusWidget();
                break;
            case _StatusType.Fire:
                if (isSetDuringOwnTurn && StatusType != status)
                    BurnCharacter();
                break;
            case _StatusType.Shocked:
                ShockCharacter();
                break;
        }
        StatusType = status;
    }

    public void SpawnAtHealth(int amount)
    {
        // Calculate health loss
        amount = maxHealth - amount;

        // Subtract remainder
        SubtractHealth(amount, null);
    }

    public void SubtractHealth(int amount, Character instigator)
    {
        if (isInvulnerable)
            return;

        health -= amount;
        healthBar?.UpdateHealthBar(maxHealth, health, amount);
        if (amount > 0)
            hitFlashComponent.PlayHitFlash();
        if (health <= 0)
            Die(instigator);
    }

    public void KillSelf()
    {
        Die(null);
    }

    protected virtual void Die(Character instigator)
    {
        // Spawns corpse on death location
        SpawnerFunctions.Instance.SpawnCorpse(corpseObject, AssignedGridCube);

        // Spawn death VFX
        DeathVFX deathobj = Instantiate(deathVFX, transform.position, transform.rotation);
        deathobj.SetDeathVFXCharacterVisual(CharacterSpriteRenderer.sprite);

        // Spawn extra game objects on death
        if (spawnObjectOnDeath != null)
            Instantiate(spawnObjectOnDeath, transform.position, transform.rotation);        

        // Experience drop on death, if killed by player or environment
        if (instigator != null)
            if (instigator.TeamType == _TeamType.Player || instigator == null)
            {
                for (int i = ExperienceAmountOnDeath; i > 0; i--)
                {
                    Instantiate(ExperiencePoint, transform.position, transform.rotation);
                }
            }

        // If this player is the player team king...
        if (this == Teams.CharacterTeams.PlayerTeamKing)
        {
            // ...Subtract one life, and respawn if possible
            int curlives = PlayerUI.LifePanel.AdjustLifeCount(-1);
            if (curlives > 0)
            {
                Teams.CharacterTeams.RemovePlayerKing();
                FeedbackUI.FeedbackPanel.ShowFeedback(FeedbackPanelScriptableObject._FeedbackType.LifeLost);
                SpawnerFunctions.Instance.SpawnPlayerAtEndTurn(AssignedGridCube.Position);
            }
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        HelperFunctions.RemoveFromTurnTrigger(this);
        // Remove character from their list before destroying it to prevent null references.
        Teams.CharacterTeams.PlayerTeamCharacters.Remove(this);
        Teams.CharacterTeams.EnemyTeamCharacters.Remove(this);
        AssignedGridCube.RemoveCharacterReferenceOnGrid(this);
    }

    public virtual void RefreshCharacterSimulation()
    {
        RemoveCardPrevis();
    }

    public virtual GameObject ToggleTilePrevis(bool isShowingPrevis, GameObject tilevisual, float angle)
    {
        if (cardPrevisBinder == null)
            cardPrevisBinder = new GameObject("CardPrevisBinder");

        // When card has no tile previs associated
        if (tilevisual == null)
            return null;

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

        return tilevisual;
    }

    private void RemoveCardPrevis()
    {
        Destroy(cardPrevisBinder);
        ActiveCardPrevisTiles.Clear();
    }

    public bool IsCharacterRelatedToMe(Character comparisonCharacter)
    {
        if (comparisonCharacter.gameObject == gameObject)
            return true;
        return false;
    }

    // ITurnSequenceTriggerable interface
    public void OnStartPlayerTurn()
    {
        // Status effects
        if (TeamType == _TeamType.Player)
            switch(StatusType)
            {
                case _StatusType.Fire:
                    BurnCharacter();
                    break;
                case _StatusType.Shocked:
                    ShockCharacter();
                    break;
            }
    }

    public void OnStartEnemyTurn()
    {
        // Status effects
        if (TeamType == _TeamType.Enemy)
            switch (StatusType)
            {
                case _StatusType.Fire:
                    BurnCharacter();
                    break;
                case _StatusType.Shocked:
                    ShockCharacter();
                    break;
            }
    }

    public void OnUpkeep()
    {
        return;
    }

    public void OnEndstep()
    {
        return;
    }

    public void OnEndTurn()
    {
        return;
    }

    private void BurnCharacter()
    {
        // Burn character
        Instantiate(GlobalSettings.FireHit, transform.position, transform.rotation);
        SubtractHealth(GlobalSettings.FireStatus.Damage, null);

        // Remove status after burn if character is no longer on a burning surface
        if (AssignedGridCube.SurfaceType != _SurfaceType.Burning)
            StatusType = _StatusType.None;
    }

    private void ShockCharacter()
    {

    }
}
