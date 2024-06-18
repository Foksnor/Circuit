using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardScreen : MonoBehaviour
{
    [SerializeField] private CardReward cardReward;
    private int amountOfRewardOptions = 3;

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

    private void Awake()
    {
        PlayerStats.RewardScreen = this;
        chanceForRareCard = defaultChanceForRareCard;
        chanceForEpicCard = defaultChanceForEpicCard;
    }

    public void GiveCardRewardOptions(CardScriptableObject[] possibleNewCardRewards)
    {
        // Add the cards to the possible pool for the future level ups, avoiding duplicates
        for (int i = 0; i < possibleNewCardRewards.Length; i++)
        {
            switch (possibleNewCardRewards[i].CardRarity)
            {
                case CardScriptableObject._CardRarity.Basic:
                    if (!basicCardRewardPool.Contains(possibleNewCardRewards[i]))
                        basicCardRewardPool.Add(possibleNewCardRewards[i]);
                    break;
                case CardScriptableObject._CardRarity.Rare:
                    if (!rareCardRewardPool.Contains(possibleNewCardRewards[i]))
                        rareCardRewardPool.Add(possibleNewCardRewards[i]);
                    break;
                case CardScriptableObject._CardRarity.Epic:
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

            // Populate that card in the reward screen, and remove it from the available options
            chosenList.Remove(cardScript);
            CardReward cardOption = Instantiate(cardReward, transform.position, transform.rotation, gameObject.transform);
            cardOption.SetCardRewardInfo(cardScript);
            currentCardRewardOptions.Add(cardOption);
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
            if (chosenReward == currentCardRewardOptions[i].CardScriptableObject)
                currentCardRewardOptions[i].RemoveFromRewardScreen(true);
            else
                currentCardRewardOptions[i].RemoveFromRewardScreen(false);
        }
        currentCardRewardOptions.Clear();
    }
}
