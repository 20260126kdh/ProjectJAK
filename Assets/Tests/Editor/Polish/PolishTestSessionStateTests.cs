using NUnit.Framework;

/// <summary>
/// 전체 폴리싱 테스트 세션의 초기화와 상태 전환을 검증합니다.
/// </summary>
public class PolishTestSessionStateTests
{
    [Test]
    public void Create_InitializesNinetyRunSession()
    {
        PolishTestSessionState state =
            PolishTestSessionState.Create("Session_Test");

        Assert.AreEqual("Session_Test", state.sessionId);
        Assert.AreEqual("Starting", state.phase);
        Assert.AreEqual(90, state.totalRuns);
        Assert.AreEqual(0, state.completedRuns);
        Assert.AreEqual(4f, state.playbackSpeed);
        Assert.IsFalse(state.isFastMode);
        Assert.AreEqual("Normal", state.executionMode);
    }

    [Test]
    public void Create_FastMode_UsesThirtyMinuteTargetSettings()
    {
        PolishTestSessionState state =
            PolishTestSessionState.Create(
                "Session_Fast",
                true,
                PlayerClass.Physique);

        Assert.IsTrue(state.isFastMode);
        Assert.AreEqual("Fast30MinuteTarget", state.executionMode);
        Assert.AreEqual(20f, state.playbackSpeed);
        Assert.AreEqual(30, state.totalRuns);
        Assert.AreEqual("Physique", state.selectedTestClass);
    }

    [Test]
    public void SetCurrentRun_StoresReproductionInformation()
    {
        PolishTestSessionState state =
            PolishTestSessionState.Create("Session_Test");

        state.SetCurrentRun("PHY-001", PlayerClass.Physique, 1234);

        Assert.AreEqual("SelectingClass", state.phase);
        Assert.AreEqual("PHY-001", state.currentRunId);
        Assert.AreEqual("Physique", state.currentClass);
        Assert.AreEqual(1234, state.currentSeed);
    }

    [Test]
    public void Create_CustomRunsAndSpeed_UsesRequestedValues()
    {
        PolishTestSessionState state = PolishTestSessionState.Create(
            "Session_Custom",
            true,
            PlayerClass.Captain,
            12,
            16f);

        Assert.AreEqual(12, state.totalRuns);
        Assert.AreEqual(16f, state.playbackSpeed);
        Assert.AreEqual("Captain", state.selectedTestClass);
    }

    [Test]
    public void Fail_StoresErrorAndStopsProgress()
    {
        PolishTestSessionState state =
            PolishTestSessionState.Create("Session_Test");

        state.Fail("Class selection timeout");

        Assert.AreEqual("Error", state.phase);
        Assert.AreEqual("Class selection timeout", state.errorMessage);
    }
}
