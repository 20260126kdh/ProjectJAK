using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 중 플레이어의 손패를 관리하는 클래스입니다.
/// 손패 카드 목록 저장, 드로우, 손패 UI 표시를 담당합니다.
/// </summary>
public class HandManager : MonoBehaviour
{
    [Header("Deck Manager")]
    [SerializeField]
    private DeckManager deckManager;

    [Header("현재 손패")]
    [SerializeField]
    private List<CardData> handCards = new List<CardData>();

    [Header("손패 UI 부모")]
    [SerializeField]
    private Transform handCardParent;

    [Header("카드 UI 프리팹")]
    [SerializeField]
    private CardUI cardPrefab;

    [Header("현재 선택된 카드")]
    [SerializeField]
    private CardUI selectedCardUI;

    /// <summary>
    /// 현재 손패 카드 목록을 반환합니다.
    /// </summary>
    public List<CardData> HandCards => handCards;

    /// <summary>
    /// 지정한 수만큼 카드를 드로우하여 손패에 추가합니다.
    /// </summary>
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

    /// <summary>
    /// 손패 UI를 현재 손패 목록 기준으로 다시 생성합니다.
    /// 카드들을 하단 중앙에 부채꼴로 배치합니다.
    /// </summary>
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
        }

        Debug.Log("[HandManager] 손패 UI 갱신 완료");
    }

    /// <summary>
    /// 손패 카드 중 하나를 선택합니다.
    /// 이미 선택된 카드가 있으면 선택 해제 후 새 카드를 선택합니다.
    /// </summary>
    public void SelectCard(CardUI cardUI)
    {
        if (selectedCardUI == cardUI)
        {
            selectedCardUI.SetDeselected();
            selectedCardUI = null;

            Debug.Log("[HandManager] 카드 선택 해제");
            return;
        }

        if (selectedCardUI != null)
        {
            selectedCardUI.SetDeselected();
        }

        selectedCardUI = cardUI;
        selectedCardUI.SetSelected();

        Debug.Log($"[HandManager] 카드 선택: {selectedCardUI.GetCardData().cardName}");
    }

    /// <summary>
    /// 손패 데이터를 모두 비웁니다.
    /// </summary>
    public void ClearHand()
    {
        selectedCardUI = null;
        handCards.Clear();
        ClearHandUI();

        Debug.Log("[HandManager] 손패 초기화 완료");
    }

    /// <summary>
    /// 손패 UI에 생성된 카드 오브젝트를 모두 제거합니다.
    /// </summary>
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