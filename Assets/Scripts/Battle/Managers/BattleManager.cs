using UnityEngine;

/// <summary>
/// 전투 전체 흐름을 관리하는 클래스입니다.
/// 현재는 전투 시작 상태, 선택된 카드 정보, 카드 효과 실행을 관리합니다.
/// </summary>
public class BattleManager : MonoBehaviour
{
    /// <summary>
    /// 현재 전투가 시작된 상태인지 나타냅니다.
    /// 씬 진입 시 항상 false로 초기화됩니다.
    /// </summary>
    private bool isBattleStarted;

    [Header("Hand Manager")]
    [SerializeField]
    private HandManager handManager;

    [Header("Turn Manager")]
    [SerializeField]
    private TurnManager turnManager;

    [Header("Card Effect Executor")]
    [SerializeField]
    private CardEffectExecutor cardEffectExecutor;

    [Header("Reward Panel UI")]
    [SerializeField]
    private RewardPanelUI rewardPanelUI;

    [Header("적 태그")]
    [SerializeField]
    private string enemyTag = "Enemy";

    [Header("현재 선택된 카드 데이터")]
    [SerializeField]
    private CardData selectedCardData;

    [Header("Deck Manager")]
    [SerializeField]
    private DeckManager deckManager;

    [Header("Enemy Spawner")]
    [SerializeField]
    private EnemySpawner enemySpawner;

    [Header("Battle Database")]
    [SerializeField]
    private BattleDatabase battleDatabase;

    [Header("HRevelation Panel UI")]
    [SerializeField]
    private HRevelationPanelUI hRevelationPanelUI;

    [Header("Class Passive Controller")]
    [SerializeField]
    private ClassPassiveController classPassiveController;

    [Header("Stage Background Controller")]
    [SerializeField]
    private StageBackgroundController stageBackgroundController;

    private CardUI selectedCardUI;

    public CardData SelectedCardData => selectedCardData;
    public CardUI SelectedCardUI => selectedCardUI;

    private void Awake()
    {
        isBattleStarted = false;
        selectedCardData = null;
        selectedCardUI = null;
    }

    /// <summary>
    /// 현재 StageManager 진행도에 맞는 적을 생성하고
    /// 첫 전투를 시작합니다.
    ///
    /// DeckManager의 덱 준비와 HandManager의 첫 손패 생성이
    /// 완료된 이후 호출해야 합니다.
    /// </summary>
    /// <returns>전투 시작 성공 여부</returns>
    public bool StartInitialBattle()
    {
        if (isBattleStarted)
        {
            Debug.LogWarning(
                "[BattleManager] 이미 전투가 시작되어 있습니다."
            );

            return false;
        }

        if (enemySpawner == null)
        {
            Debug.LogError(
                "[BattleManager] EnemySpawner가 연결되지 않았습니다."
            );

            return false;
        }

        if (battleDatabase == null)
        {
            Debug.LogError(
                "[BattleManager] BattleDatabase가 연결되지 않았습니다."
            );

            return false;
        }

        EnemyBattleData battleData =
            GetCurrentEnemyBattleData();

        if (battleData == null)
        {
            Debug.LogError(
                "[BattleManager] 현재 진행도에 해당하는 " +
                "전투 데이터를 가져오지 못했습니다."
            );

            return false;
        }

        enemySpawner.SetBattleData(
            battleData
        );

        bool spawnSucceeded =
            enemySpawner.SpawnCurrentBattle();

        if (!spawnSucceeded)
        {
            Debug.LogError(
                "[BattleManager] 첫 전투 적 생성에 실패했습니다."
            );

            return false;
        }

        StartBattle();

        if (turnManager != null)
        {
            turnManager.StartPlayerTurn();
        }
        else
        {
            Debug.LogWarning(
                "[BattleManager] TurnManager가 연결되지 않았습니다."
            );

            return false;
        }

        /*
         * 첫 플레이어 턴 준비가 완료된 후
         * 클래스의 전투 시작 패시브를 실행합니다.
         *
         * 캡틴 선원은 이 순서로 소환해야
         * 첫 턴에 즉시 성장하지 않고 12/12로 시작합니다.
         */
        if (classPassiveController != null)
        {
            classPassiveController.OnBattleStarted();
        }
        else
        {
            Debug.LogWarning(
                "[BattleManager] ClassPassiveController가 연결되지 않아 " +
                "전투 시작 패시브를 실행하지 못했습니다."
            );
        }

        Debug.Log(
            "[BattleManager] 첫 전투 시작 완료"
        );

        return true;
    }

    /// <summary>
    /// StageManager의 현재 진행도에 맞는
    /// 전투 데이터를 BattleDatabase에서 가져옵니다.
    /// </summary>
    private EnemyBattleData GetCurrentEnemyBattleData()
    {
        StageManager stageManager =
            StageManager.Instance;

        if (stageManager == null)
        {
            stageManager =
                FindFirstObjectByType<StageManager>();
        }

        if (stageManager == null)
        {
            Debug.LogError(
                "[BattleManager] StageManager를 찾지 못했습니다."
            );

            return null;
        }

        switch (stageManager.CurrentPhase)
        {
            case StagePhase.NormalBattle:
                {
                    /*
                     * StageManager의 CurrentBattleCount는 0부터 시작하고,
                     * BattleDatabase의 Battle Count는 1부터 등록합니다.
                     *
                     * CurrentBattleCount 0 → Battle 1
                     * CurrentBattleCount 1 → Battle 2
                     * CurrentBattleCount 2 → Battle 3
                     */
                    int databaseBattleCount =
                        stageManager.CurrentBattleCount + 1;

                    Debug.Log(
                        $"[BattleManager] 일반 전투 데이터 요청 / " +
                        $"Stage: {stageManager.CurrentStage} / " +
                        $"StageManager Count: {stageManager.CurrentBattleCount} / " +
                        $"Database Battle: {databaseBattleCount}"
                    );

                    return battleDatabase.GetNormalBattleData(
                        stageManager.CurrentStage,
                        databaseBattleCount
                    );
                }

            case StagePhase.BossBattle:
                {
                    /*
                     * Stage 3은 모르바엘 → 아리엘 순서가 고정이므로
                     * 기존 Boss Sequence 방식으로 가져옵니다.
                     */
                    if (stageManager.CurrentStage == 3)
                    {
                        return battleDatabase.GetBossBattleData(
                            stageManager.CurrentStage,
                            stageManager.CurrentBossSequence
                        );
                    }

                    /*
                     * Stage 1, 2에서 이미 선택된 보스 ID가 있다면
                     * 이어하기 또는 동일 전투 재시작 상태이므로
                     * 랜덤 선택하지 않고 저장된 보스를 반환합니다.
                     */
                    if (!string.IsNullOrWhiteSpace(
                            stageManager.SelectedBossBattleID))
                    {
                        EnemyBattleData savedBossBattleData =
                            battleDatabase.GetBossBattleDataByID(
                                stageManager.SelectedBossBattleID
                            );

                        if (savedBossBattleData != null)
                        {
                            Debug.Log(
                                $"[BattleManager] 저장된 보스 전투 데이터 사용 / " +
                                $"Stage: {stageManager.CurrentStage} / " +
                                $"Battle ID: {stageManager.SelectedBossBattleID}"
                            );

                            return savedBossBattleData;
                        }

                        Debug.LogWarning(
                            $"[BattleManager] 저장된 보스 ID와 일치하는 " +
                            $"전투 데이터를 찾지 못했습니다. " +
                            $"새 보스를 다시 선택합니다. / " +
                            $"Battle ID: {stageManager.SelectedBossBattleID}"
                        );

                        stageManager.ClearSelectedBossBattleID();
                    }

                    /*
                     * 아직 보스가 선택되지 않은 경우에만
                     * 해당 스테이지 보스 후보 중 하나를 무작위로 결정합니다.
                     */
                    EnemyBattleData selectedBossBattleData =
                        battleDatabase.GetBossBattleData(
                            stageManager.CurrentStage,
                            stageManager.CurrentBossSequence
                        );

                    if (selectedBossBattleData == null)
                    {
                        Debug.LogError(
                            $"[BattleManager] Stage {stageManager.CurrentStage}의 " +
                            "보스 전투 데이터를 선택하지 못했습니다."
                        );

                        return null;
                    }

                    if (string.IsNullOrWhiteSpace(
                            selectedBossBattleData.BattleId))
                    {
                        Debug.LogError(
                            $"[BattleManager] 선택된 보스 전투 데이터의 " +
                            $"Battle ID가 비어 있습니다: " +
                            $"{selectedBossBattleData.name}"
                        );

                        return null;
                    }

                    stageManager.SetSelectedBossBattleID(
                        selectedBossBattleData.BattleId
                    );

                    Debug.Log(
                        $"[BattleManager] 새로운 보스 무작위 선택 및 저장 / " +
                        $"Stage: {stageManager.CurrentStage} / " +
                        $"Battle ID: {selectedBossBattleData.BattleId} / " +
                        $"Data: {selectedBossBattleData.name}"
                    );

                    return selectedBossBattleData;
                }

            case StagePhase.Rest:
                Debug.LogWarning(
                    "[BattleManager] 현재 휴식 단계이므로 " +
                    "적 전투 데이터를 불러오지 않습니다."
                );

                return null;

            default:
                Debug.LogError(
                    $"[BattleManager] 지원하지 않는 전투 단계입니다: " +
                    $"{stageManager.CurrentPhase}"
                );

                return null;
        }
    }

    /// <summary>
    /// 전투 시작 처리를 수행합니다.
    /// </summary>
    public void StartBattle()
    {
        isBattleStarted = true;
        selectedCardData = null;
        selectedCardUI = null;

        Debug.Log("[BattleManager] 전투 시작");

        if (StageManager.Instance != null)
        {
            Debug.Log(
                $"[BattleManager] 현재 진행도 - " +
                $"Stage : {StageManager.Instance.CurrentStage} / " +
                $"Phase : {StageManager.Instance.CurrentPhase} / " +
                $"Battle : {StageManager.Instance.CurrentBattleCount}"
            );
        }
        else
        {
            Debug.LogWarning(
                "[BattleManager] StageManager.Instance가 없습니다."
            );
        }

        TryOpenHRevelationPanel();
    }

    /// <summary>
    /// 다음 전투를 준비하고 시작합니다.
    /// </summary>
    public void StartNextBattle()
    {
        ScreenFadeController.ChangeBattle(
            StartNextBattleAfterFadeOut
        );
    }

    /// <summary>
    /// 화면이 완전히 가려진 뒤 다음 전투를 준비하고 시작합니다.
    /// </summary>
    private void StartNextBattleAfterFadeOut()
    {
        Debug.Log("[BattleManager] 다음 전투 준비 시작");

        isBattleStarted = false;

        ClearSelectedCard();
        ClearAllCrews();

        /*
        * 같은 BattleScene 안에서 다음 스테이지로 넘어갈 수 있으므로
        * 현재 StageManager의 스테이지에 맞게 배경을 다시 적용합니다.
        */
        if (stageBackgroundController == null)
        {
            stageBackgroundController =
                FindFirstObjectByType<StageBackgroundController>();
        }

        if (stageBackgroundController != null)
        {
            stageBackgroundController.ApplyCurrentStageBackground();
        }
        else
        {
            Debug.LogWarning(
                "[BattleManager] StageBackgroundController를 찾지 못해 " +
                "스테이지 배경을 갱신하지 못했습니다."
            );
        }

        if (deckManager == null)
        {
            Debug.LogError(
                "[BattleManager] DeckManager가 연결되지 않아 " +
                "다음 전투의 드로우 파일을 준비할 수 없습니다."
            );

            return;
        }

        if (handManager == null)
        {
            Debug.LogError(
                "[BattleManager] HandManager가 연결되지 않아 " +
                "다음 전투의 첫 손패를 준비할 수 없습니다."
            );

            return;
        }

        /*
         * 다음 전투에서는 기존 손패와 보존 상태를 완전히 초기화한 뒤,
         * 현재 전체 덱을 기준으로 드로우 파일을 새로 생성합니다.
         */
        handManager.ResetHandForNewBattle();
        deckManager.PrepareDrawPileForBattle();

        /*
         * TurnManager의 턴 시작 드로우에만 의존하지 않고
         * 다음 전투 첫 손패 4장을 여기서 명시적으로 생성합니다.
         *
         * 이후 StartPlayerTurn()이 호출되어도 손패가 이미 4장이므로
         * 추가 드로우는 발생하지 않습니다.
         */
        handManager.DrawCards(4);

        if (handManager.HandCards.Count <= 0)
        {
            Debug.LogError(
                "[BattleManager] 다음 전투 첫 손패 생성에 실패했습니다. " +
                $"현재 덱: {deckManager.CurrentDeck.Count}장 / " +
                $"드로우 파일: {deckManager.DrawPile.Count}장"
            );

            return;
        }

        Debug.Log(
            $"[BattleManager] 다음 전투 첫 손패 준비 완료 / " +
            $"손패: {handManager.HandCards.Count}장 / " +
            $"남은 드로우 파일: {deckManager.DrawPile.Count}장"
        );

        PlayerCombat playerCombat =
            FindFirstObjectByType<PlayerCombat>();

        if (playerCombat != null)
        {
            playerCombat.ResetCombat();

            StatusEffectHandler playerStatusEffectHandler =
                playerCombat.GetComponent<StatusEffectHandler>();

            if (playerStatusEffectHandler != null)
            {
                playerStatusEffectHandler.ClearAllStatusEffects();
            }
        }

        if (enemySpawner == null)
        {
            Debug.LogWarning(
                "[BattleManager] EnemySpawner가 연결되지 않았습니다."
            );

            return;
        }

        if (battleDatabase == null)
        {
            Debug.LogError(
                "[BattleManager] BattleDatabase가 연결되지 않았습니다."
            );

            return;
        }

        /*
         * 현재는 StageManager 자동 연결 전 테스트 단계이므로
         * Stage 1 / Battle 1 데이터를 임시로 사용합니다.
         */
        EnemyBattleData battleData =
        GetCurrentEnemyBattleData();

        if (battleData == null)
        {
            Debug.LogError(
                "[BattleManager] 현재 진행도에 해당하는 " +
                "전투 데이터를 가져오지 못했습니다."
            );

            return;
        }

        enemySpawner.SetBattleData(
            battleData
        );

        bool spawnSucceeded =
            enemySpawner.SpawnCurrentBattle();

        if (!spawnSucceeded)
        {
            Debug.LogError(
                "[BattleManager] 현재 전투의 적 생성에 실패했습니다."
            );

            return;
        }

        StartBattle();

        if (turnManager != null)
        {
            turnManager.StartPlayerTurn();
        }
        else
        {
            Debug.LogWarning(
                "[BattleManager] TurnManager가 연결되지 않았습니다."
            );

            return;
        }

        /*
         * 첫 플레이어 턴 준비가 완료된 뒤
         * 클래스의 전투 시작 패시브를 실행합니다.
         */
        if (classPassiveController != null)
        {
            classPassiveController.OnBattleStarted();
        }
        else
        {
            Debug.LogWarning(
                "[BattleManager] ClassPassiveController가 연결되지 않아 " +
                "다음 전투 시작 패시브를 실행하지 못했습니다."
            );
        }

        /*
         * 플레이어 첫 턴 준비가 끝난 뒤
         * 현재 전투 시작 상태를 메모리 스냅샷으로 저장합니다.
         */
        if (SaveManager.Instance != null)
        {
            bool captureSucceeded =
                SaveManager.Instance.CaptureBattleStartSnapshot();

            if (!captureSucceeded)
            {
                Debug.LogWarning(
                    "[BattleManager] 전투 시작 스냅샷 생성 실패"
                );
            }
        }

        Debug.Log(
            "[BattleManager] 다음 전투 시작 완료"
        );
    }

    /// <summary>
    /// 현재 생성된 적에게 HRevelationController가 있다면
    /// 전투 시작 시 계시 선택 패널을 표시합니다.
    /// </summary>
    private void TryOpenHRevelationPanel()
    {
        if (hRevelationPanelUI == null)
        {
            Debug.LogWarning(
                "[BattleManager] HRevelationPanelUI가 연결되지 않았습니다."
            );

            return;
        }

        HRevelationController controller =
            FindFirstObjectByType<HRevelationController>();

        if (controller == null)
        {
            Debug.Log(
                "[BattleManager] HRevelationController가 있는 적이 없습니다."
            );

            return;
        }

        hRevelationPanelUI.ShowPanel(controller);

        Debug.Log(
            "[BattleManager] 타락한 계시 선택 패널 열기 요청"
        );
    }

    /// <summary>
    /// 선택된 카드를 변경합니다.
    /// 같은 카드를 다시 선택하면 선택 해제합니다.
    /// </summary>
    public void SelectCard(CardUI cardUI)
    {
        Debug.Log("[BattleManager] SelectCard 호출됨");

        if (!isBattleStarted)
        {
            Debug.LogWarning(
                "[BattleManager] 아직 전투가 시작되지 않았습니다."
            );

            return;
        }

        if (cardUI == null)
        {
            Debug.LogWarning(
                "[BattleManager] 선택하려는 CardUI가 비어 있습니다."
            );

            return;
        }

        if (selectedCardUI == cardUI)
        {
            selectedCardUI.SetDeselected();

            selectedCardUI = null;
            selectedCardData = null;

            TutorialManager tutorialManager =
                FindFirstObjectByType<TutorialManager>();
            tutorialManager?.ClearTargetMarkers();

            Debug.Log("[BattleManager] 카드 선택 해제");
            return;
        }

        if (selectedCardUI != null)
        {
            selectedCardUI.SetDeselected();
        }

        selectedCardUI = cardUI;
        selectedCardData = cardUI.GetCardData();

        selectedCardUI.SetSelected();

        TutorialManager selectedTutorialManager =
            FindFirstObjectByType<TutorialManager>();
        selectedTutorialManager?.NotifyCardSelected(selectedCardData);

        Debug.Log(
            $"[BattleManager] 카드 선택 : " +
            $"{selectedCardData.cardName}"
        );
    }

    /// <summary>
    /// 현재 선택된 카드를 지정한 적에게 사용합니다.
    /// Echo가 있다면 공격 카드 효과를 한 번 더 실행합니다.
    /// </summary>
    public void UseSelectedCardOnEnemy(Enemy targetEnemy)
    {
        Debug.Log(
            "[BattleManager] UseSelectedCardOnEnemy 호출됨"
        );

        if (!CanUseSelectedCard())
        {
            return;
        }

        if (IsSingleCrewSacrificeCard())
        {
            Debug.LogWarning(
                "[BattleManager] 선택한 카드는 선원을 클릭해야 합니다."
            );

            return;
        }

        if (targetEnemy == null)
        {
            Debug.LogWarning(
                "[BattleManager] 대상 Enemy가 비어 있습니다."
            );

            return;
        }

        if (cardEffectExecutor == null)
        {
            Debug.LogError(
                "[BattleManager] CardEffectExecutor가 " +
                "연결되지 않았습니다."
            );

            return;
        }

        CardData usedCardData = selectedCardData;

        int damageModifier =
            GetBrokenWillDamageModifier(
                usedCardData
            );

        int damageMultiplier =
            TryConsumeDevilPower(usedCardData)
                ? 2
                : 1;

        bool shouldActivateEcho =
            TryConsumeEcho(usedCardData);

        Debug.Log(
            $"[BattleManager] 적에게 카드 사용 : " +
            $"{usedCardData.cardName}"
        );

        /*
         * 기본 카드 효과를 한 번 실행합니다.
         */
        cardEffectExecutor.ExecuteEffects(
            usedCardData,
            targetEnemy,
            damageModifier,
            damageMultiplier
        );

        /*
         * Echo가 발동했고 첫 번째 실행 이후에도 적이 살아 있다면
         * 같은 카드 효과를 한 번 더 실행합니다.
         *
         * 카드 제거와 사용 횟수 증가는 아래 FinishCardUse에서
         * 한 번만 처리됩니다.
         */
        if (shouldActivateEcho)
        {
            if (targetEnemy != null &&
                targetEnemy.gameObject.activeSelf &&
                targetEnemy.CurrentHP > 0)
            {
                Debug.Log(
                    $"[BattleManager] 잔상 발동 : " +
                    $"{usedCardData.cardName} 효과 재실행"
                );

                cardEffectExecutor.ExecuteEffects(
                    usedCardData,
                    targetEnemy,
                    damageModifier,
                    damageMultiplier
                );
            }
            else
            {
                Debug.Log(
                    "[BattleManager] 잔상이 발동했지만 " +
                    "첫 번째 공격으로 대상이 사망하여 " +
                    "두 번째 실행을 생략합니다."
                );
            }
        }

        FinishCardUse(usedCardData);
    }

    /// <summary>
    /// 현재 선택된 카드를 플레이어 자신에게 사용합니다.
    /// Self 대상 카드 처리를 위해 사용합니다.
    /// </summary>
    public void UseSelectedCardOnPlayer(
        PlayerCombat targetPlayer)
    {
        if (!CanUseSelectedCard())
        {
            return;
        }

        if (IsSingleCrewSacrificeCard())
        {
            Debug.LogWarning(
                "[BattleManager] 선택한 카드는 선원을 클릭해야 합니다."
            );

            return;
        }

        if (RequiresHarpoonStackDamageTarget())
        {
            Debug.LogWarning(
                "[BattleManager] 선택한 카드는 적을 클릭해야 합니다."
            );

            return;
        }

        if (targetPlayer == null)
        {
            Debug.LogWarning(
                "[BattleManager] 대상 PlayerCombat이 비어 있습니다."
            );

            return;
        }

        if (cardEffectExecutor == null)
        {
            Debug.LogError(
                "[BattleManager] CardEffectExecutor가 " +
                "연결되지 않았습니다."
            );

            return;
        }

        CardData usedCardData = selectedCardData;

        Debug.Log(
            $"[BattleManager] 플레이어에게 카드 사용 : " +
            $"{usedCardData.cardName}"
        );

        cardEffectExecutor.ExecuteEffects(
            usedCardData,
            null
        );

        FinishCardUse(usedCardData);
    }

    /// <summary>
    /// 현재 선택된 단일 선원 희생 카드를 지정한 선원에게 사용합니다.
    /// 선택 대상이 필요한 희생 카드 외에는 이 입력을 처리하지 않습니다.
    /// </summary>
    public void UseSelectedCardOnCrew(Crew targetCrew)
    {
        if (!CanUseSelectedCard())
        {
            return;
        }

        if (targetCrew == null || !IsSingleCrewSacrificeCard())
        {
            Debug.LogWarning(
                "[BattleManager] 선택한 카드는 단일 선원 " +
                "희생 대상 카드가 아닙니다."
            );

            return;
        }

        if (cardEffectExecutor == null)
        {
            Debug.LogError(
                "[BattleManager] CardEffectExecutor가 " +
                "연결되지 않았습니다."
            );

            return;
        }

        CardData usedCardData = selectedCardData;

        cardEffectExecutor.ExecuteEffects(
            usedCardData,
            null,
            0,
            1,
            targetCrew
        );

        FinishCardUse(usedCardData);
    }

    /// <summary>
    /// 선택된 카드가 선원 한 명을 직접 지정해 희생하는 카드인지 확인합니다.
    /// </summary>
    private bool IsSingleCrewSacrificeCard()
    {
        if (selectedCardData == null ||
            selectedCardData.effects == null)
        {
            return false;
        }

        foreach (CardEffectData effect in selectedCardData.effects)
        {
            if (effect.effectType == CardEffectType.Sacrifice &&
                effect.value == 1 &&
                effect.target == CardTargetType.Undead)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 선택된 카드가 적의 작살 스택을 기준으로 피해를 주는지 확인합니다.
    /// </summary>
    private bool RequiresHarpoonStackDamageTarget()
    {
        if (selectedCardData == null ||
            selectedCardData.effects == null)
        {
            return false;
        }

        foreach (CardEffectData effect in selectedCardData.effects)
        {
            if (effect.effectType ==
                CardEffectType.DealDamageEqualToHarpoonerStack)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 사용하려는 카드가 공격 카드이고
    /// 플레이어가 Echo를 보유 중이라면 Echo를 제거하고
    /// true를 반환합니다.
    /// </summary>

    /// <summary>
    /// 공격 카드 사용 시 무너진 의지를 확인하고
    /// 이번 카드에 적용할 피해 보정값을 반환합니다.
    /// </summary>
    private int GetBrokenWillDamageModifier(
        CardData usedCardData)
    {
        if (usedCardData == null)
        {
            return 0;
        }

        if (usedCardData.cardType != CardType.Attack)
        {
            return 0;
        }

        HRevelationController revelationController =
            FindFirstObjectByType<HRevelationController>();

        if (revelationController == null)
        {
            return 0;
        }

        if (!revelationController.TryConsumeBrokenWill())
        {
            return 0;
        }

        return -4;
    }

    /// <summary>
    /// 다음에 사용하는 카드가 공격 카드이고
    /// 플레이어가 악마의 힘을 보유 중이라면
    /// 악마의 힘을 제거하고 true를 반환합니다.
    /// </summary>
    private bool TryConsumeDevilPower(
        CardData usedCardData)
    {
        if (usedCardData == null)
        {
            return false;
        }

        if (usedCardData.cardType != CardType.Attack)
        {
            return false;
        }

        PlayerCombat playerCombat =
            FindFirstObjectByType<PlayerCombat>();

        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[BattleManager] 악마의 힘 확인을 위한 " +
                "PlayerCombat을 찾지 못했습니다."
            );

            return false;
        }

        StatusEffectHandler statusEffectHandler =
            playerCombat.GetComponent<StatusEffectHandler>();

        if (statusEffectHandler == null)
        {
            return false;
        }

        if (!statusEffectHandler.HasStatusEffect(
                StatusEffectType.DevilPower))
        {
            return false;
        }

        statusEffectHandler.RemoveStatusEffect(
            StatusEffectType.DevilPower
        );

        Debug.Log(
            "[BattleManager] 악마의 힘 소비 : " +
            "이번 공격 카드 피해를 2배로 적용합니다."
        );

        return true;
    }

    private bool TryConsumeEcho(CardData usedCardData)
    {
        if (usedCardData == null)
        {
            return false;
        }

        if (usedCardData.cardType != CardType.Attack)
        {
            return false;
        }

        PlayerCombat playerCombat =
            FindFirstObjectByType<PlayerCombat>();

        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[BattleManager] Echo 확인을 위한 " +
                "PlayerCombat을 찾지 못했습니다."
            );

            return false;
        }

        StatusEffectHandler statusEffectHandler =
            playerCombat.GetComponent<StatusEffectHandler>();

        if (statusEffectHandler == null)
        {
            return false;
        }

        if (!statusEffectHandler.HasStatusEffect(
            StatusEffectType.Echo
        ))
        {
            return false;
        }

        /*
         * 공격 카드 실행 전에 Echo를 제거합니다.
         * 반복 실행 중 다시 Echo가 적용되더라도
         * 현재 Echo와 섞이지 않도록 먼저 소비합니다.
         */
        statusEffectHandler.RemoveStatusEffect(
            StatusEffectType.Echo
        );

        Debug.Log(
            "[BattleManager] 잔상 소비 : " +
            "다음 공격 카드를 2회 실행합니다."
        );

        return true;
    }

    /// <summary>
    /// 선택된 카드를 사용할 수 있는지 확인합니다.
    /// 전투 시작 여부, 카드 선택 여부, 턴 상태,
    /// 카드 사용 제한을 검사합니다.
    /// </summary>
    private bool CanUseSelectedCard()
    {
        Debug.Log(
            $"[BattleManager] CanUseSelectedCard 호출 / " +
            $"isBattleStarted : {isBattleStarted}"
        );

        if (!isBattleStarted)
        {
            Debug.LogWarning(
                "[BattleManager] 아직 전투가 시작되지 않았습니다."
            );

            return false;
        }

        if (selectedCardData == null)
        {
            Debug.LogWarning(
                "[BattleManager] 사용할 카드가 선택되지 않았습니다."
            );

            return false;
        }

        if (handManager != null &&
        handManager.IsCardJinxed(selectedCardUI))
        {
            Debug.LogWarning(
                $"[BattleManager] Jinx 적용 카드라 사용할 수 없습니다 : " +
                $"{selectedCardData.cardName}"
            );

            return false;
        }

        if (turnManager == null)
        {
            Debug.LogError(
                "[BattleManager] TurnManager가 연결되지 않았습니다."
            );

            return false;
        }

        return turnManager.CanUseCard(selectedCardData);
    }

    /// <summary>
    /// 카드 사용 성공 후 공통 처리를 수행합니다.
    /// 사용 횟수 기록, 손패 제거, 버림 더미 이동,
    /// 선택 해제를 처리합니다.
    /// </summary>
    private void FinishCardUse(CardData usedCardData)
    {
        if (usedCardData == null)
        {
            Debug.LogWarning(
                "[BattleManager] 사용 완료 처리할 " +
                "카드 데이터가 없습니다."
            );

            return;
        }

        /*
         * Echo로 효과가 두 번 실행되더라도
         * 카드 사용 횟수는 한 번만 증가합니다.
         */
        if (turnManager != null)
        {
            turnManager.RecordCardUse(usedCardData);
        }

        /*
         * Echo로 효과가 두 번 실행되더라도
         * 손패에서는 카드 한 장만 제거합니다.
         */
        if (handManager != null)
        {
            handManager.DiscardUsedCard(usedCardData);
        }
        else
        {
            Debug.LogError(
                "[BattleManager] HandManager가 연결되지 않았습니다."
            );
        }

        ClearSelectedCard();

        TutorialManager tutorialManager =
            FindFirstObjectByType<TutorialManager>();

        if (tutorialManager != null)
        {
            tutorialManager.NotifyCardUsed(usedCardData);
        }
    }

    /// <summary>
    /// 현재 선택된 카드를 강제로 해제합니다.
    /// </summary>
    public void ClearSelectedCard()
    {
        if (selectedCardUI != null)
        {
            selectedCardUI.SetDeselected();
        }

        selectedCardUI = null;
        selectedCardData = null;

        TutorialManager tutorialManager =
            FindFirstObjectByType<TutorialManager>();
        tutorialManager?.ClearTargetMarkers();

        Debug.Log("[BattleManager] 선택 카드 초기화");
    }

    /// <summary>
    /// Enemy 태그를 가진 오브젝트를 찾아
    /// Enemy 컴포넌트를 반환합니다.
    /// </summary>
    private Enemy FindTargetEnemyByTag()
    {
        GameObject enemyObject =
            GameObject.FindGameObjectWithTag(enemyTag);

        if (enemyObject == null)
        {
            return null;
        }

        Enemy enemy = enemyObject.GetComponent<Enemy>();

        if (enemy == null)
        {
            Debug.LogWarning(
                "[BattleManager] Enemy 태그 오브젝트에 " +
                "Enemy 컴포넌트가 없습니다."
            );

            return null;
        }

        return enemy;
    }

    /// <summary>
    /// 모든 적이 사망했는지 확인합니다.
    /// 모든 적이 사망했다면 전투를 종료합니다.
    /// </summary>
    public void CheckBattleEnd()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (Enemy enemy in enemies)
        {
            if (enemy != null &&
                enemy.gameObject.activeSelf &&
                enemy.CurrentHP > 0)
            {
                Debug.Log(
                    "[BattleManager] 아직 살아있는 적이 있습니다."
                );

                return;
            }
        }

        EndBattle();
    }

#if UNITY_EDITOR

    /// <summary>
    /// 테스트 편의를 위해 현재 일반 전투를 즉시 승리 처리합니다.
    /// 보스 전투와 시작 전 또는 이미 종료된 전투에서는 실행하지 않습니다.
    /// </summary>
    /// <returns>일반 전투 종료 처리를 실행했다면 true를 반환합니다.</returns>
    public bool SkipNormalBattleForTesting()
    {
        if (!isBattleStarted)
        {
            Debug.LogWarning(
                "[BattleManager] 시작되지 않았거나 이미 종료된 전투는 " +
                "넘길 수 없습니다."
            );

            return false;
        }

        StageManager stageManager = StageManager.Instance;

        if (stageManager == null)
        {
            stageManager = FindFirstObjectByType<StageManager>();
        }

        if (stageManager == null ||
            stageManager.CurrentPhase != StagePhase.NormalBattle)
        {
            Debug.LogWarning(
                "[BattleManager] F10 전투 넘기기는 일반 전투에서만 사용할 수 있습니다."
            );

            return false;
        }

        Debug.Log(
            "[BattleManager] F10 테스트 단축키 - 일반 전투 즉시 승리"
        );

        EndBattle();
        return true;
    }

#endif

    /// <summary>
    /// 현재 전투에 소환된 모든 선원을 제거합니다.
    /// </summary>
    private void ClearAllCrews()
    {
        CrewManager crewManager =
            FindFirstObjectByType<CrewManager>();

        if (crewManager == null)
        {
            return;
        }

        crewManager.ClearAllCrews();

        Debug.Log(
            "[BattleManager] 전투 종료 선원 초기화"
        );
    }

    /// <summary>
    /// 전투 종료 처리를 수행합니다.
    /// 일반 전투와 보스 전투의 승리 처리를 구분합니다.
    ///
    /// Stage 3 모르바엘 처치:
    /// 리워드를 표시한 후 아리엘 전투로 진행합니다.
    ///
    /// Stage 3 아리엘 처치:
    /// 리워드를 표시하지 않고 게임 클리어 처리합니다.
    /// </summary>
    private void EndBattle()
    {
        isBattleStarted = false;

        ClearSelectedCard();
        ClearAllCrews();

        if (enemySpawner != null)
        {
            enemySpawner.ClearEnemies();
        }

        Debug.Log(
            "[BattleManager] 전투 종료 - 모든 적 처치"
        );

        /*
        * 모든 클래스가 공통으로 받는
        * 전투 종료 후 최대 체력 15% 회복입니다.
        */
        HealPlayerAfterBattle();

        /*
         * 공통 회복 처리 후
         * 현재 클래스의 전투 종료 패시브를 실행합니다.
         *
         * 피지크는 이 시점에 체력을 추가로 7 회복합니다.
         */
        if (classPassiveController != null)
        {
            classPassiveController.OnBattleEnded();
        }
        else
        {
            Debug.LogWarning(
                "[BattleManager] ClassPassiveController가 연결되지 않아 " +
                "클래스 전투 종료 패시브를 실행하지 못했습니다."
            );
        }

        StageManager stageManager =
            StageManager.Instance;

        if (stageManager == null)
        {
            stageManager =
                FindFirstObjectByType<StageManager>();
        }

        if (stageManager == null)
        {
            Debug.LogWarning(
                "[BattleManager] StageManager를 찾지 못했습니다. " +
                "전투 진행 상태를 변경할 수 없습니다."
            );

            ShowRewardPanel(false);
            return;
        }

        /*
         * 일반 전투 승리 처리
         */
        if (stageManager.CurrentPhase ==
            StagePhase.NormalBattle)
        {
            stageManager.BattleWin();

            ShowRewardPanel(false);
            return;
        }

        /*
         * 보스 전투 승리 처리
         */
        if (stageManager.CurrentPhase ==
            StagePhase.BossBattle)
        {
            /*
             * BossBattleWin 호출 전 현재 전투가
             * 아리엘 전투였는지 저장합니다.
             *
             * BossBattleWin 이후에는 게임 클리어 상태가
             * 변경되기 때문에 호출 전에 확인합니다.
             */
            bool wasStage3ArielBattle =
                stageManager.IsStage3ArielBattle;

            stageManager.BossBattleWin();

            /*
             * 아리엘 처치 후에는
             * 리워드 패널을 표시하지 않습니다.
             */
            if (wasStage3ArielBattle &&
                stageManager.IsGameClear)
            {
                Debug.Log(
                    "[BattleManager] 아리엘 처치 완료 - " +
                    "리워드 없이 게임 클리어"
                );

                return;
            }

            /*
             * Stage 1·2 보스와 모르바엘은
             * 기존처럼 리워드를 표시합니다.
             */
            ShowRewardPanel(true);
            return;
        }

        Debug.LogWarning(
            $"[BattleManager] 처리할 수 없는 전투 단계입니다: " +
            $"{stageManager.CurrentPhase}"
        );
    }

    /// <summary>
    /// 전투 승리 리워드 패널을 표시합니다.
    /// </summary>
    private void ShowRewardPanel(bool isBossReward)
    {
        if (rewardPanelUI != null)
        {
            rewardPanelUI.ShowRewardPanel(isBossReward);
        }
        else
        {
            Debug.LogWarning(
                "[BattleManager] RewardPanelUI가 " +
                "연결되지 않았습니다."
            );
        }
    }

    /// <summary>
    /// 전투 종료 후 플레이어 최대 체력의 15%를 회복합니다.
    /// </summary>
    private void HealPlayerAfterBattle()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.PlayerData == null)
        {
            Debug.LogWarning(
                "[BattleManager] PlayerData를 찾지 못해 " +
                "전투 후 회복을 처리할 수 없습니다."
            );

            return;
        }

        PlayerData playerData =
            GameManager.Instance.PlayerData;

        int healAmount =
            Mathf.FloorToInt(playerData.MaxHP * 0.15f);

        playerData.Heal(healAmount);

        Debug.Log(
            $"[BattleManager] 전투 후 회복 : " +
            $"최대 체력의 15% ({healAmount})"
        );
    }
}
