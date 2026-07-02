using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 카드 한 장의 UI를 표시하고 클릭 선택을 처리하는 클래스입니다.
/// </summary>
public class CardUI : MonoBehaviour, IPointerClickHandler
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

    [Header("선택 연출")]
    [SerializeField]
    private float selectedMoveY = 40f;

    [SerializeField]
    private float selectedScale = 1.08f;

    private CardData cardData;
    private HandManager handManager;
    private RectTransform rectTransform;

    private Vector2 defaultPosition;
    private Quaternion defaultRotation;
    private Vector3 defaultScale;

    /// <summary>
    /// 카드 UI를 초기화합니다.
    /// 카드 데이터와 소유 HandManager를 설정합니다.
    /// </summary>
    public void Initialize(CardData newCardData, HandManager ownerHandManager)
    {
        cardData = newCardData;
        handManager = ownerHandManager;

        rectTransform = GetComponent<RectTransform>();

        defaultPosition = rectTransform.anchoredPosition;
        defaultRotation = rectTransform.localRotation;
        defaultScale = rectTransform.localScale;

        SetCard(cardData);
    }

    /// <summary>
    /// 카드 데이터를 UI 텍스트에 반영합니다.
    /// </summary>
    public void SetCard(CardData newCardData)
    {
        cardData = newCardData;

        cardNameText.text = cardData.cardName;
        descriptionText.text = cardData.description;
        cardTypeText.text = cardData.cardType.ToString();
        cardRarityText.text = cardData.cardRarity.ToString();

        artworkImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// 카드 클릭 시 HandManager에게 선택 요청을 보냅니다.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (handManager == null)
        {
            Debug.LogWarning("[CardUI] HandManager가 연결되지 않았습니다.");
            return;
        }

        handManager.SelectCard(this);
    }

    /// <summary>
    /// 카드를 선택 상태로 표시합니다.
    /// </summary>
    public void SetSelected()
    {
        rectTransform.anchoredPosition = defaultPosition + new Vector2(0f, selectedMoveY);
        rectTransform.localRotation = defaultRotation;
        rectTransform.localScale = defaultScale * selectedScale;
    }

    /// <summary>
    /// 카드를 선택 해제 상태로 되돌립니다.
    /// </summary>
    public void SetDeselected()
    {
        rectTransform.anchoredPosition = defaultPosition;
        rectTransform.localRotation = defaultRotation;
        rectTransform.localScale = defaultScale;
    }

    /// <summary>
    /// 이 카드 UI가 가진 카드 데이터를 반환합니다.
    /// </summary>
    public CardData GetCardData()
    {
        return cardData;
    }
}