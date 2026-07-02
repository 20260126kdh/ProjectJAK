using UnityEngine;

// 카드 종류
public enum CardType { Attack, Defense, BuffSkill, DebuffSkill }

// 카드 등급
public enum CardRarity { Normal, Rare, Epic }

// 카드 한 장의 데이터 (이름·효과는 나중에 필드 추가로 확장)
[System.Serializable]
public class CardData
{
    public CardType type;
    public CardRarity rarity;

    public CardData(CardType type, CardRarity rarity)
    {
        this.type = type;
        this.rarity = rarity;
    }
}