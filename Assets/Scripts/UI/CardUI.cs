using TMPro;
using UnityEngine;

public class CardUI : MonoBehaviour
{
    [Header("카드 이름")]
    [SerializeField]
    private TMP_Text cardNameText;

    [Header("카드 종류")]
    [SerializeField]
    private TMP_Text cardTypeText;

    [Header("카드 등급")]
    [SerializeField]
    private TMP_Text cardRarityText;

    [Header("카드 설명")]
    [SerializeField]
    private TMP_Text descriptionText;

    public void SetCard(CardInfo cardInfo)
    {
        cardNameText.text = cardInfo.cardName;
        cardTypeText.text = cardInfo.cardType.ToString();
        cardRarityText.text = cardInfo.cardRarity.ToString();
        descriptionText.text = cardInfo.description;
    }
}