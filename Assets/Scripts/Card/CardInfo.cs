using UnityEngine;

[System.Serializable]
public class CardInfo
{
    [Header("카드 이름")]
    public string cardName;

    [Header("카드 설명")]
    [TextArea]
    public string description;

    [Header("카드 종류")]
    public CardType cardType;

    [Header("카드 등급")]
    public CardRarity cardRarity;
}