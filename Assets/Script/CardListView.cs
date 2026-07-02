using System.Collections.Generic;
using UnityEngine;

public enum PileType { Deck, Waste, Current }  // 어떤 더미를 보여줄지

public class CardListView : MonoBehaviour
{
    [SerializeField] PileType pileType;       // 이 뷰가 보여줄 더미
    [SerializeField] Transform content;        // Scroll View의 Content
    [SerializeField] GameObject[] cardPrefabs; // CardType 순서대로 4종

    // 팝업 열 때 호출 → 해당 더미의 카드를 그림
    public void Refresh()
    {
        // 기존에 그려둔 카드 제거
        foreach (Transform child in content)
            Destroy(child.gameObject);

        // 보여줄 카드 목록 결정
        List<CardData> cards = GetCards();

        // 카드마다 프리팹 생성
        foreach (var card in cards)
        {
            GameObject prefab = cardPrefabs[(int)card.type];
            Instantiate(prefab, content);
        }
    }

    List<CardData> GetCards()
    {
        var db = CardDatabase.Instance;
        switch (pileType)
        {
            case PileType.Deck: return db.DeckPile;   // 카드덱
            case PileType.Waste: return db.WastePile;  // 버림패
            case PileType.Current: return db.AllCards;   // 전체(현재 덱)
            default: return new List<CardData>();
        }
    }
}