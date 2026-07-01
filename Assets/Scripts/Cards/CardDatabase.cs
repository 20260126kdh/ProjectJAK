using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour
{
    [Header("CSV Loader")]
    [SerializeField]
    private CardCSVLoader csvLoader;

    private List<CardData> allCards = new List<CardData>();

    public List<CardData> AllCards => allCards;

    private void Awake()
    {
        LoadDatabase();
    }

    private void LoadDatabase()
    {
        if (csvLoader == null)
        {
            Debug.LogError("CardCSVLoader가 연결되지 않았습니다.");
            return;
        }

        allCards = csvLoader.LoadCards();
    }

    public List<CardData> GetStartingDeck(PlayerClass playerClass)
    {
        List<CardData> startingDeck = new List<CardData>();

        foreach (CardData card in allCards)
        {
            if (card.ownerClass != playerClass)
                continue;

            if (!card.isStartingCard)
                continue;

            for (int i = 0; i < card.startingCount; i++)
            {
                startingDeck.Add(card);
            }
        }

        return startingDeck;
    }
}