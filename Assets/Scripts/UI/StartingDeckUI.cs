using System.Collections.Generic;
using UnityEngine;

public class StartingDeckUI : MonoBehaviour
{
    [Header("Deck Manager")]
    [SerializeField]
    private DeckManager deckManager;

    [Header("Hand Manager")]
    [SerializeField]
    private HandManager handManager;

    [Header("덱 보기 패널")]
    [SerializeField]
    private GameObject startingDeckPanel;

    [Header("전투 UI 패널")]
    [SerializeField]
    private GameObject battlePanel;

    [Header("카드가 생성될 부모")]
    [SerializeField]
    private Transform cardGridParent;

    [Header("카드 UI 프리팹")]
    [SerializeField]
    private CardUI cardPrefab;

    [Header("현재 시작 덱 확인 모드 여부")]
    [SerializeField]
    private bool isStartingDeckMode;

    public void ShowStartingDeck(List<CardData> deck)
    {
        isStartingDeckMode = true;

        startingDeckPanel.SetActive(true);
        battlePanel.SetActive(false);

        ShowDeckCards(deck);
    }

    public void ShowCurrentDeck()
    {
        if (deckManager == null)
        {
            Debug.LogError("[StartingDeckUI] DeckManager가 연결되지 않았습니다.");
            return;
        }

        isStartingDeckMode = false;

        startingDeckPanel.SetActive(true);
        battlePanel.SetActive(false);

        ShowDeckCards(deckManager.CurrentDeck);

        Debug.Log("[StartingDeckUI] 현재 덱 보기");
    }

    public void ConfirmStartingDeck()
    {
        Debug.Log("[StartingDeckUI] Confirm 버튼 클릭됨");

        if (!isStartingDeckMode)
        {
            CloseDeckView();
            return;
        }

        if (deckManager != null)
        {
            deckManager.PrepareDrawPileForBattle();
        }
        else
        {
            Debug.LogError("[StartingDeckUI] DeckManager가 연결되지 않았습니다.");
            return;
        }

        if (handManager != null)
        {
            handManager.ClearHand();
            handManager.DrawCards(4);
        }
        else
        {
            Debug.LogError("[StartingDeckUI] HandManager가 연결되지 않았습니다.");
            return;
        }

        startingDeckPanel.SetActive(false);
        battlePanel.SetActive(true);
    }

    public void CloseDeckView()
    {
        startingDeckPanel.SetActive(false);
        battlePanel.SetActive(true);

        Debug.Log("[StartingDeckUI] 덱 보기 닫기");
    }

    private void ShowDeckCards(List<CardData> deck)
    {
        ClearCards();

        foreach (CardData card in deck)
        {
            CardUI cardUI = Instantiate(cardPrefab, cardGridParent);
            cardUI.SetCard(card);
        }
    }

    private void ClearCards()
    {
        foreach (Transform child in cardGridParent)
        {
            Destroy(child.gameObject);
        }
    }
}