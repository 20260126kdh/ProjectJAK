using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 전투 종료 후 표시되는 리워드 패널을 관리합니다.
/// 현재 클래스 카드 중 2장을 랜덤으로 생성하고,
/// 선택한 카드 1장을 덱에 추가합니다.
/// </summary>
public class RewardPanelUI : MonoBehaviour
{
    [Header("리워드 패널")]
    [SerializeField]
    private GameObject rewardPanel;

    [Header("Battle Manager")]
    [SerializeField]
    private BattleManager battleManager;

    [Header("Deck Manager")]
    [SerializeField]
    private DeckManager deckManager;

    [Header("Card Database")]
    [SerializeField]
    private CardDatabase cardDatabase;

    [Header("Rest Panel UI")]
    [SerializeField]
    private RestPanelUI restPanelUI;

    [Header("리워드 카드 UI 부모")]
    [SerializeField]
    private Transform rewardCardParent;

    [Header("카드 UI 프리팹")]
    [SerializeField]
    private CardUI rewardCardPrefab;

    [Header("Next 버튼")]
    [SerializeField]
    private Button nextButton;

    [Header("등급 등장 확률")]
    [SerializeField]
    private int commonWeight = 70;

    [SerializeField]
    private int rareWeight = 25;

    [SerializeField]
    private int epicWeight = 5;

    /// <summary>
    /// 현재 생성된 리워드 카드 UI 목록입니다.
    /// </summary>
    private readonly List<CardUI> rewardCardUIs =
        new List<CardUI>();

    /// <summary>
    /// 현재 선택한 리워드 카드 UI입니다.
    /// </summary>
    private CardUI selectedRewardCardUI;

    /// <summary>
    /// 현재 선택한 리워드 카드 데이터입니다.
    /// </summary>
    private CardData selectedRewardCardData;

    private void Awake()
    {
        HideRewardPanel();
    }

    /// <summary>
    /// 리워드 패널을 표시하고 전투 종류에 맞는 리워드 카드를 생성합니다.
    /// </summary>
    /// <param name="isBossReward">보스 전투 승리 보상인지 여부</param>
    public void ShowRewardPanel(bool isBossReward)
    {
        if (rewardPanel == null)
        {
            Debug.LogError(
                "[RewardPanelUI] Reward Panel이 연결되지 않았습니다."
            );

            return;
        }

        ResetRewardSelection();

        rewardPanel.SetActive(true);

        GenerateRewardCards(isBossReward);

        Debug.Log(
            "[RewardPanelUI] 리워드 패널 표시"
        );
    }

    /// <summary>
    /// 리워드 패널을 숨깁니다.
    /// </summary>
    public void HideRewardPanel()
    {
        if (rewardPanel == null)
        {
            return;
        }

        rewardPanel.SetActive(false);
    }

    /// <summary>
    /// 전투 종류에 맞는 리워드 카드 2장을 생성합니다.
    /// </summary>
    private void GenerateRewardCards(bool isBossReward)
    {
        ClearRewardCards();

        CardRarity? forcedRarity =
            isBossReward
                ? CardRarity.Epic
                : null;

        List<CardData> rewardCards =
            GetRewardCards(
                2,
                forcedRarity
            );

        for (int i = 0;
             i < rewardCards.Count;
             i++)
        {
            if (rewardCardPrefab == null)
            {
                Debug.LogError(
                    "[RewardPanelUI] Reward Card Prefab이 연결되지 않았습니다."
                );

                return;
            }

            if (rewardCardParent == null)
            {
                Debug.LogError(
                    "[RewardPanelUI] Reward Card Parent가 연결되지 않았습니다."
                );

                return;
            }

            CardUI cardUI =
                Instantiate(
                    rewardCardPrefab,
                    rewardCardParent
                );

            RectTransform rect =
                cardUI.GetComponent<RectTransform>();

            if (rect != null)
            {
                rect.anchoredPosition =
                    new Vector2(
                        (i - 0.5f) * 220f,
                        0f
                    );

                rect.localRotation =
                    Quaternion.identity;

                rect.localScale =
                    Vector3.one;
            }

            cardUI.InitializeAsReward(
                rewardCards[i],
                this
            );

            cardUI.SetRewardSelected(false);

            rewardCardUIs.Add(cardUI);
        }

        Debug.Log(
            $"[RewardPanelUI] 리워드 카드 생성 : " +
            $"{rewardCards.Count}장"
        );
    }

    /// <summary>
    /// 기존에 생성된 리워드 카드를 제거합니다.
    /// </summary>
    private void ClearRewardCards()
    {
        ResetRewardSelection();

        for (int i = rewardCardUIs.Count - 1;
             i >= 0;
             i--)
        {
            CardUI cardUI =
                rewardCardUIs[i];

            if (cardUI != null)
            {
                Destroy(cardUI.gameObject);
            }
        }

        rewardCardUIs.Clear();

        if (rewardCardParent == null)
        {
            return;
        }

        /*
         * Inspector에서 임시로 만든 카드나
         * 목록에서 누락된 카드가 있으면 함께 제거합니다.
         */
        for (int i = rewardCardParent.childCount - 1;
             i >= 0;
             i--)
        {
            Transform child =
                rewardCardParent.GetChild(i);

            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
    }

    /// <summary>
    /// 현재 리워드 카드 선택을 초기화합니다.
    /// </summary>
    private void ResetRewardSelection()
    {
        if (selectedRewardCardUI != null)
        {
            selectedRewardCardUI.SetRewardSelected(false);
        }

        selectedRewardCardUI = null;
        selectedRewardCardData = null;

        if (nextButton != null)
        {
            nextButton.interactable = false;
        }

        foreach (CardUI cardUI in rewardCardUIs)
        {
            if (cardUI == null)
            {
                continue;
            }

            cardUI.SetRewardSelected(false);
        }
    }

    /// <summary>
    /// 지정한 수만큼 리워드 카드를 뽑습니다.
    /// </summary>
    private List<CardData> GetRewardCards(
        int count,
        CardRarity? forcedRarity)
    {
        List<CardData> result =
            new List<CardData>();

        if (cardDatabase == null)
        {
            Debug.LogError(
                "[RewardPanelUI] CardDatabase가 연결되지 않았습니다."
            );

            return result;
        }

        if (GameManager.Instance == null ||
            GameManager.Instance.PlayerData == null)
        {
            Debug.LogError(
                "[RewardPanelUI] PlayerData를 찾지 못했습니다."
            );

            return result;
        }

        PlayerClass currentClass =
            GameManager.Instance.PlayerData.PlayerClass;

        int safetyCount = 0;

        while (result.Count < count &&
               safetyCount < 100)
        {
            safetyCount++;

            CardRarity selectedRarity =
                forcedRarity ??
                GetRandomRarityByWeight();

            List<CardData> candidates =
                new List<CardData>();

            foreach (CardData card in cardDatabase.AllCards)
            {
                if (card == null)
                {
                    continue;
                }

                if (card.ownerClass != currentClass)
                {
                    continue;
                }

                if (card.cardRarity != selectedRarity)
                {
                    continue;
                }

                if (deckManager != null &&
                    deckManager.IsStartingDeckCard(card))
                {
                    continue;
                }

                if (result.Contains(card))
                {
                    continue;
                }

                candidates.Add(card);
            }

            if (candidates.Count <= 0)
            {
                continue;
            }

            CardData selectedCard =
                candidates[
                    Random.Range(
                        0,
                        candidates.Count
                    )
                ];

            result.Add(selectedCard);
        }

        return result;
    }

    /// <summary>
    /// 설정된 가중치에 따라 카드 등급을 결정합니다.
    /// </summary>
    private CardRarity GetRandomRarityByWeight()
    {
        int totalWeight =
            commonWeight +
            rareWeight +
            epicWeight;

        if (totalWeight <= 0)
        {
            Debug.LogWarning(
                "[RewardPanelUI] 등급 확률 총합이 0 이하입니다. " +
                "Common으로 처리합니다."
            );

            return CardRarity.Common;
        }

        int randomValue =
            Random.Range(
                0,
                totalWeight
            );

        if (randomValue < commonWeight)
        {
            return CardRarity.Common;
        }

        if (randomValue <
            commonWeight + rareWeight)
        {
            return CardRarity.Rare;
        }

        return CardRarity.Epic;
    }

    /// <summary>
    /// 리워드 카드 한 장을 선택합니다.
    /// 다른 카드를 클릭하면 기존 선택을 해제하고 새 카드를 선택합니다.
    /// 선택된 카드를 다시 클릭하면 선택을 해제합니다.
    /// </summary>
    public void SelectRewardCard(CardUI cardUI)
    {
        if (cardUI == null)
        {
            return;
        }

        CardData cardData =
            cardUI.CardData;

        if (cardData == null)
        {
            Debug.LogWarning(
                "[RewardPanelUI] 선택한 리워드 카드가 없습니다."
            );

            return;
        }

        /*
         * 현재 선택한 카드를 다시 클릭하면
         * 선택을 해제합니다.
         */
        if (selectedRewardCardUI == cardUI)
        {
            selectedRewardCardUI.SetRewardSelected(false);

            selectedRewardCardUI = null;
            selectedRewardCardData = null;

            if (nextButton != null)
            {
                nextButton.interactable = false;
            }

            Debug.Log(
                "[RewardPanelUI] 리워드 카드 선택 해제"
            );

            return;
        }

        /*
         * 기존에 선택한 카드가 있으면
         * 기존 카드의 선택 표시를 해제합니다.
         */
        if (selectedRewardCardUI != null)
        {
            selectedRewardCardUI.SetRewardSelected(false);
        }

        selectedRewardCardUI = cardUI;
        selectedRewardCardData = cardData;

        selectedRewardCardUI.SetRewardSelected(true);

        if (nextButton != null)
        {
            nextButton.interactable = true;
        }

        Debug.Log(
            $"[RewardPanelUI] 리워드 카드 선택 : " +
            $"{selectedRewardCardData.cardName}"
        );
    }

    /// <summary>
    /// 선택한 리워드 카드를 덱에 추가하고
    /// 다음 단계로 진행합니다.
    /// </summary>
    public void OnClickContinue()
    {
        if (selectedRewardCardData == null)
        {
            Debug.LogWarning(
                "[RewardPanelUI] 리워드 카드를 먼저 선택해야 합니다."
            );

            return;
        }

        if (deckManager == null)
        {
            Debug.LogError(
                "[RewardPanelUI] DeckManager가 연결되지 않았습니다."
            );

            return;
        }

        /*
         * 카드 클릭 시점이 아니라
         * Next 버튼 클릭 시점에 덱에 추가합니다.
         */
        deckManager.AddCardToDeck(
            selectedRewardCardData
        );

        Debug.Log(
            $"[RewardPanelUI] 리워드 카드 획득 : " +
            $"{selectedRewardCardData.cardName}"
        );

        HideRewardPanel();

        if (battleManager == null)
        {
            Debug.LogError(
                "[RewardPanelUI] BattleManager가 연결되지 않았습니다."
            );

            return;
        }

        if (StageManager.Instance == null)
        {
            Debug.LogWarning(
                "[RewardPanelUI] StageManager.Instance가 없습니다. " +
                "BattleManager로 직접 다음 전투를 시작합니다."
            );

            battleManager.StartNextBattle();

            return;
        }

        if (StageManager.Instance.CurrentPhase ==
            StagePhase.Rest)
        {
            if (restPanelUI != null)
            {
                restPanelUI.ShowRestPanel();
            }
            else
            {
                Debug.LogError(
                    "[RewardPanelUI] RestPanelUI가 연결되지 않았습니다."
                );
            }

            return;
        }

        StageManager.Instance.ProceedAfterReward(
            battleManager
        );
    }
}
