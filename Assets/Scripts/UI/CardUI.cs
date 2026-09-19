using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 카드 한 장의 UI를 표시하고 클릭 선택을 처리하는 클래스입니다.
/// </summary>
public class CardUI : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    ICanvasRaycastFilter
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
    private float selectedScale = 1.5f;

    [Header("손패 확대 화면 여백")]
    [SerializeField, Min(0f)]
    private float focusScreenPadding = 12f;

    private RectTransform handVisualRoot;
    private Canvas handCanvas;
    private bool isHovered;
    private bool isSelected;
    private bool isFocusVisible;
    private readonly Vector3[] focusCorners = new Vector3[4];

    /// <summary>현재 마우스로 읽고 있는 손패 카드인지 반환합니다.</summary>
    public bool IsHandHovered => isHovered;

    /// <summary>전투에서 선택된 손패 카드인지 반환합니다.</summary>
    public bool IsHandSelected => isSelected;

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
        ResetHandFocus();
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
        CreateHandVisualRoot();

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
    /// 손패의 원래 클릭 영역을 유지하고 표시 부분만 확대할 수 있게 구성합니다.
    /// 드로우·버림·보존 연출은 기존 카드 루트를 계속 사용합니다.
    /// </summary>
    private void CreateHandVisualRoot()
    {
        if (handVisualRoot != null || rectTransform == null)
        {
            return;
        }

        GameObject visualObject = new GameObject("HandFocusVisual", typeof(RectTransform));
        visualObject.layer = gameObject.layer;
        handVisualRoot = visualObject.GetComponent<RectTransform>();
        handVisualRoot.SetParent(rectTransform, false);
        handVisualRoot.anchorMin = Vector2.zero;
        handVisualRoot.anchorMax = Vector2.one;
        handVisualRoot.sizeDelta = Vector2.zero;
        handVisualRoot.pivot = rectTransform.pivot;
        handVisualRoot.anchoredPosition = Vector2.zero;

        // 생성 당시 같은 크기의 부모로 이동하므로 기존 앵커와 오프셋을 보존합니다.
        while (rectTransform.childCount > 1)
        {
            rectTransform.GetChild(0).SetParent(handVisualRoot, false);
        }

        handCanvas = GetComponentInParent<Canvas>();
        if (handCanvas != null)
        {
            handCanvas = handCanvas.rootCanvas;
        }
    }

    /// <summary>
    /// 확대된 그림이 옆 카드의 원래 클릭 영역을 가로채지 않도록 합니다.
    /// 손패 밖의 확대 영역은 표시된 카드의 클릭 대상으로 유지합니다.
    /// </summary>
    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        return handVisualRoot == null || handManager == null ||
            !handManager.CanShowCardFocus ||
            handManager.CanReceiveCardPointer(this, screenPoint, eventCamera);
    }

    /// <summary>카드 진입 시 효과음을 재생하고 손패 카드를 확대합니다.</summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayCardHover();
        }

        if (handManager == null || !handManager.CanShowCardFocus)
        {
            return;
        }

        isHovered = true;
        RefreshHandFocus();
    }

    /// <summary>마우스가 벗어나면 선택된 카드만 확대 상태를 유지합니다.</summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        RefreshHandFocus();
    }

    /// <summary>숫자키와 클릭 선택에 동일한 손패 확대를 적용합니다.</summary>
    public void SetSelected()
    {
        // 손패 외 호출부에서도 기존 공개 API의 위치·크기 동작을 보존합니다.
        if (handVisualRoot == null && rectTransform != null)
        {
            rectTransform.anchoredPosition = defaultPosition + Vector2.up * selectedMoveY;
            rectTransform.localRotation = defaultRotation;
            rectTransform.localScale = defaultScale * selectedScale;
            return;
        }

        isSelected = true;
        RefreshHandFocus();
    }

    /// <summary>선택을 해제하되 마우스가 올라간 카드는 계속 확대합니다.</summary>
    public void SetDeselected()
    {
        if (handVisualRoot == null && rectTransform != null)
        {
            rectTransform.anchoredPosition = defaultPosition;
            rectTransform.localRotation = defaultRotation;
            rectTransform.localScale = defaultScale;
            return;
        }

        isSelected = false;
        RefreshHandFocus();
    }

    /// <summary>외부 카드 연출 전에 표시 부분을 즉시 원래 크기로 복원합니다.</summary>
    public void ResetHandFocus()
    {
        isHovered = false;
        isSelected = false;
        RestoreHandVisual();
    }

    private void OnDisable()
    {
        ResetHandFocus();
    }

    private void LateUpdate()
    {
        if (handVisualRoot == null || handManager == null)
        {
            return;
        }

        if (!handManager.CanShowCardFocus)
        {
            isHovered = false;
            if (isFocusVisible)
            {
                RestoreHandVisual();
            }
            return;
        }

        if (isHovered || isSelected)
        {
            ApplyHandFocus();
        }
    }

    private void RefreshHandFocus()
    {
        if (handVisualRoot == null || handManager == null)
        {
            return;
        }

        if (handManager.CanShowCardFocus && (isHovered || isSelected))
        {
            ApplyHandFocus();
        }
        else
        {
            RestoreHandVisual();
        }

        handManager.RefreshCardSiblingOrder();
    }

    private void RestoreHandVisual()
    {
        isFocusVisible = false;
        if (handVisualRoot == null)
        {
            return;
        }

        handVisualRoot.anchoredPosition = Vector2.zero;
        handVisualRoot.localRotation = Quaternion.identity;
        handVisualRoot.localScale = Vector3.one;
    }

    private void ApplyHandFocus()
    {
        isFocusVisible = true;
        handVisualRoot.localScale = Vector3.one * Mathf.Max(1f, selectedScale);
        handVisualRoot.localRotation = Quaternion.Inverse(rectTransform.localRotation);
        handVisualRoot.position = rectTransform.position +
            rectTransform.parent.TransformVector(Vector3.up * selectedMoveY);
        ClampHandFocusToScreen();
    }

    /// <summary>Canvas 배율과 카메라를 반영하여 확대 카드의 네 모서리를 화면 안에 둡니다.</summary>
    private void ClampHandFocusToScreen()
    {
        if (handCanvas == null)
        {
            return;
        }

        Camera uiCamera = handCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null : handCanvas.worldCamera;
        Rect viewport = uiCamera != null ? uiCamera.pixelRect :
            new Rect(0f, 0f, Screen.width, Screen.height);
        float padding = Mathf.Max(0f, focusScreenPadding) * handCanvas.scaleFactor;
        handVisualRoot.GetWorldCorners(focusCorners);
        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        foreach (Vector3 corner in focusCorners)
        {
            Vector2 point = RectTransformUtility.WorldToScreenPoint(uiCamera, corner);
            min = Vector2.Min(min, point);
            max = Vector2.Max(max, point);
        }

        Vector2 offset = new Vector2(
            GetScreenCorrection(min.x, max.x, viewport.xMin + padding, viewport.xMax - padding),
            GetScreenCorrection(min.y, max.y, viewport.yMin + padding, viewport.yMax - padding));
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(uiCamera, handVisualRoot.position);
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rectTransform, screenPosition + offset, uiCamera, out Vector3 correctedPosition))
        {
            handVisualRoot.position = correctedPosition;
        }
    }

    private static float GetScreenCorrection(float min, float max, float lower, float upper)
    {
        if (max - min > upper - lower)
        {
            return (lower + upper - min - max) * 0.5f;
        }
        return min < lower ? lower - min : max > upper ? upper - max : 0f;
    }

    /// <summary>
    /// 튜토리얼에서 지정된 카드에 빨간색 강조 테두리를 표시합니다.
    /// </summary>
    public void SetTutorialHighlight(bool isVisible)
    {
        if (tutorialHighlightOutline == null)
        {
            // 손패 표시 루트 안의 프레임에 붙여 확대·회전·이동을 함께 따릅니다.
            GameObject highlightTarget = frameImage != null
                ? frameImage.gameObject : gameObject;
            tutorialHighlightOutline =
                highlightTarget.AddComponent<Outline>();
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
