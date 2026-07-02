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

    [Header("시작 덱 패널")]
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

    public void ShowStartingDeck(List<CardData> deck)
    {
        startingDeckPanel.SetActive(true);
        battlePanel.SetActive(false);

        ClearCards();

        foreach (CardData card in deck)
        {
            CardUI cardUI = Instantiate(cardPrefab, cardGridParent);
            cardUI.SetCard(card);
        }
    }

    public void ConfirmStartingDeck()
    {
        Debug.Log("[StartingDeckUI] Confirm 버튼 클릭됨");

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

    private void ClearCards()
    {
        foreach (Transform child in cardGridParent)
        {
            Destroy(child.gameObject);
        }
    }
}