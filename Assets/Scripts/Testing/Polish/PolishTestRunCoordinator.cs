using UnityEngine;

/// <summary>
/// Physique, Technician, Captain 순서로 클래스별 30 Run의 ID와 Seed를 발급합니다.
/// 실제 씬 시작과 AutoPlayer 연결은 후속 단계에서 담당합니다.
/// </summary>
public class PolishTestRunCoordinator : MonoBehaviour
{
    [SerializeField] private int runsPerClass = 30;
    [SerializeField] private int baseSeed = 20260822;
    [SerializeField] private PolishTestLogger testLogger;

    private readonly PlayerClass[] testClasses =
    {
        PlayerClass.Physique,
        PlayerClass.Technician,
        PlayerClass.Captain
    };

    private int classIndex;
    private int runIndex;
    private bool isRunActive;
    private PlayerClass currentPlayerClass = PlayerClass.None;
    private int currentSeed;
    private string currentRunId = string.Empty;
    private bool isSingleClassMode;
    private PlayerClass singleTestClass = PlayerClass.None;

    /// <summary>
    /// 현재 Run이 실행 중인지 반환합니다.
    /// </summary>
    public bool IsRunActive => isRunActive;

    /// <summary>
    /// 전체 90 Run 발급이 끝났는지 반환합니다.
    /// </summary>
    public bool IsCampaignComplete => isSingleClassMode
        ? classIndex >= 1
        : classIndex >= testClasses.Length;

    /// <summary>
    /// 선택한 클래스만 30회 실행하고 종료하도록 Run 발급 범위를 제한합니다.
    /// 첫 Run 발급 전에만 호출해야 합니다.
    /// </summary>
    /// <param name="playerClass">단독 테스트할 플레이어 클래스</param>
    /// <returns>지원하는 클래스로 설정했다면 true</returns>
    public bool ConfigureSingleClass(PlayerClass playerClass)
    {
        if (isRunActive || runIndex != 0 || classIndex != 0 ||
            playerClass == PlayerClass.None)
        {
            return false;
        }

        isSingleClassMode = true;
        singleTestClass = playerClass;
        return true;
    }

    /// <summary>
    /// 클래스별 실행 횟수를 첫 Run 발급 전에 설정합니다.
    /// </summary>
    /// <param name="runCount">클래스마다 실행할 횟수</param>
    /// <returns>유효한 시점과 횟수로 설정했다면 true</returns>
    public bool ConfigureRunsPerClass(int runCount)
    {
        if (isRunActive || runIndex != 0 || classIndex != 0 || runCount <= 0)
        {
            return false;
        }

        runsPerClass = runCount;
        return true;
    }

    /// <summary>
    /// 현재 발급된 Run의 플레이어 클래스입니다.
    /// </summary>
    public PlayerClass CurrentPlayerClass => currentPlayerClass;

    /// <summary>
    /// 현재 발급된 Run의 재현 Seed입니다.
    /// </summary>
    public int CurrentSeed => currentSeed;

    /// <summary>
    /// 현재 발급된 Run ID입니다.
    /// </summary>
    public string CurrentRunId => currentRunId;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (testLogger == null)
        {
            testLogger = GetComponent<PolishTestLogger>();
        }
    }

    /// <summary>
    /// 다음 클래스 Run의 ID와 Seed를 발급하고 기록을 시작합니다.
    /// 실제 게임 초기화는 아직 수행하지 않습니다.
    /// </summary>
    /// <returns>새 Run을 시작했으면 true를 반환합니다.</returns>
    public bool TryBeginNextRun()
    {
        if (isRunActive || IsCampaignComplete || testLogger == null)
        {
            return false;
        }

        PlayerClass playerClass = isSingleClassMode
            ? singleTestClass
            : testClasses[classIndex];
        int displayRunNumber = runIndex + 1;
        int seed = PolishTestSeedController.CreateRunSeed(
            baseSeed,
            playerClass,
            displayRunNumber);
        string runId = $"{GetClassPrefix(playerClass)}-{displayRunNumber:000}";

        currentPlayerClass = playerClass;
        currentSeed = seed;
        currentRunId = runId;
        PolishTestSeedController.Initialize(seed);
        testLogger.BeginRun(runId, playerClass, seed);
        isRunActive = true;

        Debug.Log(
            $"[PolishTestRunCoordinator] Run 준비: {runId} / " +
            $"Class: {playerClass} / Seed: {seed}");
        return true;
    }

    /// <summary>
    /// 현재 Run 결과를 저장하고 다음 Run 번호로 이동합니다.
    /// </summary>
    public void CompleteCurrentRun(
        PolishRunResult result,
        int finalStage,
        int finalBattle,
        string errorMessage = "")
    {
        if (!isRunActive || testLogger == null)
        {
            return;
        }

        testLogger.CompleteRun(result, finalStage, finalBattle, errorMessage);
        isRunActive = false;
        runIndex++;

        if (runIndex >= runsPerClass)
        {
            runIndex = 0;
            classIndex++;
        }
    }

    private static string GetClassPrefix(PlayerClass playerClass)
    {
        switch (playerClass)
        {
            case PlayerClass.Physique: return "PHY";
            case PlayerClass.Technician: return "TEC";
            case PlayerClass.Captain: return "CAP";
            default: return "INVALID";
        }
    }
}
