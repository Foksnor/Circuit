using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card_PointerInteraction : MonoBehaviour, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private bool isInteractable = true;

    private readonly float cardDragRotSpeedModifier = 2;
    private readonly float cardAngleClamp = 30;
    private float pointerInputX, pointerInputY;
    private float pointerOffsetY = 60;

    // Store how much the mouse has moved since the last frame
    private Vector3 mouseDelta = Vector3.zero;
    private Vector3 lastMousePosition = Vector3.zero;

    // Used for the card to move to when not being dragged
    private readonly float cardReleaseSpeedModifier = 10;
    private Vector2 desiredPosition = Vector2.zero;
    private bool isBeingDragged = false;

    [SerializeField] private Animator animator;
    private Card card;

    void Awake()
    {
        card = GetComponent<Card>();
    }

    void Update()
    {
        mouseDelta = Input.mousePosition - lastMousePosition;
        lastMousePosition = Input.mousePosition;

        // Continously lerp the card rotation back to it's original rotation
        pointerInputX = Mathf.Lerp(pointerInputX, 0, cardDragRotSpeedModifier * Time.deltaTime);
        pointerInputY = Mathf.Lerp(pointerInputY, 0, cardDragRotSpeedModifier * Time.deltaTime);

        // Clamp rotation values and then set the value to the card rotation
        pointerInputX = Mathf.Clamp(pointerInputX, -cardAngleClamp, cardAngleClamp);
        pointerInputY = Mathf.Clamp(pointerInputY, -cardAngleClamp, cardAngleClamp);
        transform.eulerAngles = new Vector3(pointerInputY, -pointerInputX, 0);

        // Card goes back to it's disered position when not being dragged
        if (!isBeingDragged)
            transform.position = Vector2.Lerp(transform.position, desiredPosition, cardReleaseSpeedModifier * Time.deltaTime);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isInteractable)
        {
            FeedbackUI.FeedbackPanel.ShowFeedback(FeedbackPanelScriptableObject._FeedbackType.CannotInteractCardDuringEnemyTurn);
            return;
        }

        isBeingDragged = true;

        // Input for positioning the card when dragging
        Vector2 tempVector = eventData.position + Vector2.up * pointerOffsetY;
        transform.position = tempVector;

        // Input for rotating the card when dragging
        pointerInputX += mouseDelta.x * cardDragRotSpeedModifier;
        pointerInputY += mouseDelta.y * cardDragRotSpeedModifier;
    }

    // Called by EventTrigger component
    public void OnDragStop()
    {
        isBeingDragged = false;

        // Replace card in circuit when user stopping dragging the card at a card in the circuit or empty socket
        if (CardSelect.SelectedSocket != null)
        {
            Card cardToReplace = CardSelect.SelectedSocket.SlottedCard;
            if (card.isInHand && cardToReplace != card)
                card.ConnectedCircuitboard.ReplaceCardInCircuit(card, cardToReplace);
        }
    }

    public void AssignPosition(Vector2 position)
    {
        desiredPosition = position;
    }

    public void ToggleInteractableState(bool interactState)
    {
        isInteractable = interactState;
        animator.SetBool("isInteractable", interactState);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        CardSelect.SelectedSocket = card.connectedSocket;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CardSelect.SelectedSocket == card.connectedSocket)
            CardSelect.SelectedSocket = null;
    }
}
