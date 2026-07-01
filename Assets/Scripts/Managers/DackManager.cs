using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [Header("Card Database")]
    [SerializeField]
    private CardDatabase cardDatabase;

    [Header("Starting Deck Database")]
    [SerializeField]
    private StartingDeckDatabase startingDeckDatabase;

    [Header("현재 덱")]
    [SerializeField]
    private List<CardData> currentDeck = new List<CardData>();

    [Header("시작 덱 UI")]
    [SerializeField]
    private StartingDeckUI startingDeckUI;

    public List<CardData> CurrentDeck => currentDeck;

    private void Start()
    {
        CreateStartingDeck();

        if (startingDeckUI != null)
        {
            startingDeckUI.ShowStartingDeck(currentDeck);
        }
    }

    private void CreateStartingDeck()
    {
        currentDeck.Clear();

        PlayerClass playerClass = GameManager.Instance.PlayerData.PlayerClass;

        foreach (StartingDeckEntry entry in startingDeckDatabase.entries)
        {
            if (entry.ownerClass != playerClass)
                continue;

            CardData card = cardDatabase.GetCardByID(entry.cardID);

            if (card == null)
                continue;

            for (int i = 0; i < entry.count; i++)
            {
                currentDeck.Add(card);
            }
        }

        Debug.Log($"시작 덱 생성 완료: {currentDeck.Count}장");
    }
}