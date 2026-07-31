using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 휴식 단계의 카드 강화 패널을 관리합니다.
/// 현재 덱의 카드를 화면에 생성하고,
/// 취소 시 휴식 패널로 돌아갑니다.
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

    private readonly List<CardUI> cardUIs =
        new List<CardUI>();

    private CardUI selectedCardUI;
    private CardData selectedCardData;

    private void Awake()
    {
        HidePanel();
    }

    /// <summary>
    /// 강화 패널을 표시하고 현재 덱을 생성합니다.
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

        selectedCardUI = null;
        selectedCardData = null;

        if (confirmButton != null)
        {
            confirmButton.interactable = false;
        }

        RefreshCards();

        Debug.Log("[UpgradePanelUI] 강화 패널 표시");
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
            if (card == null)
            {
                continue;
            }

            CardUI cardUI =
                Instantiate(cardPrefab, cardParent);

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
        selectedCardUI = null;
        selectedCardData = null;

        for (int i = cardUIs.Count - 1;
             i >= 0;
             i--)
        {
            CardUI cardUI = cardUIs[i];

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
         * Inspector에서 생성해둔 임시 카드가 있거나
         * 목록에서 누락된 카드가 있을 경우 함께 제거합니다.
         */
        for (int i = cardParent.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(cardParent.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 강화 패널에서 선택한 카드를 저장하고
    /// 선택 상태를 화면에 표시합니다.
    /// </summary>
    public void SelectUpgradeCard(CardUI cardUI)
    {
        if (cardUI == null)
        {
            Debug.LogWarning(
                "[UpgradePanelUI] 선택한 CardUI가 없습니다."
            );

            return;
        }

        CardData cardData =
            cardUI.GetCardData();

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
                $"[UpgradePanelUI] 이미 강화된 카드입니다: " +
                $"{cardData.GetDisplayName()}"
            );

            return;
        }

        if (selectedCardUI == cardUI)
        {
            return;
        }

        if (selectedCardUI != null)
        {
            selectedCardUI.SetDeselected();
        }

        selectedCardUI = cardUI;
        selectedCardData = cardData;

        selectedCardUI.SetSelected();

        if (confirmButton != null)
        {
            confirmButton.interactable = true;
        }

        Debug.Log(
            $"[UpgradePanelUI] 강화 카드 선택: " +
            $"{selectedCardData.cardName}"
        );
    }

    /// <summary>
    /// 선택한 카드의 모든 강화 가능한 효과를 강화합니다.
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
    /// 강화 패널을 닫고 휴식 패널로 돌아갑니다.
    /// </summary>
    public void OnClickCancel()
    {
        HidePanel();

        if (restPanelUI == null)
        {
            Debug.LogError(
                "[UpgradePanelUI] RestPanelUI가 연결되지 않았습니다."
            );

            return;
        }

        restPanelUI.ReturnToRestPanel();

        Debug.Log("[UpgradePanelUI] 강화 취소 - 휴식 패널 복귀");
    }
}