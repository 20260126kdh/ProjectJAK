using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// 단일 전투 자동 진행의 종료 조건과 안전 제한을 검증합니다.
/// </summary>
public class PolishSingleBattleControllerTests
{
    [Test]
    public void EvaluateOutcome_PlayerHpIsZero_ReturnsDeath()
    {
        PolishVisibleBattleSnapshot snapshot = CreateActiveSnapshot();
        snapshot.playerHp = 0;

        Assert.AreEqual(
            PolishSingleBattleOutcome.Death,
            Evaluate(snapshot));
    }

    [Test]
    public void EvaluateOutcome_NoEnemies_ReturnsVictory()
    {
        PolishVisibleBattleSnapshot snapshot = CreateActiveSnapshot();
        snapshot.enemies.Clear();

        Assert.AreEqual(
            PolishSingleBattleOutcome.Victory,
            Evaluate(snapshot));
    }

    [Test]
    public void ResolveAuthoritativeOutcome_NoEnemiesButBattleRunning_Waits()
    {
        Assert.AreEqual(
            PolishSingleBattleOutcome.InProgress,
            PolishSingleBattleController.ResolveAuthoritativeOutcome(
                PolishSingleBattleOutcome.Victory,
                true,
                10f,
                180f));
    }

    [Test]
    public void ResolveAuthoritativeOutcome_BattleEnded_ReturnsVictory()
    {
        Assert.AreEqual(
            PolishSingleBattleOutcome.Victory,
            PolishSingleBattleController.ResolveAuthoritativeOutcome(
                PolishSingleBattleOutcome.Victory,
                false,
                10f,
                180f));
    }

    [Test]
    public void ResolveAuthoritativeOutcome_BattleNeverEnds_ReturnsTimeout()
    {
        Assert.AreEqual(
            PolishSingleBattleOutcome.Timeout,
            PolishSingleBattleController.ResolveAuthoritativeOutcome(
                PolishSingleBattleOutcome.Victory,
                true,
                180f,
                180f));
    }

    [Test]
    public void EvaluateOutcome_ActionLimitReached_ReturnsTimeout()
    {
        PolishVisibleBattleSnapshot snapshot = CreateActiveSnapshot();

        Assert.AreEqual(
            PolishSingleBattleOutcome.Timeout,
            PolishSingleBattleController.EvaluateOutcome(
                snapshot,
                300,
                300,
                10f,
                180f));
    }

    [Test]
    public void EvaluateOutcome_ActiveBattle_ReturnsInProgress()
    {
        Assert.AreEqual(
            PolishSingleBattleOutcome.InProgress,
            Evaluate(CreateActiveSnapshot()));
    }

    private static PolishSingleBattleOutcome Evaluate(
        PolishVisibleBattleSnapshot snapshot)
    {
        return PolishSingleBattleController.EvaluateOutcome(
            snapshot,
            10,
            300,
            10f,
            180f);
    }

    private static PolishVisibleBattleSnapshot CreateActiveSnapshot()
    {
        return new PolishVisibleBattleSnapshot
        {
            playerHp = 75,
            enemies = new List<PolishVisibleEnemySnapshot>
            {
                new PolishVisibleEnemySnapshot { hp = 20 }
            }
        };
    }
}
