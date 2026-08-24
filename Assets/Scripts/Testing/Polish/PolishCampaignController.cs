#if UNITY_EDITOR || POLISH_SIMULATION_BUILD

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

/// <summary>
/// 타이틀 F7에서 시작한 클래스별 30 Run 폴리싱 테스트를 씬 사이에서 유지합니다.
/// 현재 단계에서는 첫 Run의 클래스 자동 선택과 BattleScene 진입까지만 수행합니다.
/// </summary>
public class PolishCampaignController : MonoBehaviour
{
    private const string BattleSceneName = "BattleScene";
    private const string ClassSelectSceneName = "Class_SelectScene";
    private const string GameClearSceneName = "GameClearScene";
    private const float ClassSelectionTimeout = 8f;

    private PolishTestRunCoordinator runCoordinator;
    private PolishTestSessionState sessionState;
    private PolishSingleBattleController battleController;
    private PolishRewardAutomationController rewardController;
    private PolishRestAutomationController restController;
    private PolishTestLogger testLogger;
    private float previousTimeScale = 1f;
    private bool isTimeScaleApplied;
    private float sessionStartedAt;
    private bool quitApplicationOnComplete;

    /// <summary>
    /// 현재 실행 중인 폴리싱 캠페인이 출력 생략 고속 모드인지 반환합니다.
    /// Editor 전용 표현 생략 경로에서만 사용합니다.
    /// </summary>
    public static bool IsFastModeRunning { get; private set; }

    /// <summary>
    /// 타이틀 화면에서 새 폴리싱 테스트 세션을 만들고 첫 Run을 시작합니다.
    /// </summary>
    /// <param name="titleManager">사용자 저장을 보존하며 클래스 선택 씬을 열 TitleManager</param>
    /// <param name="fastMode">표현을 생략하고 고속으로 실행할지 여부</param>
    /// <param name="singleTestClass">단독 실행 클래스이며 None이면 전체 클래스</param>
    /// <param name="runsPerClass">클래스마다 실행할 Run 수</param>
    /// <param name="playbackSpeed">0보다 크면 적용할 명시적 실행 배속</param>
    /// <param name="outputRootFolder">결과를 저장할 루트 폴더</param>
    /// <param name="quitOnComplete">최종 보고서 생성 후 앱을 종료할지 여부</param>
    /// <returns>중복 세션이 없고 첫 Run을 시작했다면 true</returns>
    public static bool TryStartFromTitle(
        TitleManager titleManager,
        bool fastMode = false,
        PlayerClass singleTestClass = PlayerClass.None,
        int runsPerClass = 30,
        float playbackSpeed = 0f,
        string outputRootFolder = null,
        bool quitOnComplete = false)
    {
        if (titleManager == null)
        {
            return false;
        }

        PolishCampaignController existing =
            FindFirstObjectByType<PolishCampaignController>();
        if (existing != null)
        {
            Debug.LogWarning(
                "[PolishCampaignController] 이미 폴리싱 테스트 세션이 실행 중입니다.",
                existing);
            return false;
        }

        GameObject campaignObject = new GameObject("PolishCampaignController");
        campaignObject.AddComponent<PolishTestLogger>();
        campaignObject.AddComponent<PolishBattleObserver>();
        campaignObject.AddComponent<PolishHumanActionExecutor>();
        campaignObject.AddComponent<PolishSpecialChoiceAutomationController>();
        PolishSingleBattleController singleBattleController =
            campaignObject.AddComponent<PolishSingleBattleController>();
        PolishTutorialAutomationController tutorialController =
            campaignObject.AddComponent<PolishTutorialAutomationController>();
        PolishRewardAutomationController rewardAutomationController =
            campaignObject.AddComponent<PolishRewardAutomationController>();
        PolishRestAutomationController restAutomationController =
            campaignObject.AddComponent<PolishRestAutomationController>();
        PolishTestRunCoordinator coordinator =
            campaignObject.AddComponent<PolishTestRunCoordinator>();
        PolishCampaignController controller =
            campaignObject.AddComponent<PolishCampaignController>();
        if (!coordinator.ConfigureRunsPerClass(runsPerClass))
        {
            Debug.LogError(
                "[PolishCampaignController] 클래스별 Run 횟수 설정 실패",
                controller);
            Destroy(campaignObject);
            return false;
        }
        if (singleTestClass != PlayerClass.None &&
            !coordinator.ConfigureSingleClass(singleTestClass))
        {
            Debug.LogError(
                "[PolishCampaignController] 클래스 단독 테스트 설정 실패",
                controller);
            Destroy(campaignObject);
            return false;
        }
        controller.Initialize(
            coordinator,
            singleBattleController,
            tutorialController,
            rewardAutomationController,
            restAutomationController,
            fastMode,
            singleTestClass,
            runsPerClass,
            playbackSpeed,
            outputRootFolder,
            quitOnComplete);

        if (!coordinator.TryBeginNextRun())
        {
            Destroy(campaignObject);
            return false;
        }

        controller.sessionState.SetCurrentRun(
            coordinator.CurrentRunId,
            coordinator.CurrentPlayerClass,
            coordinator.CurrentSeed);

        if (!titleManager.StartPolishTestGame())
        {
            controller.Fail("타이틀 테스트 전용 새 게임 진입 실패");
            Destroy(campaignObject);
            return false;
        }

        Debug.Log(
            $"[PolishCampaignController] " +
            $"{(fastMode ? "Shift+F7 고속" : "F7 일반")} 세션 시작: " +
            $"{controller.sessionState.sessionId} / " +
            $"{coordinator.CurrentRunId}",
            controller);
        return true;
    }

    private void Initialize(
        PolishTestRunCoordinator coordinator,
        PolishSingleBattleController singleBattleController,
        PolishTutorialAutomationController tutorialController,
        PolishRewardAutomationController rewardAutomationController,
        PolishRestAutomationController restAutomationController,
        bool fastMode,
        PlayerClass singleTestClass,
        int runsPerClass,
        float playbackSpeed,
        string outputRootFolder,
        bool quitOnComplete)
    {
        runCoordinator = coordinator;
        battleController = singleBattleController;
        rewardController = rewardAutomationController;
        restController = restAutomationController;
        testLogger = GetComponent<PolishTestLogger>();
        battleController.ConfigureExecutionMode(fastMode);
        tutorialController.ConfigureExecutionMode(fastMode);
        tutorialController.Initialize(
            HandleTutorialCompleted,
            Fail);
        rewardController.Initialize(
            StartCurrentBattleAutomation,
            HandleProgressionPause,
            Fail);
        restController.Initialize(
            StartCurrentBattleAutomation,
            Fail);
        sessionState = PolishTestSessionState.Create(
            $"Session_{DateTime.Now:yyyyMMdd_HHmmss}",
            fastMode,
            singleTestClass,
            runsPerClass,
            playbackSpeed);
        quitApplicationOnComplete = quitOnComplete;
        IsFastModeRunning = fastMode;
        sessionStartedAt = Time.realtimeSinceStartup;
        ApplyTestTimeScale(sessionState.playbackSpeed);
        testLogger.ConfigureSessionOutput(
            sessionState.sessionId,
            outputRootFolder);
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        Application.logMessageReceived += HandleLogMessage;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Application.logMessageReceived -= HandleLogMessage;
        RestoreTimeScale();
        IsFastModeRunning = false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (sessionState == null || sessionState.phase == "Error")
        {
            return;
        }

        if (sessionState.isFastMode)
        {
            DisablePresentationForFastMode();
        }

        if (scene.name == BattleSceneName)
        {
            sessionState.phase = "Tutorial";
            GetComponent<PolishTutorialAutomationController>()?.
                BeginBattleEntryAutomation();
            return;
        }

        if (scene.name == GameClearSceneName && runCoordinator.IsRunActive)
        {
            CompleteRunAndContinue(PolishRunResult.Clear, string.Empty);
            return;
        }

        StartCoroutine(TrySelectCurrentClass());
    }

    private IEnumerator TrySelectCurrentClass()
    {
        yield return null;

        ClassSelectManager classSelectManager =
            FindFirstObjectByType<ClassSelectManager>();
        if (classSelectManager == null)
        {
            yield break;
        }

        if (runCoordinator == null)
        {
            Fail("클래스 선택 Manager 탐색 실패");
            yield break;
        }

        classSelectManager.SelectClass(runCoordinator.CurrentPlayerClass);
        float startedAt = Time.realtimeSinceStartup;
        while (!classSelectManager.CanConfirmSelection)
        {
            if (Time.realtimeSinceStartup - startedAt >= ClassSelectionTimeout)
            {
                Fail("클래스 상세 화면 준비 시간 초과");
                yield break;
            }

            yield return null;
        }

        sessionState.phase = "LoadingBattle";
        classSelectManager.ConfirmSelection();
    }

    private void HandleTutorialCompleted()
    {
        if (battleController == null || runCoordinator == null)
        {
            Fail("자동 전투 Controller 연결 실패");
            return;
        }

        StartCurrentBattleAutomation();
    }

    private void StartCurrentBattleAutomation()
    {
        StageManager stageManager = StageManager.Instance;
        EnemySpawner enemySpawner = FindFirstObjectByType<EnemySpawner>();
        PlayerData playerData = GameManager.Instance != null
            ? GameManager.Instance.PlayerData
            : null;
        if (battleController == null || stageManager == null ||
            enemySpawner == null || enemySpawner.CurrentBattleData == null ||
            playerData == null)
        {
            Fail("자동 전투 기록에 필요한 현재 전투 정보 탐색 실패");
            return;
        }

        int stage = stageManager.CurrentStage;
        int battleNumber = stageManager.CurrentBattleCount + 1;
        int entryHp = playerData.CurrentHP;
        testLogger?.BeginBattle(
            stage,
            battleNumber,
            enemySpawner.CurrentBattleData.BattleId,
            entryHp);

        sessionState.phase = "BattleRunning";
        if (!battleController.BeginBattle(runCoordinator.CurrentSeed))
        {
            Fail("자동 전투 시작 실패");
            return;
        }

        Debug.Log(
            $"[PolishCampaignController] 자동 전투 시작: " +
            $"{sessionState.currentRunId} / S{stage}B{battleNumber}",
            this);
        StartCoroutine(WaitForBattleCompletion(entryHp));
    }

    private IEnumerator WaitForBattleCompletion(int entryHp)
    {
        while (battleController != null && battleController.IsRunning)
        {
            yield return null;
        }

        if (battleController == null)
        {
            Fail("자동 전투 Controller가 사라졌습니다");
            yield break;
        }

        PlayerData playerData = GameManager.Instance != null
            ? GameManager.Instance.PlayerData
            : null;
        int exitHp = playerData != null ? playerData.CurrentHP : 0;
        bool won = battleController.Outcome ==
            PolishSingleBattleOutcome.Victory;
        testLogger?.CompleteBattle(
            won,
            exitHp,
            Mathf.Max(0, entryHp - exitHp),
            0,
            0);

        if (!won)
        {
            PolishRunResult result = battleController.Outcome ==
                PolishSingleBattleOutcome.Death
                    ? PolishRunResult.Death
                    : PolishRunResult.Error;
            CompleteRunAndContinue(result,
                $"자동 전투 종료: {battleController.Outcome}");
            yield break;
        }

        BattleManager battleManager = FindFirstObjectByType<BattleManager>();
        float battleEndStartedAt = Time.realtimeSinceStartup;
        while (battleManager != null && battleManager.IsBattleStarted)
        {
            if (Time.realtimeSinceStartup - battleEndStartedAt >= 15f)
            {
                Fail("적 처치 후 BattleManager 전투 종료 처리 시간 초과");
                yield break;
            }
            yield return null;
        }

        sessionState.phase = "Reward";
        rewardController.BeginRewardAutomation();
    }

    private void HandleProgressionPause(StagePhase phase)
    {
        if (phase == StagePhase.Rest && restController != null)
        {
            sessionState.phase = "Rest";
            restController.BeginRestAutomation();
            return;
        }

        if (phase == StagePhase.BossBattle)
        {
            sessionState.phase = "BossBattleLoading";
            StartCoroutine(WaitForFollowUpBossBattle());
            return;
        }

        sessionState.phase = $"{phase}Pending";
        Debug.Log(
            $"[PolishCampaignController] 후속 자동화 단계 대기: {phase}",
            this);
    }

    private IEnumerator WaitForFollowUpBossBattle()
    {
        float startedAt = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - startedAt < 20f)
        {
            EnemySpawner enemySpawner = FindFirstObjectByType<EnemySpawner>();
            if (enemySpawner != null &&
                enemySpawner.CurrentBattleData != null &&
                enemySpawner.GetActiveEnemies().Count > 0)
            {
                StartCurrentBattleAutomation();
                yield break;
            }
            yield return null;
        }

        Fail("연속 보스 전투 준비 시간 초과");
    }

    private void Fail(string message)
    {
        if (runCoordinator != null && runCoordinator.IsRunActive)
        {
            CompleteRunAndContinue(PolishRunResult.Error, message);
        }
        else
        {
            sessionState?.Fail(message);
            RestoreTimeScale();
        }
        Debug.LogError(
            $"[PolishCampaignController] 자동 캠페인 중단: {message}",
            this);
    }

    private void CompleteRunAndContinue(PolishRunResult result, string error)
    {
        StopAutomation();
        StageManager stageManager = StageManager.Instance;
        int stage = stageManager != null ? stageManager.CurrentStage : 0;
        int battle = stageManager != null ? stageManager.CurrentBattleCount : 0;
        runCoordinator.CompleteCurrentRun(result, stage, battle, error);
        sessionState.completedRuns++;
        if (runCoordinator.IsCampaignComplete)
        {
            sessionState.phase = "Complete";
            RestoreTimeScale();
            string report = PolishCampaignReportWriter.Write(
                testLogger.SessionOutputFolder,
                sessionState.completedRuns,
                sessionState.totalRuns,
                sessionState.executionMode,
                sessionState.selectedTestClass,
                Time.realtimeSinceStartup - sessionStartedAt);
            Debug.Log(
                $"[PolishCampaignController] {sessionState.totalRuns} Run 완료: " +
                report,
                this);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayDialog(
                "폴리싱 자동 테스트 완료",
                $"{sessionState.selectedTestClass} " +
                $"{sessionState.totalRuns} Run을 완료했습니다.\n{report}",
                "확인");
#endif
#if POLISH_SIMULATION_BUILD
            if (quitApplicationOnComplete)
            {
                StartCoroutine(QuitApplicationAfterReport());
            }
#endif
            return;
        }
        StartCoroutine(BeginNextRun());
    }

    private static void DisablePresentationForFastMode()
    {
        foreach (Camera targetCamera in FindObjectsByType<Camera>(
                     FindObjectsInactive.Exclude,
                     FindObjectsSortMode.None))
        {
            targetCamera.enabled = true;
            targetCamera.cullingMask = 0;
            targetCamera.clearFlags = CameraClearFlags.SolidColor;
            targetCamera.backgroundColor = Color.black;
        }

        foreach (AudioListener listener in FindObjectsByType<AudioListener>(
                     FindObjectsInactive.Exclude,
                     FindObjectsSortMode.None))
        {
            listener.enabled = false;
        }

        foreach (VideoPlayer videoPlayer in FindObjectsByType<VideoPlayer>(
                     FindObjectsInactive.Exclude,
                     FindObjectsSortMode.None))
        {
            videoPlayer.enabled = false;
        }

        foreach (Renderer targetRenderer in FindObjectsByType<Renderer>(
                     FindObjectsInactive.Exclude,
                     FindObjectsSortMode.None))
        {
            targetRenderer.enabled = false;
        }
    }

    private IEnumerator BeginNextRun()
    {
        if (sessionState == null || !sessionState.isFastMode)
        {
            yield return new WaitForSecondsRealtime(0.5f);
        }
        else
        {
            yield return null;
        }
        ContinueLoadContext.Clear();
        StageManager.Instance?.ResetProgress();
        GameManager.Instance?.InitializeGame();
        if (!runCoordinator.TryBeginNextRun())
        {
            Fail("다음 Run 발급 실패");
            yield break;
        }
        sessionState.SetCurrentRun(runCoordinator.CurrentRunId,
            runCoordinator.CurrentPlayerClass, runCoordinator.CurrentSeed);
        SceneManager.LoadScene(ClassSelectSceneName);
    }

#if POLISH_SIMULATION_BUILD
    private static IEnumerator QuitApplicationAfterReport()
    {
        yield return null;
        Application.Quit(0);
    }
#endif

    private void StopAutomation()
    {
        battleController?.StopBattle();
        GetComponent<PolishTutorialAutomationController>()?.StopAllCoroutines();
        rewardController?.StopRewardAutomation();
        restController?.StopAllCoroutines();
    }

    private void HandleLogMessage(string condition, string stackTrace, LogType type)
    {
        if (type != LogType.Error && type != LogType.Exception &&
            type != LogType.Assert)
        {
            return;
        }
        testLogger?.RecordIssue(runCoordinator?.CurrentRunId,
            runCoordinator != null ? runCoordinator.CurrentSeed : 0,
            condition, stackTrace);
    }

    private void ApplyTestTimeScale(float playbackSpeed)
    {
        if (!isTimeScaleApplied)
        {
            previousTimeScale = Time.timeScale;
            isTimeScaleApplied = true;
        }
        Time.timeScale = Mathf.Max(1f, playbackSpeed);
        Debug.Log($"[PolishCampaignController] 자동 테스트 배속 적용: x{Time.timeScale}", this);
    }

    private void RestoreTimeScale()
    {
        if (!isTimeScaleApplied)
        {
            return;
        }
        Time.timeScale = previousTimeScale;
        isTimeScaleApplied = false;
        Debug.Log($"[PolishCampaignController] 게임 배속 복원: x{Time.timeScale}", this);
    }
}

#endif
