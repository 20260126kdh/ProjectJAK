using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour
{
    [Header("전체 카드 데이터")]
    [SerializeField]
    private List<CardData> allCards = new List<CardData>();

    public CardData GetCardByID(string cardID)
    {
        foreach (CardData card in allCards)
        {
            if (card.cardID == cardID)
                return card;
        }

        Debug.LogWarning($"카드 ID를 찾을 수 없습니다: {cardID}");
        return null;
    }
}