using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 덱을 관리하는 클래스입니다.
/// 시작 덱 생성, 정렬, 드로우 파일 생성, 셔플, 버림 더미 재사용을 담당합니다.
/// </summary>
public class DeckManager : MonoBehaviour
{
    [Header("Card Database")]
    [SerializeField]
    private CardDatabase cardDatabase;

    [Header("Starting Deck Database")]
    [SerializeField]
    private StartingDeckDatabase startingDeckDatabase;

    [Header("현재 보유 덱")]
    [SerializeField]
    private List<CardData> currentDeck = new List<CardData>();

    [Header("드로우 파일")]
    [SerializeField]
    private List<CardData> drawPile = new List<CardData>();

    [Header("버린 카드 더미")]
    [SerializeField]
    private List<CardData> discardPile = new List<CardData>();

    [Header("시작 덱 UI")]
    [SerializeField]
    private StartingDeckUI startingDeckUI;

    public List<CardData> CurrentDeck => currentDeck;
    public List<CardData> DrawPile => drawPile;
    public List<CardData> DiscardPile => discardPile;

    private void Start()
    {
        CreateStartingDeck();
        SortCurrentDeckByCardName();

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

        Debug.Log($"시작 덱 생성 완료 : {currentDeck.Count}장");
    }

    private void SortCurrentDeckByCardName()
    {
        currentDeck.Sort((a, b) => string.Compare(a.cardName, b.cardName));
    }

    private void CreateDrawPileFromCurrentDeck()
    {
        drawPile.Clear();
        discardPile.Clear();

        for (int i = 0; i < currentDeck.Count; i++)
        {
            drawPile.Add(currentDeck[i]);
        }
    }

    private void ShuffleDrawPile()
    {
        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            CardData temp = drawPile[i];
            drawPile[i] = drawPile[randomIndex];
            drawPile[randomIndex] = temp;
        }
    }

    public void PrepareDrawPileForBattle()
    {
        CreateDrawPileFromCurrentDeck();
        ShuffleDrawPile();

        Debug.Log($"드로우 파일 생성 완료 : {drawPile.Count}장");
    }

    /// <summary>
    /// 드로우 파일에서 카드 1장을 꺼내 반환합니다.
    /// 드로우 파일이 비어 있으면 버림 더미를 섞어서 다시 드로우 파일로 사용합니다.
    /// </summary>
    public CardData DrawOneCard()
    {
        if (drawPile.Count <= 0)
        {
            RefillDrawPileFromDiscardPile();
        }

        if (drawPile.Count <= 0)
        {
            Debug.LogWarning("[DeckManager] 드로우할 카드가 없습니다.");
            return null;
        }

        CardData card = drawPile[0];
        drawPile.RemoveAt(0);

        return card;
    }

    /// <summary>
    /// 버림 더미의 카드를 드로우 파일로 옮기고 셔플합니다.
    /// </summary>
    private void RefillDrawPileFromDiscardPile()
    {
        if (discardPile.Count <= 0)
        {
            Debug.LogWarning("[DeckManager] 버림 더미도 비어 있어 드로우 파일을 재생성할 수 없습니다.");
            return;
        }

        for (int i = 0; i < discardPile.Count; i++)
        {
            drawPile.Add(discardPile[i]);
        }

        discardPile.Clear();
        ShuffleDrawPile();

        Debug.Log($"[DeckManager] 버림 더미를 섞어 드로우 파일 재생성 : {drawPile.Count}장");
    }

    public void AddToDiscardPile(CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogWarning("[DeckManager] 버린 카드 더미에 추가할 카드 데이터가 없습니다.");
            return;
        }

        discardPile.Add(cardData);

        Debug.Log($"[DeckManager] 버린 카드 더미 추가 : {cardData.cardName}");
    }

    public void AddCardToDeck(CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogWarning("[DeckManager] 덱에 추가할 카드 데이터가 없습니다.");
            return;
        }

        currentDeck.Add(cardData);
        SortCurrentDeckByCardName();

        Debug.Log($"[DeckManager] 카드 덱 추가 : {cardData.cardName} / 현재 덱 {currentDeck.Count}장");
    }
}