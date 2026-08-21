using NUnit.Framework;

/// <summary>
/// F7 수동 전투 테스트의 Seed와 결과 변환을 검증합니다.
/// </summary>
public class PolishManualBattleTestRunnerTests
{
    [Test]
    public void CreateManualSeed_SameBattle_ReturnsSameSeed()
    {
        int first = PolishManualBattleTestRunner.CreateManualSeed(
            PlayerClass.Physique,
            1,
            2);
        int second = PolishManualBattleTestRunner.CreateManualSeed(
            PlayerClass.Physique,
            1,
            2);

        Assert.AreEqual(first, second);
    }

    [Test]
    public void CreateManualSeed_DifferentClass_ReturnsDifferentSeed()
    {
        int physique = PolishManualBattleTestRunner.CreateManualSeed(
            PlayerClass.Physique,
            1,
            1);
        int captain = PolishManualBattleTestRunner.CreateManualSeed(
            PlayerClass.Captain,
            1,
            1);

        Assert.AreNotEqual(physique, captain);
    }

    [Test]
    public void ConvertRunResult_MapsTerminalOutcomes()
    {
        Assert.AreEqual(
            PolishRunResult.Clear,
            PolishManualBattleTestRunner.ConvertRunResult(
                PolishSingleBattleOutcome.Victory));
        Assert.AreEqual(
            PolishRunResult.Death,
            PolishManualBattleTestRunner.ConvertRunResult(
                PolishSingleBattleOutcome.Death));
        Assert.AreEqual(
            PolishRunResult.Error,
            PolishManualBattleTestRunner.ConvertRunResult(
                PolishSingleBattleOutcome.Timeout));
    }
}
