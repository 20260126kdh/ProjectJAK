using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 중 플레이어의 손패를 관리하는 클래스입니다.
/// 손패 카드 목록 저장, 드로우, 손패 UI 표시, 보존 카드 선택을 담당합니다.
/// </summary>
public class HandManager : MonoBehaviour
{
    [Header("Deck Manager")]
    [SerializeField]
    private DeckManager deckManager;

    [Header("Battle Manager")]
    [SerializeField]
    private BattleManager battleManager;

    [Header("현재 손패")]
    [SerializeField]
    private List<CardData> handCards = new List<CardData>();

    [Header("보존된 카드")]
    [SerializeField]
    private CardData preservedCard;

    [Header("현재 보존 선택 중인 카드")]
    [SerializeField]
    private CardData selectedPreserveCard;

    [Header("보존 모드 여부")]
    [SerializeField]
    private bool isPreserveMode;

    [Header("손패 UI 부모")]
    [SerializeField]
    private Transform handCardParent;

    [Header("카드 UI 프리팹")]
    [SerializeField]
    private CardUI cardPrefab;

    public List<CardData> HandCards => handCards;
    public CardData PreservedCard => preservedCard;
    public bool IsPreserveMode => isPreserveMode;

    public void DrawCards(int drawCount)
    {
        if (deckManager == null)
        {
            Debug.LogError("[HandManager] DeckManager가 연결되지 않았습니다.");
            return;
        }

        for (int i = 0; i < drawCount; i++)
        {
            CardData drawnCard = deckManager.DrawOneCard();

            if (drawnCard == null)
            {
                return;
            }

            handCards.Add(drawnCard);

            Debug.Log($"[HandManager] 카드 드로우: {drawnCard.cardName}");
        }

        RefreshHandUI();

        Debug.Log($"[HandManager] 현재 손패: {handCards.Count}장");
    }

    public void RefreshHandUI()
    {
        ClearHandUI();

        if (handCardParent == null)
        {
            Debug.LogError("[HandManager] HandCardParent가 연결되지 않았습니다.");
            return;
        }

        if (cardPrefab == null)
        {
            Debug.LogError("[HandManager] CardPrefab이 연결되지 않았습니다.");
            return;
        }

        int cardCount = handCards.Count;

        float cardSpacing = 180f;
        float rotationSpacing = 8f;
        float curveHeight = 25f;

        for (int i = 0; i < cardCount; i++)
        {
            CardUI cardUI = Instantiate(cardPrefab, handCardParent);

            RectTransform rect = cardUI.GetComponent<RectTransform>();

            float centerIndex = (cardCount - 1) / 2f;
            float offset = i - centerIndex;

            float x = offset * cardSpacing;
            float y = -Mathf.Abs(offset) * curveHeight;
            float zRotation = -offset * rotationSpacing;

            rect.anchoredPosition = new Vector2(x, y);
            rect.localRotation = Quaternion.Euler(0f, 0f, zRotation);

            cardUI.Initialize(handCards[i], this);

            if (handCards[i] == selectedPreserveCard)
            {
                cardUI.SetSelected();
            }
        }

        Debug.Log("[HandManager] 손패 UI 갱신 완료");
    }

    public void RequestSelectCard(CardUI cardUI)
    {
        if (cardUI == null)
        {
            Debug.LogWarning("[HandManager] 선택 요청된 CardUI가 없습니다.");
            return;
        }

        if (isPreserveMode)
        {
            SelectPreserveCard(cardUI);
            return;
        }

        if (battleManager == null)
        {
            Debug.LogError("[HandManager] BattleManager가 연결되지 않았습니다.");
            return;
        }

        battleManager.SelectCard(cardUI);
    }

    /// <summary>
    /// 보존 모드를 시작합니다.
    /// </summary>
    public void StartPreserveMode()
    {
        isPreserveMode = true;
        selectedPreserveCard = null;

        if (battleManager != null)
        {
            battleManager.ClearSelectedCard();
        }

        RefreshHandUI();

        Debug.Log("[HandManager] 보존 모드 시작");
    }

    /// <summary>
    /// 보존 모드에서 보존할 카드를 선택합니다.
    /// </summary>
    private void SelectPreserveCard(CardUI cardUI)
    {
        CardData cardData = cardUI.GetCardData();

        if (cardData == null)
        {
            Debug.LogWarning("[HandManager] 보존 선택할 카드 데이터가 없습니다.");
            return;
        }

        if (!handCards.Contains(cardData))
        {
            Debug.LogWarning($"[HandManager] 손패에 없는 카드는 보존할 수 없습니다 : {cardData.cardName}");
            return;
        }

        if (selectedPreserveCard == cardData)
        {
            selectedPreserveCard = null;
            RefreshHandUI();

            Debug.Log("[HandManager] 보존 카드 선택 해제");
            return;
        }

        selectedPreserveCard = cardData;
        RefreshHandUI();

        Debug.Log($"[HandManager] 보존 카드 선택 : {cardData.cardName}");
    }

    /// <summary>
    /// 현재 선택한 카드를 보존 카드로 확정합니다.
    /// </summary>
    public void ConfirmPreserveCard()
    {
        if (!isPreserveMode)
        {
            Debug.LogWarning("[HandManager] 현재 보존 모드가 아닙니다.");
            return;
        }

        preservedCard = selectedPreserveCard;

        if (preservedCard != null)
        {
            Debug.Log($"[HandManager] 보존 카드 확정 : {preservedCard.cardName}");
        }
        else
        {
            Debug.Log("[HandManager] 보존 카드 없이 진행");
        }

        isPreserveMode = false;
        selectedPreserveCard = null;

        DiscardUnpreservedCards();
    }

    /// <summary>
    /// 보존 카드를 제외한 손패의 모든 카드를 버림 더미로 이동합니다.
    /// </summary>
    public void DiscardUnpreservedCards()
    {
        if (deckManager == null)
        {
            Debug.LogError("[HandManager] DeckManager가 연결되지 않았습니다.");
            return;
        }

        List<CardData> cardsToDiscard = new List<CardData>();

        for (int i = 0; i < handCards.Count; i++)
        {
            CardData card = handCards[i];

            if (card == preservedCard)
                continue;

            cardsToDiscard.Add(card);
        }

        for (int i = 0; i < cardsToDiscard.Count; i++)
        {
            handCards.Remove(cardsToDiscard[i]);
            deckManager.AddToDiscardPile(cardsToDiscard[i]);

            Debug.Log($"[HandManager] 턴 종료 버림 : {cardsToDiscard[i].cardName}");
        }

        RefreshHandUI();

        Debug.Log($"[HandManager] 보존 처리 완료. 현재 손패 : {handCards.Count}장");
    }

    public void DiscardUsedCard(CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogWarning("[HandManager] 버릴 카드 데이터가 없습니다.");
            return;
        }

        if (deckManager == null)
        {
            Debug.LogError("[HandManager] DeckManager가 연결되지 않았습니다.");
            return;
        }

        if (!handCards.Contains(cardData))
        {
            Debug.LogWarning($"[HandManager] 손패에 해당 카드가 없습니다 : {cardData.cardName}");
            return;
        }

        handCards.Remove(cardData);
        deckManager.AddToDiscardPile(cardData);

        RefreshHandUI();

        Debug.Log($"[HandManager] 사용한 카드 버림 더미 이동 : {cardData.cardName}");
    }

    public void RemoveCardFromHand(CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogWarning("[HandManager] 제거할 카드 데이터가 없습니다.");
            return;
        }

        if (handCards.Contains(cardData))
        {
            handCards.Remove(cardData);
            RefreshHandUI();

            Debug.Log($"[HandManager] 손패에서 카드 제거 : {cardData.cardName}");
        }
        else
        {
            Debug.LogWarning($"[HandManager] 손패에 해당 카드가 없습니다 : {cardData.cardName}");
        }
    }

    public void ClearHand()
    {
        handCards.Clear();
        ClearHandUI();

        Debug.Log("[HandManager] 손패 초기화 완료");
    }

    private void ClearHandUI()
    {
        if (handCardParent == null)
            return;

        foreach (Transform child in handCardParent)
        {
            Destroy(child.gameObject);
        }
    }
}