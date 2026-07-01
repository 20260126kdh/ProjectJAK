using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCardData", menuName = "Game/Card Data")]
public class CardData : ScriptableObject
{
    [Header("카드 ID")]
    public string cardID;

    [Header("카드 이름")]
    public string cardName;

    [Header("카드 설명")]
    [TextArea]
    public string description;

    [Header("카드 소유 클래스")]
    public PlayerClass ownerClass;

    [Header("카드 종류")]
    public CardType cardType;

    [Header("카드 등급")]
    public CardRarity cardRarity;

    [Header("카드 일러스트 경로")]
    public string artworkPath;

    [Header("카드 일러스트")]
    public Sprite artwork;

    [Header("카드 효과 목록")]
    public List<CardEffectData> effects = new List<CardEffectData>();
}