using UnityEngine;

/// <summary>
/// Physique, Technician, Captain 순서로 클래스별 50 Run의 ID와 Seed를 발급합니다.
/// 실제 씬 시작과 AutoPlayer 연결은 후속 단계에서 담당합니다.
/// </summary>
public class PolishTestRunCoordinator : MonoBehaviour
{
    [SerializeField] private int runsPerClass = 50;
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

    /// <summary>
    /// 현재 Run이 실행 중인지 반환합니다.
    /// </summary>
    public bool IsRunActive => isRunActive;

    /// <summary>
    /// 전체 150 Run 발급이 끝났는지 반환합니다.
    /// </summary>
    public bool IsCampaignComplete => classIndex >= testClasses.Length;

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

        PlayerClass playerClass = testClasses[classIndex];
        int displayRunNumber = runIndex + 1;
        int seed = PolishTestSeedController.CreateRunSeed(
            baseSeed,
            playerClass,
            displayRunNumber);
        string runId = $"{GetClassPrefix(playerClass)}-{displayRunNumber:000}";

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
