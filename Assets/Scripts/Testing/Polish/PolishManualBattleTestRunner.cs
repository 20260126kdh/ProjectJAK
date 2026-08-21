#if UNITY_EDITOR

using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Editor Play Mode의 현재 전투를 F7로 한 번 자동 진행하고 결과 파일을 저장합니다.
/// 보상 선택과 다음 전투 이동은 수행하지 않습니다.
/// </summary>
public class PolishManualBattleTestRunner : MonoBehaviour
{
    private const int ManualBaseSeed = 20260822;

    private PolishTestLogger testLogger;
    private PolishSingleBattleController battleController;
    private PlayerData playerData;
    private int stage;
    private int battleNumber;
    private int entryHp;
    private string runId;

    /// <summary>
    /// 현재 BattleScene 전투의 자동 테스트를 시작합니다.
    /// </summary>
    /// <returns>필수 전투 정보가 유효하고 테스트를 시작했다면 true</returns>
    public static bool TryStartCurrentBattle()
    {
        PolishManualBattleTestRunner existing =
            FindFirstObjectByType<PolishManualBattleTestRunner>();
        if (existing != null)
        {
            Debug.LogWarning(
                "[PolishManualBattleTestRunner] 이미 자동 전투 테스트가 실행 중입니다.",
                existing);
            return false;
        }

        StageManager stageManager = StageManager.Instance;
        PlayerData currentPlayerData = GameManager.Instance != null
            ? GameManager.Instance.PlayerData
            : null;
        EnemySpawner enemySpawner = FindFirstObjectByType<EnemySpawner>();
        if (stageManager == null || currentPlayerData == null ||
            enemySpawner == null || enemySpawner.CurrentBattleData == null)
        {
            Debug.LogError(
                "[PolishManualBattleTestRunner] 현재 전투 정보를 찾지 못해 " +
                "F7 자동 테스트를 시작할 수 없습니다.");
            return false;
        }

        GameObject runnerObject = new GameObject("PolishManualBattleTestRunner");
        PolishTestLogger logger = runnerObject.AddComponent<PolishTestLogger>();
        runnerObject.AddComponent<PolishBattleObserver>();
        runnerObject.AddComponent<PolishHumanActionExecutor>();
        PolishSingleBattleController controller =
            runnerObject.AddComponent<PolishSingleBattleController>();
        PolishManualBattleTestRunner runner =
            runnerObject.AddComponent<PolishManualBattleTestRunner>();

        return runner.Begin(
            logger,
            controller,
            currentPlayerData,
            stageManager,
            enemySpawner.CurrentBattleData);
    }

    /// <summary>
    /// 현재 전투 정보로 재현 가능한 수동 테스트 Seed를 생성합니다.
    /// </summary>
    /// <param name="playerClass">현재 플레이어 클래스</param>
    /// <param name="currentStage">현재 스테이지</param>
    /// <param name="currentBattleNumber">현재 전투 번호</param>
    /// <returns>판단과 게임 RNG 초기화에 사용할 Seed</returns>
    public static int CreateManualSeed(
        PlayerClass playerClass,
        int currentStage,
        int currentBattleNumber)
    {
        unchecked
        {
            int seed = ManualBaseSeed;
            seed = seed * 397 ^ (int)playerClass;
            seed = seed * 397 ^ currentStage;
            seed = seed * 397 ^ currentBattleNumber;
            return seed;
        }
    }

    /// <summary>
    /// 단일 전투 종료 결과를 수동 Run 결과로 변환합니다.
    /// </summary>
    /// <param name="outcome">단일 전투 자동 진행 결과</param>
    /// <returns>JSON과 CSV에 저장할 Run 결과</returns>
    public static PolishRunResult ConvertRunResult(
        PolishSingleBattleOutcome outcome)
    {
        switch (outcome)
        {
            case PolishSingleBattleOutcome.Victory:
                return PolishRunResult.Clear;
            case PolishSingleBattleOutcome.Death:
                return PolishRunResult.Death;
            case PolishSingleBattleOutcome.Timeout:
            case PolishSingleBattleOutcome.ExecutionError:
                return PolishRunResult.Error;
            default:
                return PolishRunResult.Invalid;
        }
    }

    private bool Begin(
        PolishTestLogger logger,
        PolishSingleBattleController controller,
        PlayerData currentPlayerData,
        StageManager stageManager,
        EnemyBattleData battleData)
    {
        testLogger = logger;
        battleController = controller;
        playerData = currentPlayerData;
        stage = stageManager.CurrentStage;
        battleNumber = stageManager.CurrentBattleCount + 1;
        entryHp = playerData.CurrentHP;

        int seed = CreateManualSeed(
            playerData.PlayerClass,
            stage,
            battleNumber);
        runId =
            $"MANUAL-{playerData.PlayerClass}-S{stage}B{battleNumber}-" +
            DateTime.Now.ToString("yyyyMMdd-HHmmss-fff");

        PolishTestSeedController.Initialize(seed);
        testLogger.BeginRun(runId, playerData.PlayerClass, seed);
        testLogger.BeginBattle(
            stage,
            battleNumber,
            battleData.BattleId,
            entryHp);

        if (!battleController.BeginBattle(seed))
        {
            testLogger.CompleteRun(
                PolishRunResult.Error,
                stage,
                battleNumber,
                "단일 전투 컨트롤러 시작 실패");
            Destroy(gameObject);
            return false;
        }

        Debug.Log(
            $"[PolishManualBattleTestRunner] F7 자동 테스트 시작: " +
            $"{runId} / Seed:{seed}",
            this);
        StartCoroutine(WaitForBattleResult());
        return true;
    }

    private IEnumerator WaitForBattleResult()
    {
        while (battleController.IsRunning)
        {
            yield return null;
        }

        PolishSingleBattleOutcome outcome = battleController.Outcome;
        int exitHp = playerData != null ? playerData.CurrentHP : 0;
        bool won = outcome == PolishSingleBattleOutcome.Victory;
        int netDamageTaken = Mathf.Max(0, entryHp - exitHp);

        testLogger.CompleteBattle(
            won,
            exitHp,
            netDamageTaken,
            0,
            0);
        PolishRunResult runResult = ConvertRunResult(outcome);
        string error = runResult == PolishRunResult.Error
            ? outcome.ToString()
            : string.Empty;
        testLogger.CompleteRun(
            runResult,
            stage,
            battleNumber,
            error);

        string outputFolder = System.IO.Path.Combine(
            Application.persistentDataPath,
            "PolishTestResults");
        Debug.Log(
            $"[PolishManualBattleTestRunner] 자동 테스트 종료: " +
            $"{outcome} / 결과: {outputFolder} / {runId}.json",
            this);
        Destroy(gameObject);
    }
}

#endif
