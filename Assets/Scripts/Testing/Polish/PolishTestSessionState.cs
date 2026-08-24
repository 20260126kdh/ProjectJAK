using System;

/// <summary>
/// 타이틀에서 시작한 전체 폴리싱 테스트 세션의 현재 상태입니다.
/// </summary>
[Serializable]
public class PolishTestSessionState
{
    public string sessionId;
    public string phase;
    public int completedRuns;
    public int totalRuns = 90;
    public string currentRunId;
    public string currentClass;
    public int currentSeed;
    public string errorMessage;
    public float playbackSpeed = 4f;
    public bool isFastMode;
    public string executionMode = "Normal";
    public string selectedTestClass = "All";

    /// <summary>
    /// 새 클래스별 30 Run 세션의 초기 상태를 생성합니다.
    /// </summary>
    /// <param name="newSessionId">결과 폴더와 보고서에 사용할 세션 ID</param>
    /// <param name="fastMode">고속 실행 여부</param>
    /// <param name="selectedClass">단독 실행 클래스이며 None이면 전체 클래스</param>
    /// <param name="runsPerClass">클래스마다 실행할 Run 수</param>
    /// <param name="playbackSpeedOverride">0보다 크면 적용할 명시적 실행 배속</param>
    /// <returns>타이틀 시작 상태의 새 세션</returns>
    public static PolishTestSessionState Create(
        string newSessionId,
        bool fastMode = false,
        PlayerClass selectedClass = PlayerClass.None,
        int runsPerClass = 30,
        float playbackSpeedOverride = 0f)
    {
        int safeRunsPerClass = Math.Max(1, runsPerClass);
        float playbackSpeed = playbackSpeedOverride > 0f
            ? playbackSpeedOverride
            : fastMode ? 20f : 4f;
        return new PolishTestSessionState
        {
            sessionId = newSessionId ?? string.Empty,
            phase = "Starting",
            completedRuns = 0,
            totalRuns = selectedClass == PlayerClass.None
                ? safeRunsPerClass * 3
                : safeRunsPerClass,
            isFastMode = fastMode,
            executionMode = fastMode ? "Fast30MinuteTarget" : "Normal",
            selectedTestClass = selectedClass == PlayerClass.None
                ? "All"
                : selectedClass.ToString(),
            playbackSpeed = playbackSpeed
        };
    }

    /// <summary>
    /// 현재 발급된 Run 정보를 세션 상태에 반영합니다.
    /// </summary>
    /// <param name="runId">클래스 접두사가 포함된 Run ID</param>
    /// <param name="playerClass">현재 테스트 클래스</param>
    /// <param name="seed">현재 Run 재현 Seed</param>
    public void SetCurrentRun(
        string runId,
        PlayerClass playerClass,
        int seed)
    {
        currentRunId = runId ?? string.Empty;
        currentClass = playerClass.ToString();
        currentSeed = seed;
        phase = "SelectingClass";
    }

    /// <summary>
    /// 세션을 오류 상태로 전환하고 원인을 저장합니다.
    /// </summary>
    /// <param name="message">자동 진행을 중단한 원인</param>
    public void Fail(string message)
    {
        phase = "Error";
        errorMessage = message ?? string.Empty;
    }
}
