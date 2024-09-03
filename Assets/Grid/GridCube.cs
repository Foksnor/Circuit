using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GridCube : MonoBehaviour
{
    public Vector2 Position { get; private set; }
    public float Height { get; private set; }
    private _SurfaceEffect SurfaceEffect;
    private _StatusEffect SurfaceStatus;
    [SerializeField] private GameObject waterVisual = null, oilVisual = null;
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

    public void ToggleSurfaceEffect(_SurfaceEffect surfaceEffect)
    {
        SurfaceEffect = surfaceEffect;
        switch (surfaceEffect)
        {
            default:
            case _SurfaceEffect.None:
                break;
            case _SurfaceEffect.Water:
                waterVisual.SetActive(true);
                oilVisual.SetActive(false);
                break;
            case _SurfaceEffect.Oil:
                waterVisual.SetActive(false);
                oilVisual.SetActive(true);
                break;
            case _SurfaceEffect.Burning:
                // QQQ TODO: pass this effect to other oil surfaces in the vicinity
                break;
        }
    }

    public void ToggleSurfaceStatus(_StatusEffect surfaceStatus)
    {
        SurfaceStatus = surfaceStatus;
        switch (surfaceStatus)
        {
            default:
            case _StatusEffect.None:
                break;
            case _StatusEffect.Fire:
                Instantiate(GlobalSettings.FireEffectObject, transform);

                // Ignite oil on this grid
                if (SurfaceEffect == _SurfaceEffect.Oil)
                    ToggleSurfaceEffect(_SurfaceEffect.Burning);
                break;
            case _StatusEffect.Shocked:
                Instantiate(GlobalSettings.ElectricEffectObject, transform);
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

    private void OnGUI()
    {
        
    }
}
