using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.Mody;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class GridCube : MonoBehaviour, ITurnSequenceTriggerable
{
    public Vector2 Position { get; private set; }
    public float Height { get; private set; }
    public _SurfaceType SurfaceType { get; private set; }
    public _StatusType StatusType { get; private set; }
    private GameObject activeSurfaceParticle = null;
    [SerializeField] private MeshRenderer gridMeshRenderer = null;
    [SerializeField] private Material gridMatWater = null, gridMatOil = null, gridMatBurning;
    private GameObject instancedTilePrevis = null;

    [SerializeField] private bool isStaircase = false;
    [SerializeField] private TextMesh textMeshGridNumber;
    [SerializeField] private TextMesh textMeshCharacterRef;
    [SerializeField] private float highestElevation;
    [SerializeField] private float lowestElevation;
    [SerializeField] private SpriteRenderer floorSprite;
    [SerializeField] private Sprite[] randomFloorSprite;
    [SerializeField] private GameObject visualElevation = null;
    public Character CharacterOnThisGrid { private set; get; }
    public GameObject CorpseOnThisGrid { private set; get; }
    public GameObject MovementPlayerIndicator, DamagePlayerIndicator,
                        MovementEnemyIndicator, DamageEnemyIndicator;

    private void Awake()
    {
        Position = transform.position;
        Height = transform.position.z;
        if (!isStaircase)
        {
            floorSprite.sprite = randomFloorSprite[Random.Range(0, randomFloorSprite.Length)];
            floorSprite.transform.eulerAngles = new Vector3(0, 0, Random.Range(0, 3) * 90);
        }
        SetGridReferenceNumber();
    }

    public void SetGridReferenceNumber()
    {
        Grid.GridPositions.GridCubes.Add(this);
        textMeshGridNumber.text = Position.x + "," + Position.y;
    }

    public void SetHeight(Vector3 gridPosition)
    {
        Height = Random.Range(lowestElevation, highestElevation);
        visualElevation.transform.position = gridPosition + new Vector3(0, 0, Height);
    }

    public GameObject GetIndicatorVisual(Character targetCharacter, CardScriptableObject cardScriptable)
    {
        GameObject indicatorvisual;

        switch (cardScriptable.CardType)
        {
            default:
            case _CardType.Movement:
                // Movement Tile Previs turned off for now. See how it plays without it
                if (targetCharacter.TeamType == _TeamType.Player)
                    indicatorvisual = MovementPlayerIndicator;
                else
                    indicatorvisual = MovementEnemyIndicator;
                break;
            case _CardType.Attack:
                if (targetCharacter.TeamType == _TeamType.Player)
                    indicatorvisual = DamagePlayerIndicator;
                else
                    indicatorvisual = DamageEnemyIndicator;
                break;
        }
        return indicatorvisual;
    }

    public void SetCharacterReferenceOnGrid(Character character)
    {
        CharacterOnThisGrid = character;
        textMeshCharacterRef.text = character.name;

        // Transfer gridstatus effect to character
        character.SetStatus(StatusType, true);
    }

    public void RemoveCharacterReferenceOnGrid(Character character)
    {
        if (CharacterOnThisGrid == character)
        {
            CharacterOnThisGrid = null;
            if (textMeshCharacterRef != null)
                textMeshCharacterRef.text = "";
        }
    }

    public void KillCharacterOnGrid()
    {
        CharacterOnThisGrid.KillSelf();
    }

    public void SetCorpseReferenceOnGrid(GameObject corpse)
    {
        CorpseOnThisGrid = corpse;
    }

    public void ToggleSurface(Character instigator, _SurfaceType surfaceEffect)
    {
        gridMeshRenderer.enabled = false;

        switch (surfaceEffect)
        {
            default:
            case _SurfaceType.None:
                break;
            case _SurfaceType.Water:
                gridMeshRenderer.enabled = true;
                gridMeshRenderer.material = gridMatWater;

                // Dousing fire, remove surface
                if (SurfaceType == _SurfaceType.Burning)
                {
                    PlaceSurfaceParticle(GlobalSettings.DousedSurface, activeSurfaceParticle.transform.position);
                    gridMeshRenderer.enabled = false;
                }
                break;
            case _SurfaceType.Oil:
                gridMeshRenderer.enabled = true;
                gridMeshRenderer.material = gridMatOil;
                break;
            case _SurfaceType.Burning:
                StatusType = _StatusType.Fire;
                gridMeshRenderer.enabled = true;
                gridMeshRenderer.material = gridMatBurning;
                Vector3 randomOffset = new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(-0.4f, 0.4f));
                PlaceSurfaceParticle(GlobalSettings.BurningSurface, transform.position + randomOffset);
                Instantiate(GlobalSettings.FireChain, transform);
                SpreadStatus(instigator, _SurfaceType.Oil, _StatusType.Fire);

                // Dousing fire, remove surface
                if (SurfaceType == _SurfaceType.Burning)
                {
                    PlaceSurfaceParticle(GlobalSettings.DousedSurface, activeSurfaceParticle.transform.position);
                    gridMeshRenderer.enabled = false;
                }
                break;
            case _SurfaceType.Electrified:
                StatusType = _StatusType.Shocked;
                gridMeshRenderer.enabled = true;
                gridMeshRenderer.material = gridMatWater;
                Instantiate(GlobalSettings.ShockChain, transform);
                SpreadStatus(instigator, _SurfaceType.Water, _StatusType.Shocked);
                break;
        }
        SurfaceType = surfaceEffect;
    }

    public void ToggleStatus(Character instigator, _StatusType status, bool isCausedByAttack)
    {
        // Return when this status is already active
        if (StatusType == status)
            return;

        // Updates new status effects when the end of turn triggers
        if (!TurnSequence.TransitionTurns.TurnSequenceTriggerables.Contains(this))
            TurnSequence.TransitionTurns.TurnSequenceTriggerables.Add(this);

        switch (status)
        {
            default:
            case _StatusType.None:
                break;
            case _StatusType.Fire:
                if (isCausedByAttack)
                    Instantiate(GlobalSettings.FireHit, transform);

                // Ignite oil on this grid
                if (SurfaceType == _SurfaceType.Oil)
                    ToggleSurface(instigator, _SurfaceType.Burning);
                break;
            case _StatusType.Shocked:
                if (isCausedByAttack)
                    Instantiate(GlobalSettings.ShockHit, transform);

                // Shock water on this grid
                if (SurfaceType == _SurfaceType.Water)
                    ToggleSurface(instigator, _SurfaceType.Electrified);
                break;
        }

        if (instigator != null)
        {
            // Apply surface status to the characters on this grid
            CharacterOnThisGrid?.SetStatus(status, true);
        }
    }

    private void SpreadStatus(Character instigator, _SurfaceType requiredSurface, _StatusType statusToSpread)
    {
        List<GridCube> vicinityCubes = new();
        vicinityCubes.AddRange(HelperFunctions.GetVicinityGridCubes(this, 1));
        for (int i = 0; i < vicinityCubes.Count; i++)
            if (vicinityCubes[i].SurfaceType == requiredSurface)
                vicinityCubes[i].ToggleStatus(instigator, statusToSpread, false);
    }

    private void UpdateStatusEffects()
    {
        // Remove itself so it no longer triggers subsequently
        HelperFunctions.RemoveFromTurnTrigger(this);

        switch (SurfaceType)
        {
            case _SurfaceType.None:
            case _SurfaceType.Water:
            case _SurfaceType.Oil:
                // Remove fire and other status effects at the end of turn when there is no surface present
                StatusType = _StatusType.None;
                Destroy(instancedTilePrevis);
                break;
            case _SurfaceType.Burning:
                /*
                surfaceDuration--;
                if (surfaceDuration <= 0)
                {
                    ToggleSurface(null, _SurfaceType.None);
                }
                // Add end of turn trigger so the burning patch removes itself next turn
                // Updates new status effects when the end of turn triggers
                    HelperFunctions.AddToTurnTrigger(this);
                */
                break;
            case _SurfaceType.Electrified:
                break;
        }
    }

    private void PlaceSurfaceParticle(GameObject particle, Vector3 position)
    {
        if (activeSurfaceParticle != null)
            Destroy(activeSurfaceParticle);
        activeSurfaceParticle = Instantiate(particle, position, transform.rotation, transform);
    }

    public void ToggleDebugText()
    {
        textMeshGridNumber.gameObject.SetActive(!textMeshGridNumber.gameObject.activeSelf);
        textMeshCharacterRef.gameObject.SetActive(!textMeshCharacterRef.gameObject.activeSelf);
    }

    public void UpdateGridCubeToSaveState(_StatusType status, _SurfaceType surface)
    {
        ToggleStatus(null, status, false);
        ToggleSurface(null, surface);
    }

    // ITurnSequenceTriggerable interface
    public void OnStartPlayerTurn()
    {
        return;
    }

    public void OnStartEnemyTurn()
    {
        return;
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
        UpdateStatusEffects();
    }
}
