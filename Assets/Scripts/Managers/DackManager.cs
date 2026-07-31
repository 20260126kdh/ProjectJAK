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
        ClearCurrentDeck();

        if (GameManager.Instance == null ||
            GameManager.Instance.PlayerData == null)
        {
            Debug.LogError(
                "[DeckManager] PlayerData를 찾지 못했습니다."
            );

            return;
        }

        PlayerClass playerClass =
            GameManager.Instance.PlayerData.PlayerClass;

        foreach (StartingDeckEntry entry
                 in startingDeckDatabase.entries)
        {
            if (entry.ownerClass != playerClass)
            {
                continue;
            }

            CardData originalCard =
                cardDatabase.GetCardByID(entry.cardID);

            if (originalCard == null)
            {
                Debug.LogWarning(
                    $"[DeckManager] 카드를 찾지 못했습니다: " +
                    $"{entry.cardID}"
                );

                continue;
            }

            for (int i = 0; i < entry.count; i++)
            {
                CardData runtimeCard =
                    CreateRuntimeCard(originalCard);

                currentDeck.Add(runtimeCard);
            }
        }

        Debug.Log(
            $"[DeckManager] 시작 덱 생성 완료 : " +
            $"{currentDeck.Count}장"
        );
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
            Debug.LogWarning(
                "[DeckManager] 덱에 추가할 카드 데이터가 없습니다."
            );

            return;
        }

        CardData runtimeCard =
            CreateRuntimeCard(cardData);

        currentDeck.Add(runtimeCard);

        SortCurrentDeckByCardName();

        Debug.Log(
            $"[DeckManager] 카드 덱 추가 : " +
            $"{runtimeCard.cardName} / " +
            $"현재 덱 {currentDeck.Count}장"
        );
    }

    public bool IsStartingDeckCard(CardData cardData)
    {
        if (cardData == null)
        {
            return false;
        }

        if (startingDeckDatabase == null)
        {
            Debug.LogWarning("[DeckManager] StartingDeckDatabase가 연결되지 않았습니다.");
            return false;
        }

        PlayerClass playerClass = GameManager.Instance.PlayerData.PlayerClass;

        foreach (StartingDeckEntry entry in startingDeckDatabase.entries)
        {
            if (entry.ownerClass != playerClass)
                continue;

            if (entry.cardID == cardData.cardID)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 원본 카드 데이터로부터
    /// 현재 게임에서 사용할 독립적인 카드 한 장을 생성합니다.
    /// </summary>
    private CardData CreateRuntimeCard(
        CardData originalCard)
    {
        if (originalCard == null)
        {
            return null;
        }

        CardData runtimeCard =
            Instantiate(originalCard);

        runtimeCard.name =
            $"{originalCard.name}_Runtime";

        runtimeCard.ResetUpgradeState();

        return runtimeCard;
    }

    /// <summary>
    /// 기존 런타임 카드들을 제거하고
    /// 현재 덱 목록을 초기화합니다.
    /// </summary>
    private void ClearCurrentDeck()
    {
        for (int i = currentDeck.Count - 1;
             i >= 0;
             i--)
        {
            CardData card = currentDeck[i];

            if (card == null)
            {
                continue;
            }

            /*
             * Project 에셋 원본이 아니라
             * 실행 중 생성한 복사본만 제거합니다.
             */
            if (card.name.EndsWith("_Runtime"))
            {
                Destroy(card);
            }
        }

        currentDeck.Clear();
    }
}