using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card_PointerInteraction : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private bool isInteractable = true;

    private readonly float cardDragRotSpeedModifier = 2;
    private readonly float cardAngleClamp = 30;
    private float pointerInputX, pointerInputY;

    // Store how much the mouse has moved since the last frame
    private Vector3 mouseDelta = Vector3.zero;

    // Used for the card to move to when not being dragged
    private readonly float cardReleaseSpeedModifier = 2000;
    private Vector2 startPosition = Vector2.zero;
    private Vector2 desiredPosition = Vector2.zero;
    private bool isBeingDragged = false;

    [SerializeField] private Animator animator;
    private Card card;
    private Card targetHoverOverCard;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private GameObject hoveredGameObject;
    private int originalSortingOrder;

    void Awake()
    {
        card = GetComponent<Card>();
        animator.SetBool("isInteractable", true);
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponent<Canvas>();
    }

    void Update()
    {
        // Continously lerp the card rotation back to it's original rotation
        pointerInputX = Mathf.Lerp(pointerInputX, 0, cardDragRotSpeedModifier * Time.deltaTime);
        pointerInputY = Mathf.Lerp(pointerInputY, 0, cardDragRotSpeedModifier * Time.deltaTime);

        // Clamp rotation values and then set the value to the card rotation
        pointerInputX = Mathf.Clamp(pointerInputX, -cardAngleClamp, cardAngleClamp);
        pointerInputY = Mathf.Clamp(pointerInputY, -cardAngleClamp, cardAngleClamp);
        transform.eulerAngles = new Vector3(pointerInputY, -pointerInputX, 0);

        // Card goes back to it's desired position when not being dragged
        if (!isBeingDragged)
        {
            MoveCardPosition();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Only call this method when the card is interactable
        if (!isInteractable)
            return;

        // Allow the card to read cards underneath is when beign dragged
        canvasGroup.blocksRaycasts = false;

        // Save the original sorting order to reset it later
        originalSortingOrder = canvas.sortingOrder;

        // Set higher sorting order so it's on top of other cards while dragging
        canvas.sortingOrder = 10;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Only call this method when the card is interactable
        if (!isInteractable)
        {
            FeedbackUI.FeedbackPanel.ShowFeedback(FeedbackPanelScriptableObject._FeedbackType.CannotInteractCardDuringEnemyTurn);
            return;
        }

        isBeingDragged = true;

        // Input for rotating the card when dragging
        mouseDelta = Input.mousePosition - transform.position;
        pointerInputX += mouseDelta.x * cardDragRotSpeedModifier;
        pointerInputY += mouseDelta.y * cardDragRotSpeedModifier;

        // Input for positioning the card when dragging
        transform.position = eventData.position;

        // Check if hovering over a valid target to swap
        hoveredGameObject = eventData.pointerEnter;

        ManageCardHoverStates();
    }

    private void ManageCardHoverStates()
    {
        // Only call this method when the card is interactable
        if (!isInteractable)
            return;

        // Show animation on the card you are hovering over to replace
        if (hoveredGameObject != null)
        {
            if (hoveredGameObject.GetComponentInParent<Card>() is Card hoveredCard)
            {
                ShowCardHover(hoveredCard);
            }
            else
            {
                // Hide previous hovered card when hovering over a gameobject that is not a card
                HideCardHover();
            }
        }
        else
        {
            // Hide previous hovered card when not hovering over any gameobject currently
            HideCardHover();
        }
    }

    private void ShowCardHover(Card targetCard)
    {
        // Don't show animation when target card and current card are both from hand
        // as well as target card is the same as this one
        if (card.ConnectedSocket == null &&
            targetCard.ConnectedSocket == null ||
            targetCard == card)
            return;

        // Check if the hovered card is changed compared to last time
        // Player can continously hover over Card components e.g. when hovering over their hand, so have to check if it's a different card
        if (targetCard != targetHoverOverCard)
        {
            // Update animations if player is hovering over a card
            if (targetHoverOverCard != null)
                targetHoverOverCard.cardAnimator.SetBool("isHoverReplace", false);
            targetCard.cardAnimator.SetBool("isHoverReplace", true);

            // Set reference for future checks to see if the player is hovering over a different card
            targetHoverOverCard = targetCard;
        }
    }

    private void HideCardHover()
    {
        // Hide animation on the card you were hovering over to replace
        if (targetHoverOverCard != null)
        {
            targetHoverOverCard.cardAnimator.SetBool("isHoverReplace", false);
            targetHoverOverCard = null;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Only call this method when the card is interactable
        if (!isInteractable)
            return;

        // Restore raycast blocking, so card is interactable again
        canvasGroup.blocksRaycasts = true;
        isBeingDragged = false;

        // Reset sorting order when dragging ends
        canvas.sortingOrder = originalSortingOrder;

        // Reset hover state
        if (targetHoverOverCard != null)
            targetHoverOverCard.cardAnimator.SetBool("isHoverReplace", false);

        // Replace card in a socket or swap places with another card
        if (hoveredGameObject != null)
        {
            if (hoveredGameObject.GetComponentInParent<CardSocket>() is CardSocket hoveredSocket)
            {
                // Remove card from socket if card is in play
                if (card.ConnectedSocket != null)
                    card.RemoveFromSocket();

                card.ConnectToSocket(hoveredSocket);
            }
            else if (hoveredGameObject.GetComponentInParent<Card>() is Card hoveredCard)
            {
                // If target card is slotted to a socket, swap it with dragged card
                if (hoveredCard.ConnectedSocket != null)
                {
                    SwapCards(hoveredCard);
                }
                // When dragging a card from a socket to one in your hand, swap values for the rest of this function's logic
                else if (hoveredCard.ConnectedSocket == null && card.ConnectedSocket != null)
                {
                    // Tuple to swap values
                    (card, hoveredCard) = (hoveredCard, card);
                    // Activate the function with swapped values
                    SwapCards(hoveredCard);
                    // Swap card value back to it's original value before previous tuple
                    card = hoveredCard;
                }
            }
        }
        // When releasing the drag, and the card doesn't hover over something valid
        else
        {
            AssignPosition();
        }

        // Updates the order in which cards are played
        PlayerUI.PlayerCircuitboard.UpdateCardsInPlay();
    }

    private void SwapCards(Card hoveredCard)
    {
        // Save target socket reference before the hoveredCard gets removed from it's socket
        CardSocket targetSocket = hoveredCard.ConnectedSocket;

        // If dragged card is connected to a socket, then swap socket places
        if (card.ConnectedSocket != null)
        {
            // Save target socket reference before dragged card gets removed from it's socket
            CardSocket startingSocket = card.ConnectedSocket;

            // Swap sockets
            // First remove them from their socket...
            hoveredCard.ConnectedCircuitboard.RemoveFromSocket(hoveredCard);
            card.ConnectedCircuitboard.RemoveFromSocket(card);

            // ...then place them inside their new socket
            hoveredCard.ConnectedCircuitboard.PlaceCardInSocket(hoveredCard, startingSocket);
            card.ConnectedCircuitboard.PlaceCardInSocket(card, targetSocket);
        }
        // If dragged card comes from hand, swap dragged card to the socket card
        else
        {
            // Add hovered card to hand, and remove it from it's socket
            hoveredCard.ConnectedCircuitboard.RemoveFromSocket(hoveredCard);
            PlayerUI.HandPanel.AssignCardToPanel(hoveredCard);

            // Add this card to hovered card's circuitboard socket
            PlayerUI.HandPanel.RemoveCardFromPanel(card, false);
            card.ConnectedCircuitboard.PlaceCardInSocket(card, targetSocket);

            // Update card positions in hand after the card swap
            PlayerUI.HandPanel.UpdateCardPositions();
        }
    }

    private void MoveCardPosition()
    {
        // Moves the card in a linear speed towards their desired position
        // Speed scales with screen resolution
        float scaledSpeed = cardReleaseSpeedModifier * HelperFunctions.GetResolutionScale();
        transform.position = Vector2.MoveTowards(transform.position, desiredPosition, Time.deltaTime * scaledSpeed);
    }

    public void AssignPosition(Vector2 position)
    {
        // Only assign position, if the position is different than before
        if (position != desiredPosition)
        {
            desiredPosition = position;
            AssignPosition();
        }
    }

    public void AssignPosition()
    {
        startPosition = transform.position;
    }

    public void SetInteractableState(bool interactState)
    {
        isInteractable = interactState;
        animator.SetBool("isInteractable", interactState);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // QQQQ TODO: Add that card shows tooltip on hover when no cards are beign dragged
        if (!eventData.dragging)
            animator.SetBool("isShowingTooltip", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // QQQQ TODO: Add that card hides tooltip if possible
        animator.SetBool("isShowingTooltip", false);
    }
}
