using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCube : MonoBehaviour
{
    public Vector2 Position { get; private set; }
    public float Height { get; private set; }
    [SerializeField] private bool enableTileVisuals = true;
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

    private void Awake()
    {
        Position = transform.position;
        Height = transform.position.z;
        if (enableTileVisuals)
        {
            floorSprite.sprite = randomFloorSprite[Random.Range(0, randomFloorSprite.Length)];
            floorSprite.transform.eulerAngles = new Vector3(0, 0, Random.Range(0, 3) * 90);
        }
        else
            floorSprite.gameObject.SetActive(false);
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

    public void ToggleDebugText()
    {
        textMeshGridNumber.gameObject.SetActive(!textMeshGridNumber.gameObject.activeSelf);
        textMeshCharacterRef.gameObject.SetActive(!textMeshCharacterRef.gameObject.activeSelf);
        textMeshSimulationRef.gameObject.SetActive(!textMeshSimulationRef.gameObject.activeSelf);
    }
}
