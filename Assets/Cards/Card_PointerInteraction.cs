using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Card), typeof(Button), typeof(CanvasGroup))]
public class Card_PointerInteraction : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private bool isInteractable = true;

    private const float cardDragRotSpeedModifier = 2;
    private const float cardAngleClamp = 30;
    private float pointerInputX, pointerInputY;

    // Store how much the mouse has moved since the last frame
    private Vector3 mouseDelta = Vector3.zero;

    // Used for the card to move to when not being dragged
    private const float cardReleaseSpeedModifier = 2000;
    private const float cardRotationSpeed = 1.5f;
    private Vector2 startPosition = Vector2.zero;
    private Vector2 desiredPosition = Vector2.zero;
    private float travelDuration = 0;
    private float curTime = 0;
    private bool isBeingDragged = false;

    // UI references
    private Card card;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private RectTransform rectTransform;
    private Button button;

    [SerializeField] private Transform visualRoot;
    [SerializeField] private Animator animator;
    private Card targetHoverOverCard;
    private GameObject hoveredGameObject;
    private int originalSortingOrder;

    void Awake()
    {
        card = GetComponent<Card>();
        animator.SetBool("isInteractable", true);
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
    }

    void Update()
    {
        // Rotate the card when dragging, allowing it to appear in 3d space
        RotateCardOnPointerInput();

        // Card goes back to it's desired position when not being dragged
        if (!isBeingDragged)
            MoveCardPosition();
    }

    private void RotateCardOnPointerInput()
    {
        // Continously lerp the card rotation back to it's original rotation
        pointerInputX = Mathf.Lerp(pointerInputX, 0, cardDragRotSpeedModifier * Time.deltaTime);
        pointerInputY = Mathf.Lerp(pointerInputY, 0, cardDragRotSpeedModifier * Time.deltaTime);

        // Clamp rotation values and then set the value to the card rotation
        pointerInputX = Mathf.Clamp(pointerInputX, -cardAngleClamp, cardAngleClamp);
        pointerInputY = Mathf.Clamp(pointerInputY, -cardAngleClamp, cardAngleClamp);

        Vector3 rotation = new(pointerInputY, -pointerInputX, transform.eulerAngles.z);
        AssignRotation(rotation);
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

        // Resets the rotation of the card
        // This allows cards that are being dragged from the hand panel to not have their hand panel rotation/fanning
        AssignRotation(Vector3.zero);
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
            else if (hoveredGameObject.GetComponentInParent<CardSocket>() is CardSocket hoveredSocket)
            {
                if (hoveredSocket.SlottedCard != null)
                    ShowCardHover(hoveredSocket.SlottedCard);
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
                // If dragging held card onto a socket that holds a different card, swap cards
                if (hoveredSocket.SlottedCard != null)
                {
                    SwapCards(hoveredSocket.SlottedCard);
                }
                // Otherwise you place the held card into an empty socket
                else
                {
                    // Remove card from socket if card is in play
                    if (card.ConnectedSocket != null)
                        card.RemoveFromSocket();

                    card.ConnectedCircuitboard.PlaceCardInSocket(card, hoveredSocket);

                    // Remove card from hand panel if applicable
                    PlayerUI.HandPanel.RemoveCardFromPanel(card, false);
                }
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
            // Sets a new start position for the card to return from
            startPosition = transform.position;

            // Reset card rotation
            AssignRotation(Vector3.zero);
            PlayerUI.HandPanel.FanCardsInPanel();
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
            PlayerUI.HandPanel.FanCardsInPanel();
        }
    }

    private void MoveCardPosition()
    {
        if (travelDuration > 0)
        {
            // Lerp movement
            curTime += Time.deltaTime / travelDuration;
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, desiredPosition, curTime);

            // Rotates the card towards the target position
            RotateCardTowardsTarget(desiredPosition);
        }
        else
        {
            // Moves the card in a linear speed towards their desired position
            // Speed scales with screen resolution
            float scaledSpeed = cardReleaseSpeedModifier * HelperFunctions.GetResolutionScale();
            rectTransform.anchoredPosition = Vector2.MoveTowards(rectTransform.anchoredPosition, desiredPosition, Time.deltaTime * scaledSpeed);
        }
    }

    public void AssignAnchoredPosition(Vector2 position, float travelTime = 0)
    {
        // Only assign position if it's different than before
        if (position != desiredPosition)
        {
            // Reset the movement timer
            curTime = 0;

            if (travelTime > 0)
            {
                // Set travel duration for smooth movement
                travelDuration = travelTime;

                // Cards that change zone should not be able to be interacted with
                isInteractable = false;
                GetComponent<Button>().interactable = false;
                CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;

                // Play card animation for cards with travelTime
                animator.speed = 1 / travelTime;
                animator.Play("A_Card_ChangeZone");
            }

            // Set desired position in local UI space
            desiredPosition = position;
            startPosition = rectTransform.anchoredPosition;
        }
    }

    public void AssignRotation(Vector3 rotation)
    {
        transform.eulerAngles = rotation;
    }

    private void RotateCardTowardsTarget(Vector2 targetPosition)
    {
        // Get the direction to the target
        Vector2 direction = targetPosition - (Vector2)visualRoot.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        // Smoothly interpolate the Z rotation
        float newZRotation = Mathf.LerpAngle(visualRoot.eulerAngles.z, targetAngle, curTime * cardRotationSpeed);

        // Apply only Z-axis rotation
        visualRoot.rotation = Quaternion.Euler(0, 0, newZRotation);
    }

    public void SetInteractableState(bool interactState)
    {
        isInteractable = interactState;
        animator.SetBool("isInteractable", interactState);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!eventData.dragging)
        {
            animator.SetBool("isShowingTooltip", true);
            card.ToggleCardTooltip(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator.SetBool("isShowingTooltip", false);
        card.ToggleCardTooltip(false);
    }
}
