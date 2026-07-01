using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    [Header("카드 프레임")]
    [SerializeField]
    private Image frameImage;

    [Header("카드 일러스트")]
    [SerializeField]
    private Image artworkImage;

    [Header("카드 이름")]
    [SerializeField]
    private TMP_Text cardNameText;

    [Header("카드 설명")]
    [SerializeField]
    private TMP_Text descriptionText;

    [Header("카드 종류")]
    [SerializeField]
    private TMP_Text cardTypeText;

    [Header("카드 등급")]
    [SerializeField]
    private TMP_Text cardRarityText;

    public void SetCard(CardData cardData)
    {
        cardNameText.text = cardData.cardName;
        descriptionText.text = cardData.description;
        cardTypeText.text = cardData.cardType.ToString();
        cardRarityText.text = cardData.cardRarity.ToString();

        artworkImage.gameObject.SetActive(false);
    }
}