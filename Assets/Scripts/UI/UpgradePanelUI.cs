using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 휴식 단계의 카드 강화 패널을 관리합니다.
/// 현재 덱의 카드를 화면에 생성하고,
/// 카드 한 장을 선택하여 강화합니다.
/// </summary>
public class UpgradePanelUI : MonoBehaviour
{
    [Header("Upgrade Panel")]
    [SerializeField]
    private GameObject panel;

    [Header("Rest Panel UI")]
    [SerializeField]
    private RestPanelUI restPanelUI;

    [Header("Deck Manager")]
    [SerializeField]
    private DeckManager deckManager;

    [Header("Card Parent")]
    [SerializeField]
    private Transform cardParent;

    [Header("Card Prefab")]
    [SerializeField]
    private CardUI cardPrefab;

    [Header("강화 확정 버튼")]
    [SerializeField]
    private Button confirmButton;

    [Header("강화 망치 VFX")]
    [SerializeField]
    private CardUpgradeHammerVfx upgradeHammerVfxPrefab;

    [Header("전투 손패")]
    [SerializeField]
    private GameObject handCardParentObject;

    private readonly List<CardUI> cardUIs =
        new List<CardUI>();

    private CardUI selectedCardUI;
    private CardData selectedCardData;
    private bool isUpgradeVfxPlaying;
    private bool wasHandCardParentActive;
    private bool hasStoredHandCardParentState;

    [ContextMenu("강화 망치 VFX 테스트")]
    private void PlayUpgradeHammerVfxTest()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[UpgradePanelUI] 강화 망치 VFX 테스트는 Play Mode에서 실행해야 합니다.");
            return;
        }

        if (isUpgradeVfxPlaying || selectedCardUI == null)
        {
            Debug.LogWarning("[UpgradePanelUI] 테스트할 강화 카드를 먼저 선택해야 합니다.");
            return;
        }

        RectTransform selectedCardRect = selectedCardUI.transform as RectTransform;

        if (selectedCardRect == null || upgradeHammerVfxPrefab == null || panel == null)
        {
            Debug.LogError("[UpgradePanelUI] 강화 망치 VFX 테스트 설정이 누락되었습니다.");
            return;
        }

        isUpgradeVfxPlaying = true;
        SetUpgradeInteractionLocked(true);

        CardUpgradeHammerVfx hammerVfx = Instantiate(
            upgradeHammerVfxPrefab,
            panel.transform,
            false
        );
        hammerVfx.transform.SetAsLastSibling();
        hammerVfx.Play(
            selectedCardRect,
            null,
            () =>
            {
                isUpgradeVfxPlaying = false;
                SetUpgradeInteractionLocked(false);
            }
        );
    }

    private void SetUpgradeInteractionLocked(bool locked)
    {
        foreach (CardUI cardUI in cardUIs)
        {
            if (cardUI != null)
            {
                cardUI.SetUpgradeSelectionLocked(locked);
            }
        }

        if (confirmButton != null)
        {
            confirmButton.interactable = !locked && selectedCardData != null;
        }
    }

    private void Awake()
    {
        HidePanel();
    }

    /// <summary>
    /// 강화 패널을 표시하고 현재 덱의 카드를 생성합니다.
    /// </summary>
    public void ShowPanel()
    {
        if (panel == null)
        {
            Debug.LogError(
                "[UpgradePanelUI] Panel이 연결되지 않았습니다."
            );

            return;
        }

        panel.SetActive(true);
        HideHandCards();

        ResetSelection();
        RefreshCards();

        Debug.Log(
            $"[UpgradePanelUI] 강화 패널 표시 / " +
            $"ActiveSelf: {panel.activeSelf} / " +
            $"ActiveInHierarchy: {panel.activeInHierarchy}"
        );
    }

    /// <summary>
    /// 강화 패널을 숨깁니다.
    /// </summary>
    public void HidePanel()
    {
        if (panel == null)
        {
            return;
        }

        panel.SetActive(false);
        RestoreHandCards();
    }

    private void HideHandCards()
    {
        if (handCardParentObject == null)
        {
            return;
        }

        if (!hasStoredHandCardParentState)
        {
            wasHandCardParentActive = handCardParentObject.activeSelf;
            hasStoredHandCardParentState = true;
        }

        handCardParentObject.SetActive(false);
    }

    private void RestoreHandCards()
    {
        if (!hasStoredHandCardParentState)
        {
            return;
        }

        if (handCardParentObject != null)
        {
            handCardParentObject.SetActive(wasHandCardParentActive);
        }

        hasStoredHandCardParentState = false;
    }

    /// <summary>
    /// 현재 보유 덱을 강화 패널에 표시합니다.
    /// </summary>
    private void RefreshCards()
    {
        ClearCards();

        if (deckManager == null)
        {
            Debug.LogError(
                "[UpgradePanelUI] DeckManager가 연결되지 않았습니다."
            );

            return;
        }

        if (cardPrefab == null)
        {
            Debug.LogError(
                "[UpgradePanelUI] Card Prefab이 연결되지 않았습니다."
            );

            return;
        }

        if (cardParent == null)
        {
            Debug.LogError(
                "[UpgradePanelUI] Card Parent가 연결되지 않았습니다."
            );

            return;
        }

        foreach (CardData card in deckManager.CurrentDeck)
        {
            if (card == null || card.IsUpgraded)
            {
                continue;
            }

            CardUI cardUI =
                Instantiate(
                    cardPrefab,
                    cardParent
                );

            cardUI.InitializeAsUpgrade(
                card,
                this
            );

            cardUIs.Add(cardUI);
        }

        Debug.Log(
            $"[UpgradePanelUI] 강화 대상 카드 표시 : " +
            $"{cardUIs.Count}장"
        );
    }

    /// <summary>
    /// 기존에 생성된 카드 UI를 제거합니다.
    /// </summary>
    private void ClearCards()
    {
        ResetSelection();

        for (int i = cardUIs.Count - 1;
             i >= 0;
             i--)
        {
            CardUI cardUI =
                cardUIs[i];

            if (cardUI != null)
            {
                Destroy(cardUI.gameObject);
            }
        }

        cardUIs.Clear();

        if (cardParent == null)
        {
            return;
        }

        /*
         * Inspector에서 만들어둔 임시 카드나
         * 목록에서 누락된 카드가 있다면 함께 제거합니다.
         */
        for (int i = cardParent.childCount - 1;
             i >= 0;
             i--)
        {
            Transform child =
                cardParent.GetChild(i);

            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
    }

    /// <summary>
    /// 현재 강화 카드 선택 상태를 초기화합니다.
    /// </summary>
    private void ResetSelection()
    {
        selectedCardUI = null;
        selectedCardData = null;

        if (confirmButton != null)
        {
            confirmButton.interactable = false;
        }

        foreach (CardUI cardUI in cardUIs)
        {
            if (cardUI == null)
            {
                continue;
            }

            cardUI.ResetUpgradeSelection();
        }
    }

    /// <summary>
    /// 강화할 카드 한 장을 선택합니다.
    /// 선택된 카드를 다시 클릭하면 선택을 해제합니다.
    /// 다른 카드를 클릭하면 해당 카드로 선택을 전환합니다.
    /// </summary>
    public void SelectUpgradeCard(CardUI cardUI)
    {
        if (cardUI == null)
        {
            return;
        }

        /*
         * 현재 선택된 카드를 다시 클릭했다면
         * 선택을 해제합니다.
         */
        if (selectedCardUI == cardUI)
        {
            DeselectUpgradeCard();

            Debug.Log(
                "[UpgradePanelUI] 강화 카드 선택 해제"
            );

            return;
        }

        CardData cardData =
            cardUI.CardData;

        if (cardData == null)
        {
            Debug.LogWarning(
                "[UpgradePanelUI] 선택한 카드 데이터가 없습니다."
            );

            return;
        }

        if (cardData.IsUpgraded)
        {
            Debug.LogWarning(
                "[UpgradePanelUI] 이미 강화된 카드는 선택할 수 없습니다."
            );

            return;
        }

        if (selectedCardUI != null)
        {
            selectedCardUI.SetUpgradeSelected(false);
        }

        selectedCardUI = cardUI;
        selectedCardData = cardData;

        selectedCardUI.SetUpgradeSelected(true);

        if (confirmButton != null)
        {
            confirmButton.interactable = true;
        }

        Debug.Log(
            $"[UpgradePanelUI] 강화 카드 선택: " +
            $"{selectedCardData.GetDisplayName()}"
        );
    }

    /// <summary>
    /// 현재 선택된 강화 카드의 선택을 해제합니다.
    /// 모든 카드의 클릭 잠금을 해제합니다.
    /// </summary>
    private void DeselectUpgradeCard()
    {
        if (selectedCardUI != null)
        {
            selectedCardUI.SetUpgradeSelected(false);
        }

        selectedCardUI = null;
        selectedCardData = null;

        foreach (CardUI cardUI in cardUIs)
        {
            if (cardUI == null)
            {
                continue;
            }

            cardUI.SetUpgradeSelectionLocked(false);
        }

        if (confirmButton != null)
        {
            confirmButton.interactable = false;
        }
    }

    /// <summary>
    /// 선택한 카드의 강화 가능한 효과를 강화합니다.
    /// 카드 한 장을 강화한 뒤 휴식 패널로 돌아갑니다.
    /// </summary>
    public void OnClickConfirm()
    {
        if (selectedCardData == null)
        {
            Debug.LogWarning(
                "[UpgradePanelUI] 강화할 카드를 먼저 선택해야 합니다."
            );

            return;
        }

        if (selectedCardData.IsUpgraded)
        {
            Debug.LogWarning(
                "[UpgradePanelUI] 이미 강화된 카드입니다."
            );

            return;
        }

        bool upgraded =
            CardUpgradeUtility.UpgradeCard(
                selectedCardData
            );

        if (!upgraded)
        {
            Debug.LogWarning(
                $"[UpgradePanelUI] 강화 가능한 효과가 없습니다: " +
                $"{selectedCardData.cardName}"
            );

            return;
        }

        Debug.Log(
            $"[UpgradePanelUI] 카드 강화 완료: " +
            $"{selectedCardData.GetDisplayName()}"
        );

        /*
         * 강화가 완료되면 현재 카드들의
         * 선택 표시와 잠금 상태를 해제합니다.
         */
        ResetSelection();
        HidePanel();

        if (restPanelUI == null)
        {
            Debug.LogError(
                "[UpgradePanelUI] RestPanelUI가 연결되지 않았습니다."
            );

            return;
        }

        restPanelUI.CompleteUpgrade();
    }

    /// <summary>
    /// 취소 버튼에서 호출합니다.
    /// 카드 선택 상태를 초기화하고 휴식 패널로 돌아갑니다.
    /// </summary>
    public void OnClickCancel()
    {
        ResetSelection();
        HidePanel();

        if (restPanelUI == null)
        {
            Debug.LogError(
                "[UpgradePanelUI] RestPanelUI가 연결되지 않았습니다."
            );

            return;
        }

        restPanelUI.ReturnToRestPanel();

        Debug.Log(
            "[UpgradePanelUI] 강화 취소 - 휴식 패널 복귀"
        );
    }
}
