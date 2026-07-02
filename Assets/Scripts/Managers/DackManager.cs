using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 덱을 관리하는 클래스입니다.
/// 시작 덱 생성, 정렬, 드로우 파일 생성 및 셔플을 담당합니다.
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

    /// <summary>
    /// 현재 보유 덱을 반환합니다.
    /// </summary>
    public List<CardData> CurrentDeck => currentDeck;

    /// <summary>
    /// 현재 드로우 파일을 반환합니다.
    /// </summary>
    public List<CardData> DrawPile => drawPile;

    /// <summary>
    /// 버린 카드 더미를 반환합니다.
    /// </summary>
    public List<CardData> DiscardPile => discardPile;

    /// <summary>
    /// BattleScene 시작 시 호출됩니다.
    /// 시작 덱을 생성하고 정렬하여 UI에 표시합니다.
    /// </summary>
    private void Start()
    {
        CreateStartingDeck();
        SortCurrentDeckByCardName();

        if (startingDeckUI != null)
        {
            startingDeckUI.ShowStartingDeck(currentDeck);
        }
    }

    /// <summary>
    /// 선택한 클래스에 맞는 시작 덱을 생성합니다.
    /// </summary>
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

    /// <summary>
    /// 시작 덱을 카드 이름 기준으로 정렬합니다.
    /// 시작 덱 UI에서 항상 일정한 순서로 표시하기 위해 사용합니다.
    /// </summary>
    private void SortCurrentDeckByCardName()
    {
        currentDeck.Sort((a, b) => string.Compare(a.cardName, b.cardName));
    }

    /// <summary>
    /// 현재 보유 덱을 복사하여 드로우 파일을 생성합니다.
    /// 새로운 전투 시작 시 호출됩니다.
    /// </summary>
    private void CreateDrawPileFromCurrentDeck()
    {
        drawPile.Clear();
        discardPile.Clear();

        for (int i = 0; i < currentDeck.Count; i++)
        {
            drawPile.Add(currentDeck[i]);
        }
    }

    /// <summary>
    /// 드로우 파일을 무작위로 섞습니다.
    /// Fisher-Yates Shuffle 방식을 사용합니다.
    /// </summary>
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

    /// <summary>
    /// 전투 시작 전에 드로우 파일을 생성하고 셔플합니다.
    /// 시작 덱 확인 후 Confirm 버튼에서 호출됩니다.
    /// </summary>
    public void PrepareDrawPileForBattle()
    {
        CreateDrawPileFromCurrentDeck();
        ShuffleDrawPile();

        Debug.Log($"드로우 파일 생성 완료 : {drawPile.Count}장");
    }

    /// <summary>
    /// 드로우 파일에서 카드 1장을 꺼내 반환합니다.
    /// 드로우 파일이 비어 있으면 null을 반환합니다.
    /// </summary>
    public CardData DrawOneCard()
    {
        if (drawPile.Count <= 0)
        {
            Debug.LogWarning("[DeckManager] 드로우 파일이 비어 있습니다.");
            return null;
        }

        CardData card = drawPile[0];
        drawPile.RemoveAt(0);

        return card;
    }
}