using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Animator))]
public class RewardScreen : MonoBehaviour
{
    [SerializeField] private Card cardReward;
    [SerializeField] private RectTransform targetContainer;
    private int amountOfRewardOptions = 3;
    private float cardRewardScale = 1.25f;

    private List<CardScriptableObject> basicCardRewardPool = new List<CardScriptableObject>();
    private List<CardScriptableObject> rareCardRewardPool = new List<CardScriptableObject>();
    private List<CardScriptableObject> epicCardRewardPool = new List<CardScriptableObject>();
    private List<CardScriptableObject> tempBasicCardPool = new List<CardScriptableObject>();
    private List<CardScriptableObject> tempRareCardPool = new List<CardScriptableObject>();
    private List<CardScriptableObject> tempEpicCardPool = new List<CardScriptableObject>();
    private List<CardReward> currentCardRewardOptions = new List<CardReward>();

    // Odds gets increased each time the player does not get a card of this type
    private float chanceForRareCard;
    [SerializeField] private float defaultChanceForRareCard;
    [SerializeField] private float increasedLuckForRareCard;
    private float chanceForEpicCard;
    [SerializeField] private float defaultChanceForEpicCard;
    [SerializeField] private float increasedLuckForEpicCard;

    private Animator animator = null;
    [SerializeField] private RewardScreenRayPopulator rewardScreenRayPopulator = null;

    private void Awake()
    {
        PlayerUI.RewardScreen = this;
        chanceForRareCard = defaultChanceForRareCard;
        chanceForEpicCard = defaultChanceForEpicCard;
        animator = GetComponent<Animator>();
    }

    public void GiveCardRewardOptions(CardScriptableObject[] possibleNewCardRewards)
    {
        // Shows background rays
        animator.SetBool("isShowing", true);
        rewardScreenRayPopulator.PopulateRays();

        // Add the cards to the possible pool for the future level ups, avoiding duplicates
        for (int i = 0; i < possibleNewCardRewards.Length; i++)
        {
            switch (possibleNewCardRewards[i].CardRarity)
            {
                case _CardRarity.Basic:
                    if (!basicCardRewardPool.Contains(possibleNewCardRewards[i]))
                        basicCardRewardPool.Add(possibleNewCardRewards[i]);
                    break;
                case _CardRarity.Rare:
                    if (!rareCardRewardPool.Contains(possibleNewCardRewards[i]))
                        rareCardRewardPool.Add(possibleNewCardRewards[i]);
                    break;
                case _CardRarity.Epic:
                    if (!epicCardRewardPool.Contains(possibleNewCardRewards[i]))
                        epicCardRewardPool.Add(possibleNewCardRewards[i]);
                    break;
            }
        }

        // Add the reward pools to temporary versions
        // This is used so we can remove an entry from the temp pool to avoid selecting the same card multiple times during the reward screen
        tempBasicCardPool.AddRange(basicCardRewardPool);
        tempRareCardPool.AddRange(rareCardRewardPool);
        tempEpicCardPool.AddRange(epicCardRewardPool);

        // Pick random cards to be in included for this reward screen
        List<Card> currentCardOptions = new();
        for (int i = 0; i < amountOfRewardOptions; i++)
        {
            // Choose a list at random
            float rng = Random.Range(0, 100);
            List<CardScriptableObject> chosenList;
            if (rng < chanceForEpicCard && tempEpicCardPool.Count > 0)
            {
                chosenList = tempEpicCardPool;
                chanceForEpicCard = defaultChanceForEpicCard;
            }
            else if (rng < chanceForRareCard && tempRareCardPool.Count > 0)
            {
                chosenList = tempRareCardPool;
                chanceForRareCard = defaultChanceForRareCard;
            }
            else
            {
                chosenList = tempBasicCardPool;
                chanceForEpicCard += increasedLuckForEpicCard;
                chanceForRareCard += increasedLuckForRareCard;
            }

            // Choose a random card from the chosen list
            int cardNumber = Random.Range(0, chosenList.Count);
            CardScriptableObject cardScript = chosenList[cardNumber];

            // Populate that card in the targetContainer, and remove it from the available options
            chosenList.Remove(cardScript);
            Card cardOption = Instantiate(cardReward, targetContainer.transform.position, transform.rotation, targetContainer.transform);
            cardOption.SetCardInfo(cardScript, PlayerUI.PlayerCircuitboard, true);
            cardOption.SetCardScale(cardRewardScale);
            currentCardOptions.Add(cardOption);

            // Used for accessing the other cards not chosen during the reward screen
            CardReward cardOptionReward = cardOption.AddComponent<CardReward>();
            currentCardRewardOptions.Add(cardOptionReward);

            // Update the layout, that way the cards are positioned correctly inside the Grid Layout Group
            // Code can then read the information about their position after the UI has been updated
            LayoutRebuilder.ForceRebuildLayoutImmediate(targetContainer);
            Canvas.ForceUpdateCanvases();

            // Now get the final positions AFTER GridLayout updates
            foreach (Card referenceCard in currentCardOptions)
            {
                RectTransform rect = referenceCard.GetComponent<RectTransform>();
                Vector2 finalAnchoredPosition = rect.anchoredPosition;

                // Only assign position if it's different from the Grid Layout Group's auto positioning
                referenceCard.CardPointerInteraction.AssignAnchoredPosition(finalAnchoredPosition, targetContainer.position);
            }
        }

        // Empty temp lists so we can populate them again with new rewards to avoid duplicates during reward selection
        tempBasicCardPool.Clear();
        tempRareCardPool.Clear();
        tempEpicCardPool.Clear();
    }

    public void RemoveCardRewardOptions(CardScriptableObject chosenReward)
    {
        for (int i = 0; i < currentCardRewardOptions.Count; i++)
        {
            if (chosenReward == currentCardRewardOptions[i].GetCardInfo())
                currentCardRewardOptions[i].RemoveFromRewardScreen(true);
            else
                currentCardRewardOptions[i].RemoveFromRewardScreen(false);
        }
        currentCardRewardOptions.Clear();


        // Remove background rays
        animator.SetBool("isShowing", false);
        Invoke(nameof(RemoveRaysDelayed), 2f);
    }

    private void RemoveRaysDelayed()
    {
        rewardScreenRayPopulator.RemoveRays();
    }
}
