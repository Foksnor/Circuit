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
    private _SurfaceType SurfaceType;
    private _StatusType StatusType;
    private int surfaceDuration = 2;
    [SerializeField] private MeshRenderer gridMeshRenderer = null;
    [SerializeField] private Material gridMatWater = null, gridMatOil = null, gridMatBurning;
    [SerializeField] private SpriteRenderer tileStatusEffectPrevis = null;
    private GameObject instancedTilePrevis = null;

    [SerializeField] private bool isStaircase = false;
    [SerializeField] private TextMesh textMeshGridNumber;
    [SerializeField] private TextMesh textMeshCharacterRef;
    [SerializeField] private TextMesh textMeshSimulationRef;
    [SerializeField] private float highestElevation;
    [SerializeField] private float lowestElevation;
    [SerializeField] private SpriteRenderer floorSprite;
    [SerializeField] private Sprite[] randomFloorSprite;
    [SerializeField] private GameObject visualElevation = null;
    public Character CharacterOnThisGrid { private set; get; }
    public Character SimulationOnThisGrid { private set; get; }
    public GameObject MovementPlayerIndicator, DamagePlayerIndicator,
                        MovementEnemyIndicator, DamageEnemyIndicator;

    // Used for determining which character has priority to move on a gridcube
    // E.g. Multiple characters that want to move on the same grid at the same time
    private Dictionary<int, Character> charactersMovementActionNumber = new();

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
        GridPositions._GridCubes.Add(this);
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
            case CardScriptableObject._CardType.Movement:
                if (targetCharacter.TeamType == Character._TeamType.Player)
                    indicatorvisual = MovementPlayerIndicator;
                else
                    indicatorvisual = MovementEnemyIndicator;
                break;
            case CardScriptableObject._CardType.Attack:
                if (targetCharacter.TeamType == Character._TeamType.Player)
                    indicatorvisual = DamagePlayerIndicator;
                else
                    indicatorvisual = DamageEnemyIndicator;
                break;
        }
        return indicatorvisual;
    }

    public void SetCharacterOnGrid(Character character)
    {
        if (character.CharacterSimulation != null)
        {
            CharacterOnThisGrid = character;
            textMeshCharacterRef.text = character.name;
        }
        else
        {
            SimulationOnThisGrid = character;
            textMeshSimulationRef.text = character.name;
        }

        // Transfer gridstatus effect to character
        character.SetStatus(StatusType, true);
    }

    public void RemoveCharacterOnGrid(Character character)
    {
        if (CharacterOnThisGrid == character)
        {
            CharacterOnThisGrid = null;
            if (textMeshCharacterRef != null)
                textMeshCharacterRef.text = "";
        }
        else if (SimulationOnThisGrid == character)
        {
            SimulationOnThisGrid = null;
            if (textMeshSimulationRef != null)
                textMeshSimulationRef.text = "";
        }
    }

    public void ToggleSurface(Character instigator, _SurfaceType surfaceEffect)
    {
        SurfaceType = surfaceEffect;
        gridMeshRenderer.enabled = false;

        switch (surfaceEffect)
        {
            default:
            case _SurfaceType.None:
                break;
            case _SurfaceType.Water:
                gridMeshRenderer.enabled = true;
                gridMeshRenderer.material = gridMatWater;
                if (instigator.isSimulation)
                {
                    // Only set previs tile if not already initialized
                    if (instancedTilePrevis == null)
                    {
                        surfaceDuration = 2;
                        tileStatusEffectPrevis.sprite = GlobalSettings.ShockStatus.Icon;
                        Character simulationOwner = instigator.GetComponent<CharacterSimulation>().OwnerOfThisSimulation;
                        instancedTilePrevis = simulationOwner.ToggleTilePrevis(true, int.MaxValue, tileStatusEffectPrevis.gameObject, 90);
                        SpreadStatus(instigator, _SurfaceType.Water, _StatusType.Shocked);
                    }
                }
                break;
            case _SurfaceType.Oil:
                gridMeshRenderer.enabled = true;
                gridMeshRenderer.material = gridMatOil;
                if (instigator.isSimulation)
                {
                    // Only set previs tile if not already initialized
                    if (instancedTilePrevis == null)
                    {
                        surfaceDuration = 2;
                        tileStatusEffectPrevis.sprite = GlobalSettings.FireStatus.Icon;
                        Character simulationOwner = instigator.GetComponent<CharacterSimulation>().OwnerOfThisSimulation;
                        instancedTilePrevis = simulationOwner.ToggleTilePrevis(true, int.MaxValue, tileStatusEffectPrevis.gameObject, 90);
                        SpreadStatus(instigator, _SurfaceType.Oil, _StatusType.Fire);
                    }
                }
                break;
            case _SurfaceType.Burning:
                gridMeshRenderer.enabled = true;
                gridMeshRenderer.material = gridMatBurning;
                Instantiate(GlobalSettings.BurningEffectObject, transform);
                SpreadStatus(instigator, _SurfaceType.Oil, _StatusType.Fire);
                break;
            case _SurfaceType.Electrified:
                gridMeshRenderer.enabled = true;
                gridMeshRenderer.material = gridMatWater;
                Instantiate(GlobalSettings.ElectrifiedEffectObject, transform);
                SpreadStatus(instigator, _SurfaceType.Water, _StatusType.Shocked);
                break;
        }
    }

    public void ToggleStatus(Character instigator, _StatusType status, bool isCausedByAttack)
    {
        // Return when this status is already active
        if (StatusType == status)
            return;

        // Updates new status effects when the end of turn triggers
        if (!TurnSequence.TurnSequenceTriggerables.Contains(this))
            TurnSequence.TurnSequenceTriggerables.Add(this);

        StatusType = status;
        switch (status)
        {
            default:
            case _StatusType.None:
                break;
            case _StatusType.Fire:
                if (isCausedByAttack)
                    Instantiate(GlobalSettings.FireEffectObject, transform);

                // Ignite oil on this grid
                if (SurfaceType == _SurfaceType.Oil)
                    if (instigator.isSimulation)
                        ToggleSurface(instigator, _SurfaceType.Oil);
                    else
                        ToggleSurface(instigator, _SurfaceType.Burning);
                break;
            case _StatusType.Shocked:
                if (isCausedByAttack)
                    Instantiate(GlobalSettings.ShockEffectObject, transform);

                // Shock water on this grid
                if (SurfaceType == _SurfaceType.Water)
                    if (instigator.isSimulation)
                        ToggleSurface(instigator, _SurfaceType.Water);
                    else
                        ToggleSurface(instigator, _SurfaceType.Electrified);
                break;
        }

        // Apply surface status to the characters on this grid
        if (instigator.isSimulation)
            SimulationOnThisGrid?.SetStatus(status, true);
        else
            CharacterOnThisGrid?.SetStatus(status, true);
    }

    private void SpreadStatus(Character instigator, _SurfaceType requiredSurface, _StatusType statusToSpread)
    {
        List<GridCube> vicinityCubes = new();
        vicinityCubes.AddRange(HelperFunctions.GetVicinityGridCubes(this, 1));
        for (int i = 0; i < vicinityCubes.Count; i++)
            if (vicinityCubes[i].SurfaceType == requiredSurface)
                vicinityCubes[i].ToggleStatus(instigator, statusToSpread, false);
    }

    private void UpdateStatusEffects(bool isSimulation)
    {
        // Remove itself so it no longer triggers subsequently
        if (!isSimulation)
            TurnSequence.TurnSequenceTriggerables.Remove(this);

        switch (SurfaceType)
        {
            case _SurfaceType.None:
            case _SurfaceType.Water:
            case _SurfaceType.Oil:
                // Remove fire and other status effects at the end of turn when there is no surface present
                StatusType = _StatusType.None;
                break;
            case _SurfaceType.Burning:
            case _SurfaceType.Electrified:
                if (!isSimulation)
                {
                    surfaceDuration--;
                    if (surfaceDuration <= 0)
                    {
                        ToggleSurface(null, _SurfaceType.None);
                    }
                    // Add end of turn trigger so the burning patch removes itself next turn
                    // Updates new status effects when the end of turn triggers
                    if (!TurnSequence.TurnSequenceTriggerables.Contains(this))
                        TurnSequence.TurnSequenceTriggerables.Add(this);
                }
                break;
        }
    }

    public bool GetCharacterMovementPriority(Character character, int actionNumber)
    {
        // If the passed 'movement action number' is unique, add their character reference and save the number
        if (!charactersMovementActionNumber.ContainsKey(actionNumber))
            charactersMovementActionNumber.Add(actionNumber, character);

        // Check if the accepted action number is from the same character. Allows characters to move on gridcubes they are previewing
        // Other characters should not be able to move on the same grid during the same action number
        if (charactersMovementActionNumber.FirstOrDefault(x => x.Value == character).Key == actionNumber)
            return true;
        return false;
    }

    public void ResetCharacterMovementPriority()
    {
        charactersMovementActionNumber.Clear();
    }

    public void ToggleDebugText()
    {
        textMeshGridNumber.gameObject.SetActive(!textMeshGridNumber.gameObject.activeSelf);
        textMeshCharacterRef.gameObject.SetActive(!textMeshCharacterRef.gameObject.activeSelf);
        textMeshSimulationRef.gameObject.SetActive(!textMeshSimulationRef.gameObject.activeSelf);
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

    public void OnEndTurn()
    {
        UpdateStatusEffects(true);
    }

    public void OnStartPlayerSimulationTurn()
    {
        return;
    }

    public void OnStartEnemySimulationTurn()
    {
        return;
    }

    public void OnEndSimulationTurn()
    {
        UpdateStatusEffects(false);
    }
}
