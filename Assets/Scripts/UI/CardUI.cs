using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 카드 한 장의 UI를 표시하고 클릭 선택을 처리하는 클래스입니다.
/// </summary>
public class CardUI : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler
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

    [Header("강화 카드 선택 테두리")]
    [SerializeField]
    private Outline upgradeSelectionOutline;

    [Header("Jinx 사용 불가 표시")]
    [SerializeField]
    private GameObject jinxBlockMark;

    private CardData cardData;
    private HandManager handManager;
    private RectTransform rectTransform;

    private Vector2 defaultPosition;
    private Quaternion defaultRotation;
    private Vector3 defaultScale;

    private RewardPanelUI rewardPanelUI;
    private bool isRewardCard;

    private UpgradePanelUI upgradePanelUI;
    private bool isUpgradeCard;
    private bool isUpgradeSelected;
    private bool isUpgradeSelectionLocked;

    private bool isJinxed;
    private Outline tutorialHighlightOutline;

    /// <summary>
    /// 현재 Jinx로 사용 불가 상태인지 반환합니다.
    /// </summary>
    public bool IsJinxed => isJinxed;

    /// <summary>
    /// 현재 UI에 연결된 카드 데이터를 반환합니다.
    /// </summary>
    public CardData CardData => cardData;

    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();

        SaveDefaultTransform();

        if (jinxBlockMark != null)
        {
            jinxBlockMark.SetActive(false);
        }

        if (upgradeSelectionOutline != null)
        {
            upgradeSelectionOutline.enabled = false;
        }

        isJinxed = false;
        isUpgradeSelected = false;
        isUpgradeSelectionLocked = false;
    }

    /// <summary>
    /// 현재 카드의 기본 위치, 회전, 크기를 저장합니다.
    /// </summary>
    private void SaveDefaultTransform()
    {
        if (rectTransform == null)
        {
            rectTransform =
                GetComponent<RectTransform>();
        }

        if (rectTransform == null)
        {
            return;
        }

        defaultPosition =
            rectTransform.anchoredPosition;

        defaultRotation =
            rectTransform.localRotation;

        defaultScale =
            rectTransform.localScale;
    }

    /// <summary>
    /// 카드가 사용되는 UI 종류를 초기화합니다.
    /// </summary>
    private void ResetCardUIType()
    {
        handManager = null;
        rewardPanelUI = null;
        upgradePanelUI = null;

        isRewardCard = false;
        isUpgradeCard = false;
        isUpgradeSelected = false;
        isUpgradeSelectionLocked = false;

        if (upgradeSelectionOutline != null)
        {
            upgradeSelectionOutline.enabled = false;
        }
    }

    /// <summary>
    /// 카드 UI를 손패 카드로 초기화합니다.
    /// </summary>
    public void Initialize(
        CardData newCardData,
        HandManager ownerHandManager)
    {
        ResetCardUIType();

        cardData = newCardData;
        handManager = ownerHandManager;

        SaveDefaultTransform();

        SetCard(cardData);
        SetJinxed(false);
    }

    /// <summary>
    /// 카드 데이터를 UI 텍스트와 이미지에 반영합니다.
    /// </summary>
    public void SetCard(CardData newCardData)
    {
        cardData = newCardData;

        if (cardData == null)
        {
            Debug.LogWarning(
                "[CardUI] 표시할 카드 데이터가 없습니다."
            );

            return;
        }

        if (cardNameText != null)
        {
            cardNameText.text =
                cardData.GetDisplayName();
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                cardData.DisplayDescription;
        }

        if (cardTypeText != null)
        {
            cardTypeText.text =
                cardData.cardType.ToString();
        }

        if (cardRarityText != null)
        {
            cardRarityText.text =
                cardData.cardRarity.ToString();
        }

        if (artworkImage != null &&
            cardData.artwork != null)
        {
            artworkImage.sprite =
                cardData.artwork;

            artworkImage.gameObject.SetActive(true);
        }
        else if (artworkImage != null)
        {
            artworkImage.sprite = null;
            artworkImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 카드 종류에 따라 클릭 요청을 전달합니다.
    /// </summary>
    public void OnPointerClick(
        PointerEventData eventData)
    {
        if (isRewardCard)
        {
            if (rewardPanelUI == null)
            {
                Debug.LogWarning(
                    "[CardUI] RewardPanelUI가 연결되지 않았습니다."
                );

                return;
            }

            rewardPanelUI.SelectRewardCard(this);

            return;
        }

        if (isUpgradeCard)
        {
            if (upgradePanelUI == null)
            {
                Debug.LogWarning(
                    "[CardUI] UpgradePanelUI가 연결되지 않았습니다."
                );

                return;
            }

            upgradePanelUI.SelectUpgradeCard(this);

            return;
        }

        if (handManager == null)
        {
            Debug.LogWarning(
                "[CardUI] HandManager가 연결되지 않았습니다."
            );

            return;
        }

        handManager.RequestSelectCard(this);
    }

    /// <summary>
    /// 카드 위로 마우스가 진입할 때 카드 Hover 효과음을 재생합니다.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayCardHover();
        }
    }

    /// <summary>
    /// 손패 카드를 선택 상태로 표시합니다.
    /// </summary>
    public void SetSelected()
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchoredPosition =
            defaultPosition +
            new Vector2(
                0f,
                selectedMoveY
            );

        rectTransform.localRotation =
            defaultRotation;

        rectTransform.localScale =
            defaultScale * selectedScale;
    }

    /// <summary>
    /// 손패 카드를 선택 해제 상태로 되돌립니다.
    /// </summary>
    public void SetDeselected()
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchoredPosition =
            defaultPosition;

        rectTransform.localRotation =
            defaultRotation;

        rectTransform.localScale =
            defaultScale;
    }

    /// <summary>
    /// 튜토리얼에서 지정된 카드에 빨간색 강조 테두리를 표시합니다.
    /// </summary>
    public void SetTutorialHighlight(bool isVisible)
    {
        if (tutorialHighlightOutline == null)
        {
            tutorialHighlightOutline =
                gameObject.AddComponent<Outline>();
            tutorialHighlightOutline.effectColor = Color.red;
            tutorialHighlightOutline.effectDistance =
                new Vector2(6f, -6f);
            tutorialHighlightOutline.useGraphicAlpha = false;
        }

        tutorialHighlightOutline.enabled = isVisible;
    }

    /// <summary>
    /// 이 카드 UI가 가진 카드 데이터를 반환합니다.
    /// </summary>
    public CardData GetCardData()
    {
        return cardData;
    }

    /// <summary>
    /// 카드의 Jinx 사용 불가 상태와 표시를 설정합니다.
    /// </summary>
    public void SetJinxed(bool value)
    {
        isJinxed = value;

        if (jinxBlockMark != null)
        {
            jinxBlockMark.SetActive(value);
        }
    }

    /// <summary>
    /// 리워드 카드 UI로 초기화합니다.
    /// </summary>
    public void InitializeAsReward(
        CardData newCardData,
        RewardPanelUI ownerRewardPanelUI)
    {
        ResetCardUIType();

        cardData = newCardData;
        rewardPanelUI = ownerRewardPanelUI;
        isRewardCard = true;

        SaveDefaultTransform();

        SetCard(cardData);
        SetJinxed(false);
    }

    /// <summary>
    /// 강화 패널에 표시되는 카드로 초기화합니다.
    /// 강화 패널의 카드는 GridLayoutGroup이 위치를 관리하므로
    /// 위치, 회전, 크기를 코드에서 변경하지 않습니다.
    /// </summary>
    public void InitializeAsUpgrade(
        CardData newCardData,
        UpgradePanelUI newUpgradePanelUI)
    {
        ResetCardUIType();

        cardData = newCardData;
        upgradePanelUI = newUpgradePanelUI;

        isUpgradeCard = true;
        isUpgradeSelected = false;
        isUpgradeSelectionLocked = false;

        /*
         * 강화 패널의 카드 위치는 GridLayoutGroup이 관리합니다.
         * SaveDefaultTransform()과 SetDeselected()를 호출하면
         * 레이아웃 계산 전 위치로 이동할 수 있으므로 사용하지 않습니다.
         */

        if (upgradeSelectionOutline != null)
        {
            upgradeSelectionOutline.enabled = false;
        }

        SetCard(cardData);
        SetJinxed(false);
    }

    /// <summary>
    /// 강화 패널에서 선택 테두리를 표시하거나 숨깁니다.
    /// GridLayoutGroup이 관리하는 카드 위치와 크기는 변경하지 않습니다.
    /// </summary>
    public void SetUpgradeSelected(bool selected)
    {
        isUpgradeSelected = selected;

        if (upgradeSelectionOutline != null)
        {
            upgradeSelectionOutline.enabled = selected;
        }
    }

    /// <summary>
    /// 리워드 패널에서 카드 선택 테두리를 표시하거나 숨깁니다.
    /// 카드의 위치와 크기는 변경하지 않습니다.
    /// </summary>
    public void SetRewardSelected(bool selected)
    {
        if (upgradeSelectionOutline != null)
        {
            upgradeSelectionOutline.enabled = selected;
        }
    }

    /// <summary>
    /// 다른 강화 카드가 선택되었을 때
    /// 현재 카드의 추가 선택을 막거나 해제합니다.
    /// </summary>
    public void SetUpgradeSelectionLocked(bool locked)
    {
        isUpgradeSelectionLocked = locked;
    }

    /// <summary>
    /// 강화 카드의 선택 표시와 클릭 잠금 상태를 초기화합니다.
    /// 카드 Transform은 변경하지 않습니다.
    /// </summary>
    public void ResetUpgradeSelection()
    {
        isUpgradeSelected = false;
        isUpgradeSelectionLocked = false;

        if (upgradeSelectionOutline != null)
        {
            upgradeSelectionOutline.enabled = false;
        }
    }
}
