using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 덱을 관리하는 클래스입니다.
///
/// 담당 기능:
/// - 시작 덱 생성
/// - 현재 덱 정렬
/// - 드로우 파일 생성 및 셔플
/// - 버림 더미 재사용
/// - 카드 획득
/// - 저장 데이터 기반 덱 복원
/// </summary>
public class DeckManager : MonoBehaviour
{
    [Header("Card Database")]
    [SerializeField]
    private CardDatabase cardDatabase;

    [Header("Starting Deck Database")]
    [SerializeField]
    private StartingDeckDatabase startingDeckDatabase;

    [Header("현재 보유 덱")]
    [SerializeField]
    private List<CardData> currentDeck =
        new List<CardData>();

    [Header("드로우 파일")]
    [SerializeField]
    private List<CardData> drawPile =
        new List<CardData>();

    [Header("버린 카드 더미")]
    [SerializeField]
    private List<CardData> discardPile =
        new List<CardData>();

    [Header("시작 덱 UI")]
    [SerializeField]
    private StartingDeckUI startingDeckUI;

    /// <summary>
    /// 현재 보유한 전체 덱입니다.
    /// </summary>
    public List<CardData> CurrentDeck =>
        currentDeck;

    /// <summary>
    /// 현재 뽑을 패 더미입니다.
    /// </summary>
    public List<CardData> DrawPile =>
        drawPile;

    /// <summary>
    /// 현재 버림 패 더미입니다.
    /// </summary>
    public List<CardData> DiscardPile =>
        discardPile;

    /// <summary>
    /// BattleScene 진입 시 새 게임과 이어하기를 구분하여
    /// 현재 덱을 준비합니다.
    /// </summary>
    private void Start()
    {
        if (ContinueLoadContext.HasPendingSaveData)
        {
            RestoreContinueDeck();
            return;
        }

        PrepareNewGameDeck();
    }

    /// <summary>
    /// 새 게임용 시작 덱을 생성하고
    /// 시작 덱 확인 화면을 표시합니다.
    /// </summary>
    private void PrepareNewGameDeck()
    {
        CreateStartingDeck();
        SortCurrentDeckByCardName();

        if (startingDeckUI == null)
        {
            Debug.LogError(
                "[DeckManager] StartingDeckUI가 연결되지 않았습니다."
            );

            return;
        }

        startingDeckUI.ShowStartingDeck(
            currentDeck
        );

        Debug.Log(
            "[DeckManager] 새 게임 시작 덱 준비 완료"
        );
    }

    /// <summary>
    /// ContinueLoadContext에 보관된 저장 데이터를 사용해
    /// 현재 보유 덱을 복원합니다.
    ///
    /// 복원 성공 시 시작 덱 확인창을 생략하고
    /// 바로 전투용 손패를 준비합니다.
    /// </summary>
    private void RestoreContinueDeck()
    {
        if (!ContinueLoadContext.TryGetPendingSaveData(
                out GameSaveData saveData))
        {
            Debug.LogError(
                "[DeckManager] 이어하기 저장 데이터를 가져오지 못했습니다."
            );

            PrepareNewGameDeck();
            return;
        }

        bool restoreSucceeded =
            RestoreDeck(
                saveData.cards
            );

        if (!restoreSucceeded)
        {
            Debug.LogError(
                "[DeckManager] 저장 덱 복원에 실패했습니다. " +
                "이어하기를 중단하고 임시 데이터를 제거합니다."
            );

            ContinueLoadContext.Clear();

            /*
             * 복원 실패 후 임의의 시작 덱으로 계속 진행하면
             * 플레이어와 Stage 진행도는 저장 상태인데
             * 덱만 시작 덱이 되는 불일치가 생깁니다.
             *
             * 따라서 시작 덱 확인창으로 자동 전환하지 않고
             * 오류 상태로 남겨 원인을 확인하도록 합니다.
             */
            return;
        }

        if (startingDeckUI == null)
        {
            Debug.LogError(
                "[DeckManager] StartingDeckUI가 연결되지 않아 " +
                "이어하기 전투 준비를 완료할 수 없습니다."
            );

            ContinueLoadContext.Clear();
            return;
        }

        bool battlePrepareSucceeded =
            startingDeckUI.PrepareBattleAfterContinue();

        if (!battlePrepareSucceeded)
        {
            Debug.LogError(
                "[DeckManager] 이어하기 손패 준비에 실패했습니다."
            );

            ContinueLoadContext.Clear();
            return;
        }

        ContinueLoadContext.Clear();

        Debug.Log(
            $"[DeckManager] 이어하기 덱 복원 및 전투 준비 완료: " +
            $"{currentDeck.Count}장"
        );
    }

    /// <summary>
    /// 선택한 클래스의 시작 덱을 생성합니다.
    /// </summary>
    private void CreateStartingDeck()
    {
        ClearCurrentDeck();

        if (GameManager.Instance == null ||
            GameManager.Instance.PlayerData == null)
        {
            Debug.LogError(
                "[DeckManager] PlayerData를 찾지 못했습니다."
            );

            return;
        }

        if (startingDeckDatabase == null)
        {
            Debug.LogError(
                "[DeckManager] StartingDeckDatabase가 연결되지 않았습니다."
            );

            return;
        }

        if (cardDatabase == null)
        {
            Debug.LogError(
                "[DeckManager] CardDatabase가 연결되지 않았습니다."
            );

            return;
        }

        PlayerClass playerClass =
            GameManager.Instance.PlayerData.PlayerClass;

        foreach (StartingDeckEntry entry
                 in startingDeckDatabase.entries)
        {
            if (entry == null)
            {
                continue;
            }

            if (entry.ownerClass != playerClass)
            {
                continue;
            }

            CardData originalCard =
                cardDatabase.GetCardByID(
                    entry.cardID
                );

            if (originalCard == null)
            {
                Debug.LogWarning(
                    $"[DeckManager] 카드를 찾지 못했습니다: " +
                    $"{entry.cardID}"
                );

                continue;
            }

            for (int i = 0;
                 i < entry.count;
                 i++)
            {
                CardData runtimeCard =
                    CreateRuntimeCard(
                        originalCard
                    );

                if (runtimeCard == null)
                {
                    continue;
                }

                currentDeck.Add(
                    runtimeCard
                );
            }
        }

        Debug.Log(
            $"[DeckManager] 시작 덱 생성 완료: " +
            $"{currentDeck.Count}장"
        );
    }

    /// <summary>
    /// 현재 덱을 카드 이름순으로 정렬합니다.
    /// </summary>
    private void SortCurrentDeckByCardName()
    {
        currentDeck.Sort(
            (a, b) =>
            {
                if (a == null && b == null)
                {
                    return 0;
                }

                if (a == null)
                {
                    return 1;
                }

                if (b == null)
                {
                    return -1;
                }

                return string.Compare(
                    a.cardName,
                    b.cardName,
                    System.StringComparison.Ordinal
                );
            }
        );
    }

    /// <summary>
    /// 현재 전체 덱을 기준으로 드로우 파일을 생성합니다.
    /// 버림 더미는 초기화합니다.
    /// </summary>
    private void CreateDrawPileFromCurrentDeck()
    {
        drawPile.Clear();
        discardPile.Clear();

        for (int i = 0;
             i < currentDeck.Count;
             i++)
        {
            CardData card =
                currentDeck[i];

            if (card == null)
            {
                continue;
            }

            drawPile.Add(
                card
            );
        }
    }

    /// <summary>
    /// 현재 드로우 파일을 무작위로 섞습니다.
    /// </summary>
    private void ShuffleDrawPile()
    {
        for (int i = drawPile.Count - 1;
             i > 0;
             i--)
        {
            int randomIndex =
                Random.Range(
                    0,
                    i + 1
                );

            CardData temp =
                drawPile[i];

            drawPile[i] =
                drawPile[randomIndex];

            drawPile[randomIndex] =
                temp;
        }
    }

    /// <summary>
    /// 현재 덱을 기준으로 전투용 드로우 파일을 생성하고 섞습니다.
    /// </summary>
    public void PrepareDrawPileForBattle()
    {
        CreateDrawPileFromCurrentDeck();
        ShuffleDrawPile();

        Debug.Log(
            $"[DeckManager] 드로우 파일 생성 완료: " +
            $"{drawPile.Count}장"
        );
    }

    /// <summary>
    /// 새 게임 튜토리얼에서 사용할 첫 손패 네 장을
    /// 투창, 회피, 클래스 스킬 1, 클래스 스킬 2 순서로 고정합니다.
    /// 나머지 시작 덱 카드만 섞어 이후 드로우 순서로 사용합니다.
    /// </summary>
    /// <param name="playerClass">현재 선택한 플레이어 클래스</param>
    /// <returns>튜토리얼 드로우 파일 준비 성공 여부</returns>
    public bool PrepareTutorialDrawPile(
        PlayerClass playerClass)
    {
        string firstSkillCardID;
        string secondSkillCardID;

        switch (playerClass)
        {
            case PlayerClass.Physique:
                firstSkillCardID = "PHY_SKL_001";
                secondSkillCardID = "PHY_SKL_002";
                break;

            case PlayerClass.Technician:
                firstSkillCardID = "TEC_SKL_001";
                secondSkillCardID = "TEC_SKL_002";
                break;

            case PlayerClass.Captain:
                firstSkillCardID = "CAP_SKL_001";
                secondSkillCardID = "CAP_SKL_002";
                break;

            default:
                Debug.LogError(
                    $"[DeckManager] 튜토리얼 첫 손패를 지원하지 않는 클래스입니다: " +
                    $"{playerClass}"
                );
                return false;
        }

        List<CardData> remainingCards =
            new List<CardData>();

        for (int i = 0; i < currentDeck.Count; i++)
        {
            CardData card = currentDeck[i];

            if (card != null)
            {
                remainingCards.Add(card);
            }
        }

        string[] tutorialCardIDs =
        {
            "ALL_ATK_001",
            "ALL_DEF_001",
            firstSkillCardID,
            secondSkillCardID
        };
        List<CardData> tutorialOpeningCards =
            new List<CardData>();

        for (int i = 0; i < tutorialCardIDs.Length; i++)
        {
            CardData tutorialCard =
                TakeFirstCardByID(
                    remainingCards,
                    tutorialCardIDs[i]
                );

            if (tutorialCard == null)
            {
                Debug.LogError(
                    $"[DeckManager] 튜토리얼 첫 손패 카드가 시작 덱에 없습니다: " +
                    $"{tutorialCardIDs[i]}"
                );
                return false;
            }

            tutorialOpeningCards.Add(tutorialCard);
        }

        drawPile.Clear();
        discardPile.Clear();
        drawPile.AddRange(remainingCards);
        ShuffleDrawPile();
        drawPile.InsertRange(0, tutorialOpeningCards);

        Debug.Log(
            $"[DeckManager] 튜토리얼 드로우 파일 준비 완료 / " +
            $"첫 손패: 투창, 회피, {firstSkillCardID}, {secondSkillCardID} / " +
            $"남은 드로우 파일: {remainingCards.Count}장"
        );

        return true;
    }

    /// <summary>
    /// 카드 목록에서 지정 ID의 첫 카드 한 장을 꺼냅니다.
    /// 동일 공통 카드가 여러 장인 시작 덱에서도 정확히 한 장만 사용합니다.
    /// </summary>
    private CardData TakeFirstCardByID(
        List<CardData> cards,
        string cardID)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            CardData card = cards[i];

            if (card == null || card.cardID != cardID)
            {
                continue;
            }

            cards.RemoveAt(i);
            return card;
        }

        return null;
    }

    /// <summary>
    /// 드로우 파일에서 카드 한 장을 꺼내 반환합니다.
    ///
    /// 드로우 파일이 비어 있으면
    /// 버림 더미를 섞어서 다시 사용합니다.
    /// </summary>
    public CardData DrawOneCard()
    {
        if (drawPile.Count <= 0)
        {
            RefillDrawPileFromDiscardPile();
        }

        if (drawPile.Count <= 0)
        {
            Debug.LogWarning(
                "[DeckManager] 드로우할 카드가 없습니다."
            );

            return null;
        }

        CardData card =
            drawPile[0];

        drawPile.RemoveAt(0);

        return card;
    }

    /// <summary>
    /// 버림 더미의 카드를 드로우 파일로 옮기고 섞습니다.
    /// </summary>
    private void RefillDrawPileFromDiscardPile()
    {
        if (discardPile.Count <= 0)
        {
            Debug.LogWarning(
                "[DeckManager] 버림 더미도 비어 있어 " +
                "드로우 파일을 재생성할 수 없습니다."
            );

            return;
        }

        for (int i = 0;
             i < discardPile.Count;
             i++)
        {
            CardData card =
                discardPile[i];

            if (card == null)
            {
                continue;
            }

            drawPile.Add(
                card
            );
        }

        discardPile.Clear();

        ShuffleDrawPile();

        Debug.Log(
            $"[DeckManager] 버림 더미를 섞어 " +
            $"드로우 파일 재생성: {drawPile.Count}장"
        );
    }

    /// <summary>
    /// 카드를 버림 더미에 추가합니다.
    /// </summary>
    public void AddToDiscardPile(
        CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogWarning(
                "[DeckManager] 버린 카드 더미에 추가할 " +
                "카드 데이터가 없습니다."
            );

            return;
        }

        discardPile.Add(
            cardData
        );

        Debug.Log(
            $"[DeckManager] 버린 카드 더미 추가: " +
            $"{cardData.cardName}"
        );
    }

    /// <summary>
    /// 원본 카드 데이터를 복사해
    /// 현재 보유 덱에 새 카드 한 장을 추가합니다.
    /// </summary>
    public void AddCardToDeck(
        CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogWarning(
                "[DeckManager] 덱에 추가할 카드 데이터가 없습니다."
            );

            return;
        }

        CardData runtimeCard =
            CreateRuntimeCard(
                cardData
            );

        if (runtimeCard == null)
        {
            Debug.LogError(
                "[DeckManager] 런타임 카드 생성에 실패했습니다."
            );

            return;
        }

        currentDeck.Add(
            runtimeCard
        );

        SortCurrentDeckByCardName();

        Debug.Log(
            $"[DeckManager] 카드 덱 추가: " +
            $"{runtimeCard.cardName} / " +
            $"현재 덱 {currentDeck.Count}장"
        );
    }

    /// <summary>
    /// 전달받은 저장 카드 목록을 기준으로
    /// 현재 보유 덱을 복원합니다.
    ///
    /// 카드 한 장이라도 복원에 실패하면
    /// 기존 덱은 유지하고 전체 복원을 취소합니다.
    /// </summary>
    /// <param name="savedCards">
    /// 저장된 카드 ID와 강화 상태 목록
    /// </param>
    /// <returns>덱 복원 성공 여부</returns>
    public bool RestoreDeck(
        List<SavedCardData> savedCards)
    {
        if (savedCards == null ||
            savedCards.Count <= 0)
        {
            Debug.LogError(
                "[DeckManager] 복원할 카드 저장 목록이 비어 있습니다."
            );

            return false;
        }

        if (cardDatabase == null)
        {
            Debug.LogError(
                "[DeckManager] CardDatabase가 연결되지 않아 " +
                "덱을 복원할 수 없습니다."
            );

            return false;
        }

        /*
         * 기존 덱을 바로 제거하지 않고,
         * 임시 목록에 모든 카드 복원을 먼저 시도합니다.
         *
         * 중간에 실패하면 임시 카드만 제거하고
         * 기존 덱은 그대로 유지합니다.
         */
        List<CardData> restoredCards =
            new List<CardData>();

        for (int i = 0;
             i < savedCards.Count;
             i++)
        {
            SavedCardData savedCard =
                savedCards[i];

            if (savedCard == null ||
                string.IsNullOrWhiteSpace(
                    savedCard.cardID))
            {
                Debug.LogError(
                    $"[DeckManager] {i}번 저장 카드 정보가 " +
                    "올바르지 않습니다."
                );

                DestroyRuntimeCardList(
                    restoredCards
                );

                return false;
            }

            CardData originalCard =
                cardDatabase.GetCardByID(
                    savedCard.cardID
                );

            if (originalCard == null)
            {
                Debug.LogError(
                    $"[DeckManager] 저장 카드 원본을 찾지 못했습니다: " +
                    $"{savedCard.cardID}"
                );

                DestroyRuntimeCardList(
                    restoredCards
                );

                return false;
            }

            CardData runtimeCard =
                CreateRuntimeCard(
                    originalCard
                );

            if (runtimeCard == null)
            {
                Debug.LogError(
                    $"[DeckManager] 런타임 카드 생성 실패: " +
                    $"{savedCard.cardID}"
                );

                DestroyRuntimeCardList(
                    restoredCards
                );

                return false;
            }

            if (savedCard.isUpgraded)
            {
                bool upgradeSucceeded =
                    CardUpgradeUtility.UpgradeCard(
                        runtimeCard
                    );

                if (!upgradeSucceeded)
                {
                    Debug.LogError(
                        $"[DeckManager] 강화 상태 복원 실패: " +
                        $"{savedCard.cardID}"
                    );

                    Destroy(
                        runtimeCard
                    );

                    DestroyRuntimeCardList(
                        restoredCards
                    );

                    return false;
                }
            }

            restoredCards.Add(
                runtimeCard
            );
        }

        /*
         * 모든 카드 복원이 성공한 이후에만
         * 기존 런타임 덱을 제거하고 새 덱으로 교체합니다.
         */
        ClearCurrentDeck();

        currentDeck.AddRange(
            restoredCards
        );

        drawPile.Clear();
        discardPile.Clear();

        Debug.Log(
            $"[DeckManager] 저장 덱 복원 완료: " +
            $"{currentDeck.Count}장 / " +
            $"강화 카드: {CountUpgradedCards()}장"
        );

        return true;
    }

    /// <summary>
    /// 저장된 CurrentDeck 인덱스 순서대로
    /// 드로우 파일을 복원합니다.
    ///
    /// 이어하기에서는 이 순서를 그대로 사용하며
    /// 다시 셔플하지 않습니다.
    /// </summary>
    /// <param name="savedDrawPileIndices">
    /// CurrentDeck 기준 드로우 파일 카드 인덱스
    /// </param>
    /// <returns>드로우 파일 복원 성공 여부</returns>
    public bool RestoreDrawPile(
        List<int> savedDrawPileIndices)
    {
        if (savedDrawPileIndices == null)
        {
            Debug.LogError(
                "[DeckManager] 복원할 드로우 파일 인덱스가 없습니다."
            );

            return false;
        }

        if (currentDeck == null ||
            currentDeck.Count <= 0)
        {
            Debug.LogError(
                "[DeckManager] CurrentDeck이 비어 있어 " +
                "드로우 파일을 복원할 수 없습니다."
            );

            return false;
        }

        List<CardData> restoredDrawPile =
            new List<CardData>();

        HashSet<int> usedIndices =
            new HashSet<int>();

        for (int i = 0;
             i < savedDrawPileIndices.Count;
             i++)
        {
            int deckIndex =
                savedDrawPileIndices[i];

            if (deckIndex < 0 ||
                deckIndex >= currentDeck.Count)
            {
                Debug.LogError(
                    $"[DeckManager] 잘못된 드로우 파일 카드 인덱스입니다: " +
                    $"{deckIndex}"
                );

                return false;
            }

            if (!usedIndices.Add(deckIndex))
            {
                Debug.LogError(
                    $"[DeckManager] 드로우 파일 카드 인덱스가 " +
                    $"중복되었습니다: {deckIndex}"
                );

                return false;
            }

            CardData card =
                currentDeck[deckIndex];

            if (card == null)
            {
                Debug.LogError(
                    $"[DeckManager] CurrentDeck의 {deckIndex}번 카드가 null입니다."
                );

                return false;
            }

            restoredDrawPile.Add(card);
        }

        drawPile.Clear();
        drawPile.AddRange(restoredDrawPile);

        discardPile.Clear();

        Debug.Log(
            $"[DeckManager] 이어하기 드로우 파일 복원 완료: " +
            $"{drawPile.Count}장"
        );

        return true;
    }

    /// <summary>
    /// 전달받은 카드가 현재 클래스의
    /// 시작 덱 카드인지 확인합니다.
    /// </summary>
    public bool IsStartingDeckCard(
        CardData cardData)
    {
        if (cardData == null)
        {
            return false;
        }

        if (startingDeckDatabase == null)
        {
            Debug.LogWarning(
                "[DeckManager] StartingDeckDatabase가 연결되지 않았습니다."
            );

            return false;
        }

        if (GameManager.Instance == null ||
            GameManager.Instance.PlayerData == null)
        {
            Debug.LogWarning(
                "[DeckManager] PlayerData를 찾지 못했습니다."
            );

            return false;
        }

        PlayerClass playerClass =
            GameManager.Instance.PlayerData.PlayerClass;

        foreach (StartingDeckEntry entry
                 in startingDeckDatabase.entries)
        {
            if (entry == null)
            {
                continue;
            }

            if (entry.ownerClass != playerClass)
            {
                continue;
            }

            if (entry.cardID ==
                cardData.cardID)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 원본 카드 데이터로부터
    /// 현재 게임에서 사용할 독립적인 카드 한 장을 생성합니다.
    /// </summary>
    private CardData CreateRuntimeCard(
        CardData originalCard)
    {
        if (originalCard == null)
        {
            return null;
        }

        CardData runtimeCard =
            Instantiate(
                originalCard
            );

        runtimeCard.name =
            $"{originalCard.name}_Runtime";

        runtimeCard.ResetUpgradeState();

        return runtimeCard;
    }

    /// <summary>
    /// 현재 덱에 있는 런타임 카드들을 제거하고
    /// 모든 카드 더미를 초기화합니다.
    /// </summary>
    private void ClearCurrentDeck()
    {
        DestroyRuntimeCardList(
            currentDeck
        );

        currentDeck.Clear();
        drawPile.Clear();
        discardPile.Clear();
    }

    /// <summary>
    /// 전달받은 목록의 런타임 카드들을 제거합니다.
    ///
    /// Project의 원본 ScriptableObject는 제거하지 않고,
    /// 이름이 _Runtime으로 끝나는 실행 중 복사본만 제거합니다.
    /// </summary>
    private void DestroyRuntimeCardList(
        List<CardData> cards)
    {
        if (cards == null)
        {
            return;
        }

        for (int i = cards.Count - 1;
             i >= 0;
             i--)
        {
            CardData card =
                cards[i];

            if (card == null)
            {
                continue;
            }

            if (card.name.EndsWith(
                    "_Runtime",
                    System.StringComparison.Ordinal))
            {
                Destroy(
                    card
                );
            }
        }
    }

    /// <summary>
    /// 현재 덱의 강화 카드 수를 반환합니다.
    /// </summary>
    private int CountUpgradedCards()
    {
        int upgradedCount = 0;

        for (int i = 0;
             i < currentDeck.Count;
             i++)
        {
            CardData card =
                currentDeck[i];

            if (card != null &&
                card.IsUpgraded)
            {
                upgradedCount++;
            }
        }

        return upgradedCount;
    }
}
