using System.Collections.Generic;
using UnityEngine;

public class HandUI : MonoBehaviour
{
    [Header("카드가 생성될 부모")]
    [SerializeField]
    private Transform cardParent;

    [Header("카드 UI 프리팹")]
    [SerializeField]
    private CardUI cardPrefab;

    public void ShowCards(List<CardInfo> cards)
    {
        ClearCards();

        foreach (CardInfo card in cards)
        {
            CardUI cardUI = Instantiate(cardPrefab, cardParent);
            cardUI.SetCard(card);
        }
    }

    private void ClearCards()
    {
        foreach (Transform child in cardParent)
        {
            Destroy(child.gameObject);
        }
    }
}