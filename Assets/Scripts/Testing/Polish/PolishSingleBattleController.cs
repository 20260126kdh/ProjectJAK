#if UNITY_EDITOR || POLISH_SIMULATION_BUILD
using System.Collections;
using UnityEngine;

/// <summary>
/// 단일 전투 자동 진행 결과입니다.
/// </summary>
public enum PolishSingleBattleOutcome
{
    None,
    InProgress,
    Victory,
    Death,
    Timeout,
    ExecutionError
}

/// <summary>
/// 공개 전투 정보의 관찰, 인간형 판단과 실제 행동 실행을 단일 전투 동안 반복합니다.
/// 보상 선택이나 다음 전투 진입은 수행하지 않습니다.
/// </summary>
public class PolishSingleBattleController : MonoBehaviour
{
    [SerializeField] private PolishBattleObserver observer;
    [SerializeField] private PolishHumanActionExecutor actionExecutor;
    [SerializeField] private PolishTestLogger testLogger;
    [SerializeField] private float actionInterval = 0.35f;
    [SerializeField] private float battleTimeout = 180f;
    [SerializeField] private int maxActionCount = 300;

    private Coroutine battleRoutine;
    private PolishHumanDecisionEngine decisionEngine;
    private HandManager handManager;
    private PolishSpecialChoiceAutomationController specialChoiceController;
    private int actionCount;
    private float startedAt;
    private bool isRunning;
    private bool wasPlayerTurn;
    private int playerTurnNumber;
    private PolishTurnRecord currentTurnRecord;
    private PolishTurnRecord lastCompletedTurnRecord;
    private bool deathRecorded;
    private PolishSingleBattleOutcome outcome;

    /// <summary>
    /// 현재 단일 전투 자동 진행이 실행 중인지 반환합니다.
    /// </summary>
    public bool IsRunning => isRunning;

    /// <summary>
    /// 현재 또는 가장 최근 단일 전투의 자동 진행 결과입니다.
    /// </summary>
    public PolishSingleBattleOutcome Outcome => outcome;

    /// <summary>
    /// 현재 전투에서 전달한 행동 수입니다.
    /// </summary>
    public int ActionCount => actionCount;

    /// <summary>
    /// 자동 테스트 모드에 맞춰 카드 행동 사이의 실시간 대기를 설정합니다.
    /// </summary>
    /// <param name="fastMode">30분 목표 고속 모드 여부</param>
    public void ConfigureExecutionMode(bool fastMode)
    {
        actionInterval = fastMode ? 0.01f : 0.35f;
    }

    /// <summary>
    /// 현재 열린 전투의 자동 진행을 시작합니다.
    /// </summary>
    /// <param name="decisionSeed">게임 RNG와 분리된 판단용 Seed</param>
    /// <returns>필수 구성 요소를 찾아 시작했다면 true</returns>
    public bool BeginBattle(int decisionSeed)
    {
        if (isRunning)
        {
            return false;
        }

        ResolveDependencies();
        if (observer == null || actionExecutor == null || handManager == null)
        {
            outcome = PolishSingleBattleOutcome.ExecutionError;
            return false;
        }

        decisionEngine = new PolishHumanDecisionEngine(decisionSeed);
        actionCount = 0;
        playerTurnNumber = 0;
        wasPlayerTurn = false;
        currentTurnRecord = null;
        lastCompletedTurnRecord = null;
        deathRecorded = false;
        startedAt = Time.unscaledTime;
        outcome = PolishSingleBattleOutcome.InProgress;
        isRunning = true;
        battleRoutine = StartCoroutine(RunBattle());
        return true;
    }

    /// <summary>
    /// 진행 중인 단일 전투 자동 입력만 중지합니다.
    /// 실제 전투 상태는 변경하지 않습니다.
    /// </summary>
    public void StopBattle()
    {
        if (battleRoutine != null)
        {
            StopCoroutine(battleRoutine);
            battleRoutine = null;
        }

        isRunning = false;
        FlushTurnRecord();

        if (outcome == PolishSingleBattleOutcome.InProgress)
        {
            outcome = PolishSingleBattleOutcome.None;
        }
    }

    /// <summary>
    /// 공개 스냅샷과 안전 제한을 기준으로 전투 종료 여부를 판정합니다.
    /// </summary>
    /// <param name="snapshot">현재 공개 전투 상태</param>
    /// <param name="currentActionCount">현재까지 실행한 행동 수</param>
    /// <param name="actionLimit">허용할 최대 행동 수</param>
    /// <param name="elapsedSeconds">전투 자동 진행 경과 시간</param>
    /// <param name="timeoutSeconds">허용할 최대 경과 시간</param>
    /// <returns>계속 진행하거나 종료할 결과</returns>
    public static PolishSingleBattleOutcome EvaluateOutcome(
        PolishVisibleBattleSnapshot snapshot,
        int currentActionCount,
        int actionLimit,
        float elapsedSeconds,
        float timeoutSeconds)
    {
        if (snapshot == null)
        {
            return PolishSingleBattleOutcome.ExecutionError;
        }

        if (snapshot.playerHp <= 0)
        {
            return PolishSingleBattleOutcome.Death;
        }

        if (snapshot.enemies == null || snapshot.enemies.Count == 0)
        {
            return PolishSingleBattleOutcome.Victory;
        }

        if (currentActionCount >= actionLimit ||
            elapsedSeconds >= timeoutSeconds)
        {
            return PolishSingleBattleOutcome.Timeout;
        }

        return PolishSingleBattleOutcome.InProgress;
    }

    /// <summary>
    /// 화면에서 적이 사라진 뒤 실제 전투 종료 처리까지 반영하여 최종 결과를 판정합니다.
    /// 적 교체나 소환 직전의 빈 프레임을 승리로 오인하지 않도록 BattleManager 상태를 우선합니다.
    /// </summary>
    /// <param name="evaluatedOutcome">화면 공개 정보로 먼저 판정한 결과</param>
    /// <param name="isBattleStarted">BattleManager의 실제 전투 진행 상태</param>
    /// <param name="elapsedSeconds">현재 전투의 실제 경과 시간</param>
    /// <param name="timeoutSeconds">허용할 최대 경과 시간</param>
    /// <returns>실제 전투 상태까지 반영한 최종 판정</returns>
    public static PolishSingleBattleOutcome ResolveAuthoritativeOutcome(
        PolishSingleBattleOutcome evaluatedOutcome,
        bool isBattleStarted,
        float elapsedSeconds,
        float timeoutSeconds)
    {
        if (evaluatedOutcome != PolishSingleBattleOutcome.Victory ||
            !isBattleStarted)
        {
            return evaluatedOutcome;
        }

        return elapsedSeconds >= timeoutSeconds
            ? PolishSingleBattleOutcome.Timeout
            : PolishSingleBattleOutcome.InProgress;
    }

    private IEnumerator RunBattle()
    {
        while (true)
        {
            PolishVisibleBattleSnapshot snapshot = observer.Capture();
            float elapsedSeconds = Time.unscaledTime - startedAt;
            outcome = EvaluateOutcome(
                snapshot,
                actionCount,
                maxActionCount,
                elapsedSeconds,
                battleTimeout);
            BattleManager battleManager = FindFirstObjectByType<BattleManager>();
            outcome = ResolveAuthoritativeOutcome(
                outcome,
                battleManager != null && battleManager.IsBattleStarted,
                elapsedSeconds,
                battleTimeout);

            if (outcome != PolishSingleBattleOutcome.InProgress)
            {
                if (outcome == PolishSingleBattleOutcome.Death)
                {
                    RecordDeath(snapshot);
                }
                FlushTurnRecord();
                isRunning = false;
                battleRoutine = null;
                yield break;
            }

            if (!snapshot.isPlayerTurn)
            {
                wasPlayerTurn = false;
                yield return null;
                continue;
            }

            if (specialChoiceController != null &&
                specialChoiceController.TryHandleOpenChoice(snapshot))
            {
                yield return new WaitForSecondsRealtime(0.2f);
                continue;
            }

            if (handManager.IsCardFlowBusy)
            {
                yield return null;
                continue;
            }

            if (!wasPlayerTurn)
            {
                playerTurnNumber++;
                currentTurnRecord = PolishTurnRecordBuilder.Create(
                    snapshot,
                    playerTurnNumber);
                wasPlayerTurn = true;
            }

            PolishHumanDecision decision = decisionEngine.Decide(snapshot);
            PolishActionExecutionResult executionResult =
                actionExecutor.Execute(decision);
            PolishTurnRecordBuilder.AppendAction(
                currentTurnRecord,
                decision,
                executionResult,
                snapshot);
            if (executionResult != PolishActionExecutionResult.Success)
            {
                Debug.LogError(
                    $"[PolishSingleBattleController] 행동 실행 실패: " +
                    $"{executionResult}",
                    this);
                outcome = PolishSingleBattleOutcome.ExecutionError;
                FlushTurnRecord();
                isRunning = false;
                battleRoutine = null;
                yield break;
            }

            actionCount++;
            if (decision.decisionType == PolishDecisionType.EndTurn)
            {
                FlushTurnRecord();
                wasPlayerTurn = false;
            }
            if (actionInterval <= 0.01f)
            {
                yield return null;
            }
            else
            {
                yield return new WaitForSecondsRealtime(
                    Mathf.Max(0.05f, actionInterval));
            }
        }
    }

    private void ResolveDependencies()
    {
        if (observer == null)
        {
            observer = GetComponent<PolishBattleObserver>();
        }

        if (actionExecutor == null)
        {
            actionExecutor = GetComponent<PolishHumanActionExecutor>();
        }

        if (testLogger == null)
        {
            testLogger = FindFirstObjectByType<PolishTestLogger>();
        }

        handManager = FindFirstObjectByType<HandManager>();
        specialChoiceController =
            FindFirstObjectByType<PolishSpecialChoiceAutomationController>();
    }

    private void FlushTurnRecord()
    {
        if (currentTurnRecord == null)
        {
            return;
        }

        testLogger?.RecordTurn(currentTurnRecord);
        lastCompletedTurnRecord = currentTurnRecord;
        currentTurnRecord = null;
    }

    private void RecordDeath(PolishVisibleBattleSnapshot snapshot)
    {
        if (deathRecorded || testLogger == null || snapshot == null)
        {
            return;
        }

        StageManager stageManager = StageManager.Instance;
        EnemySpawner enemySpawner = FindFirstObjectByType<EnemySpawner>();
        PolishTurnRecord sourceTurn = currentTurnRecord ?? lastCompletedTurnRecord;
        PolishDeathRecord record = new PolishDeathRecord
        {
            stage = stageManager != null ? stageManager.CurrentStage : 0,
            battle = stageManager != null ? stageManager.CurrentBattleCount + 1 : 0,
            encounter = enemySpawner?.CurrentBattleData != null
                ? enemySpawner.CurrentBattleData.BattleId
                : string.Empty,
            turn = sourceTurn != null ? sourceTurn.turn : playerTurnNumber,
            hp = snapshot.playerHp,
            block = snapshot.playerBlock,
            lastDamage = sourceTurn != null
                ? Mathf.Max(0, sourceTurn.hp + sourceTurn.block -
                               snapshot.playerHp - snapshot.playerBlock)
                : 0,
            damageSource = sourceTurn != null
                ? string.Join(" | ", sourceTurn.enemyIntents)
                : string.Empty,
            primaryCause = "IncomingDamage"
        };

        foreach (PolishVisibleCardSnapshot card in snapshot.hand)
        {
            if (card != null)
            {
                record.hand.Add($"{card.cardId}|{card.displayName}");
            }
        }
        foreach (PolishVisibleStatusSnapshot status in snapshot.playerStatuses)
        {
            if (status != null)
            {
                record.statusEffects.Add(
                    $"{status.type}|Value:{status.value}|Turn:{status.remainingTurn}");
            }
        }
        if (sourceTurn != null)
        {
            record.recentActions.AddRange(sourceTurn.actions);
        }

        testLogger.RecordDeath(record);
        deathRecorded = true;
    }
}
#endif
