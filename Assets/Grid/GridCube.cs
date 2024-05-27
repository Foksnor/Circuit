using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCube : MonoBehaviour
{
    [SerializeField] private TextMesh textMeshGridNumber;
    [SerializeField] private TextMesh textMeshCharacterRef;
    [SerializeField] private float highestElevation;
    [SerializeField] private float lowestElevation;
    [SerializeField] private SpriteRenderer floorSprite;
    [SerializeField] private Sprite[] randomFloorSprite;
    [HideInInspector] public float Height;
    [SerializeField] private GameObject visualElevation = null;
    private Character characterOnThisGrid = null;
    public GameObject PlayerMovementIndicator,
                        PlayerMovementArrowIndicator,
                        PlayerDamageIndicator,
                        EnemyMovementIndicator,
                        EnemyMovementArrowIndicator,
                        EnemyDamageIndicator;

    public Character CharacterOnThisGrid
    {
        get => characterOnThisGrid;
        set => characterOnThisGrid = value;
    }

    private void Awake()
    {
        floorSprite.sprite = randomFloorSprite[Random.Range(0, randomFloorSprite.Length)];
        floorSprite.transform.eulerAngles = new Vector3(0, 0, Random.Range(0, 3) * 90);
    }

    public void SetGridReferenceNumber(int numberInGrid)
    {
        textMeshGridNumber.text = numberInGrid.ToString();
    }

    public void SetHeight(Vector3 gridPosition)
    {
        Height = Random.Range(lowestElevation, highestElevation);
        visualElevation.transform.position = gridPosition + new Vector3(0, 0, Height);
    }

    public void SetIndicatorVisual(bool isShowingPrevis, CardScriptableObject cardScriptable, float angle)
    {
        if (isShowingPrevis)
        {
            if (cardScriptable.CardType == CardScriptableObject._CardType.Movement)
            {
                PlayerMovementIndicator.gameObject.SetActive(true);
                PlayerMovementArrowIndicator.gameObject.SetActive(true);
                PlayerMovementArrowIndicator.gameObject.transform.eulerAngles = new Vector3(0, 0, angle);
            }
            else if (cardScriptable.CardType == CardScriptableObject._CardType.Attack)
            {
                PlayerDamageIndicator.gameObject.SetActive(true);
            }
        }
        else
        {
            PlayerMovementIndicator.SetActive(false);
            PlayerMovementArrowIndicator.SetActive(false);
            PlayerDamageIndicator.SetActive(false);
            EnemyMovementIndicator.SetActive(false);
            EnemyMovementArrowIndicator.SetActive(false);
            EnemyDamageIndicator.SetActive(false);
        }
    }

    public void SetCharacterOnGrid(Character character)
    {
        CharacterOnThisGrid = character;
        textMeshCharacterRef.text = character.name;
    }

    public void RemoveCharacterOnGrid(Character character)
    {
        if (CharacterOnThisGrid == character)
        {
            CharacterOnThisGrid = null;
            textMeshCharacterRef.text = "";
        }
    }
}
