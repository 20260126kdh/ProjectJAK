using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour
{
    public static CardDatabase Instance { get; private set; }

    public List<CardData> AllCards { get; private set; } = new();

    // 세 더미
    public List<CardData> DeckPile { get; private set; } = new();   // 카드덱
    public List<CardData> HandPile { get; private set; } = new();   // 손패
    public List<CardData> WastePile { get; private set; } = new();  // 버림패

    void Awake()
    {
        Instance = this;
        BuildStartingDeck();

        // 시작 시 모든 카드를 카드덱에 넣음
        DeckPile.AddRange(AllCards);
    }

    void BuildStartingDeck()
    {
        AddCards(CardType.Attack, CardRarity.Normal, 4);
        AddCards(CardType.Defense, CardRarity.Normal, 2);
        AddCards(CardType.BuffSkill, CardRarity.Normal, 1);
        AddCards(CardType.DebuffSkill, CardRarity.Normal, 1);
        AddCards(CardType.Attack, CardRarity.Rare, 2);
        AddCards(CardType.Defense, CardRarity.Rare, 1);
        AddCards(CardType.BuffSkill, CardRarity.Rare, 1);
        AddCards(CardType.Attack, CardRarity.Epic, 1);
        AddCards(CardType.Defense, CardRarity.Epic, 1);
        AddCards(CardType.BuffSkill, CardRarity.Epic, 1);
    }

    void AddCards(CardType type, CardRarity rarity, int count)
    {
        for (int i = 0; i < count; i++)
            AllCards.Add(new CardData(type, rarity));
    }

    // 카드덱에서 무작위로 한 장 뽑아 손패로. 덱이 비면 버림패를 셔플해 되채움
    public CardData DrawCard()
    {
        if (DeckPile.Count == 0)
            ReshuffleWasteIntoDeck();

        if (DeckPile.Count == 0)
            return null;  // 버림패도 비었으면 뽑을 카드 없음

        int idx = Random.Range(0, DeckPile.Count);
        CardData card = DeckPile[idx];
        DeckPile.RemoveAt(idx);
        HandPile.Add(card);
        return card;
    }

    // 버림패를 카드덱으로 되돌리고 섞기
    void ReshuffleWasteIntoDeck()
    {
        DeckPile.AddRange(WastePile);
        WastePile.Clear();
        Debug.Log("버림패를 카드덱으로 셔플");
    }

    // 손패의 카드를 버림패로 (카드 사용/턴 종료 시)
    public void DiscardFromHand(CardData card)
    {
        if (HandPile.Remove(card))
            WastePile.Add(card);
    }
}