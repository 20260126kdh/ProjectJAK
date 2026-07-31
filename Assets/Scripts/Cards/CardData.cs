using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCardData",
    menuName = "Game/Card Data"
)]
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
    public List<CardEffectData> effects =
        new List<CardEffectData>();

    [Header("런타임 강화 상태")]
    [SerializeField]
    private bool isUpgraded;

    /// <summary>
    /// 현재 카드가 강화되었는지 반환합니다.
    /// </summary>
    public bool IsUpgraded => isUpgraded;

    /// <summary>
    /// 현재 카드를 강화 상태로 변경합니다.
    /// </summary>
    public void MarkAsUpgraded()
    {
        isUpgraded = true;
    }

    /// <summary>
    /// 런타임 카드 생성 시 강화 상태를 초기화합니다.
    /// </summary>
    public void ResetUpgradeState()
    {
        isUpgraded = false;
    }

    /// <summary>
    /// 화면에 표시할 카드 이름을 반환합니다.
    /// 강화된 카드는 이름 뒤에 +가 붙습니다.
    /// </summary>
    public string GetDisplayName()
    {
        return isUpgraded
            ? cardName + "+"
            : cardName;
    }
}