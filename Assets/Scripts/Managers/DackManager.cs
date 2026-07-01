using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [Header("Card Database")]
    [SerializeField]
    private CardDatabase cardDatabase;

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

        startingDeckUI.ShowStartingDeck(currentDeck);
    }

    private void CreateStartingDeck()
    {
        currentDeck.Clear();

        PlayerClass playerClass = GameManager.Instance.PlayerData.PlayerClass;

        currentDeck = cardDatabase.GetStartingDeck(playerClass);

        Debug.Log($"시작 덱 생성 완료: {currentDeck.Count}장");
    }
}