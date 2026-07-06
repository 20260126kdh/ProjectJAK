using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 전투 종료 후 표시되는 리워드 패널을 관리합니다.
/// 현재 클래스 카드 중 2장을 랜덤으로 생성하고,
/// 기존 CardUI 프리팹을 사용해 1장을 선택하여 덱에 추가합니다.
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

    private bool hasSelectedRewardCard;

    private void Awake()
    {
        HideRewardPanel();
    }

    public void ShowRewardPanel()
    {
        if (rewardPanel == null)
        {
            Debug.LogError("[RewardPanelUI] Reward Panel이 연결되지 않았습니다.");
            return;
        }

        hasSelectedRewardCard = false;

        if (nextButton != null)
        {
            nextButton.interactable = false;
        }

        rewardPanel.SetActive(true);

        GenerateRewardCards();

        Debug.Log("[RewardPanelUI] 리워드 패널 표시");
    }

    public void HideRewardPanel()
    {
        if (rewardPanel == null)
            return;

        rewardPanel.SetActive(false);
    }

    private void GenerateRewardCards()
    {
        ClearRewardCards();

        List<CardData> rewardCards = GetRewardCards(2);

        for (int i = 0; i < rewardCards.Count; i++)
        {
            if (rewardCardPrefab == null)
            {
                Debug.LogError("[RewardPanelUI] Reward Card Prefab이 연결되지 않았습니다.");
                return;
            }

            if (rewardCardParent == null)
            {
                Debug.LogError("[RewardPanelUI] Reward Card Parent가 연결되지 않았습니다.");
                return;
            }

            CardUI cardUI = Instantiate(rewardCardPrefab, rewardCardParent);

            RectTransform rect = cardUI.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2((i - 0.5f) * 220f, 0f);
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;

            cardUI.InitializeAsReward(rewardCards[i], this);
        }

        Debug.Log($"[RewardPanelUI] 리워드 카드 생성 : {rewardCards.Count}장");
    }

    private void ClearRewardCards()
    {
        if (rewardCardParent == null)
            return;

        foreach (Transform child in rewardCardParent)
        {
            Destroy(child.gameObject);
        }
    }

    private List<CardData> GetRewardCards(int count)
    {
        List<CardData> result = new List<CardData>();

        if (cardDatabase == null)
        {
            Debug.LogError("[RewardPanelUI] CardDatabase가 연결되지 않았습니다.");
            return result;
        }

        if (GameManager.Instance == null || GameManager.Instance.PlayerData == null)
        {
            Debug.LogError("[RewardPanelUI] PlayerData를 찾지 못했습니다.");
            return result;
        }

        PlayerClass currentClass = GameManager.Instance.PlayerData.PlayerClass;

        int safetyCount = 0;

        while (result.Count < count && safetyCount < 100)
        {
            safetyCount++;

            CardRarity selectedRarity = GetRandomRarityByWeight();

            List<CardData> candidates = new List<CardData>();

            foreach (CardData card in cardDatabase.AllCards)
            {
                if (card == null)
                    continue;

                if (card.ownerClass != currentClass)
                    continue;

                if (card.cardRarity != selectedRarity)
                    continue;

                if (result.Contains(card))
                    continue;

                candidates.Add(card);
            }

            if (candidates.Count <= 0)
                continue;

            CardData selectedCard = candidates[Random.Range(0, candidates.Count)];
            result.Add(selectedCard);
        }

        return result;
    }

    private CardRarity GetRandomRarityByWeight()
    {
        int totalWeight = commonWeight + rareWeight + epicWeight;

        if (totalWeight <= 0)
        {
            Debug.LogWarning("[RewardPanelUI] 등급 확률 총합이 0 이하입니다. Common으로 처리합니다.");
            return CardRarity.Common;
        }

        int randomValue = Random.Range(0, totalWeight);

        if (randomValue < commonWeight)
            return CardRarity.Common;

        if (randomValue < commonWeight + rareWeight)
            return CardRarity.Rare;

        return CardRarity.Epic;
    }

    public void SelectRewardCard(CardData cardData)
    {
        if (hasSelectedRewardCard)
        {
            Debug.LogWarning("[RewardPanelUI] 이미 리워드 카드를 선택했습니다.");
            return;
        }

        if (cardData == null)
        {
            Debug.LogWarning("[RewardPanelUI] 선택한 리워드 카드가 없습니다.");
            return;
        }

        if (deckManager == null)
        {
            Debug.LogError("[RewardPanelUI] DeckManager가 연결되지 않았습니다.");
            return;
        }

        deckManager.AddCardToDeck(cardData);

        hasSelectedRewardCard = true;

        if (nextButton != null)
        {
            nextButton.interactable = true;
        }

        Debug.Log($"[RewardPanelUI] 리워드 카드 선택 : {cardData.cardName}");
    }

    public void OnClickContinue()
    {
        if (!hasSelectedRewardCard)
        {
            Debug.LogWarning("[RewardPanelUI] 리워드 카드를 먼저 선택해야 합니다.");
            return;
        }

        Debug.Log("[RewardPanelUI] Next 클릭");

        HideRewardPanel();

        if (battleManager == null)
        {
            Debug.LogError("[RewardPanelUI] BattleManager가 연결되지 않았습니다.");
            return;
        }

        if (StageManager.Instance == null)
        {
            Debug.LogWarning("[RewardPanelUI] StageManager.Instance가 없습니다. BattleManager로 직접 다음 전투를 시작합니다.");
            battleManager.StartNextBattle();
            return;
        }

        if (StageManager.Instance.CurrentPhase == StagePhase.Rest)
        {
            if (restPanelUI != null)
            {
                restPanelUI.ShowRestPanel();
            }
            else
            {
                Debug.LogError("[RewardPanelUI] RestPanelUI가 연결되지 않았습니다.");
            }

            return;
        }

        StageManager.Instance.ProceedAfterReward(battleManager);
    }
}