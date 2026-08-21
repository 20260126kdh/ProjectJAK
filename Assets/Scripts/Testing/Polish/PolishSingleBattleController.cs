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
    private int actionCount;
    private float startedAt;
    private bool isRunning;
    private bool wasPlayerTurn;
    private int playerTurnNumber;
    private PolishTurnRecord currentTurnRecord;
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

    private IEnumerator RunBattle()
    {
        while (true)
        {
            PolishVisibleBattleSnapshot snapshot = observer.Capture();
            outcome = EvaluateOutcome(
                snapshot,
                actionCount,
                maxActionCount,
                Time.unscaledTime - startedAt,
                battleTimeout);

            if (outcome != PolishSingleBattleOutcome.InProgress)
            {
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
            yield return new WaitForSecondsRealtime(
                Mathf.Max(0.05f, actionInterval));
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
    }

    private void FlushTurnRecord()
    {
        if (currentTurnRecord == null)
        {
            return;
        }

        testLogger?.RecordTurn(currentTurnRecord);
        currentTurnRecord = null;
    }
}
