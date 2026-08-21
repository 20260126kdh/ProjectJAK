using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// 공개 전투 정보와 판단 결과의 턴 기록 변환을 검증합니다.
/// </summary>
public class PolishTurnRecordBuilderTests
{
    [Test]
    public void Create_CapturesVisibleBattleInformation()
    {
        PolishVisibleBattleSnapshot snapshot = CreateSnapshot();

        PolishTurnRecord record = PolishTurnRecordBuilder.Create(snapshot, 2);

        Assert.AreEqual(2, record.turn);
        Assert.AreEqual(61, record.hp);
        Assert.AreEqual(7, record.block);
        Assert.AreEqual(1, record.hand.Count);
        Assert.AreEqual(1, record.enemyStates.Count);
        Assert.AreEqual(1, record.enemyIntents.Count);
        Assert.AreEqual(3, record.harpoonStack);
        Assert.AreEqual(1, record.debuffs.Count);
    }

    [Test]
    public void AppendAction_AccumulatesActionsInSameTurn()
    {
        PolishVisibleBattleSnapshot snapshot = CreateSnapshot();
        PolishTurnRecord record = PolishTurnRecordBuilder.Create(snapshot, 1);
        PolishHumanDecision decision = new PolishHumanDecision
        {
            decisionType = PolishDecisionType.UseCard,
            target = PolishDecisionTarget.Enemy,
            handIndex = 0,
            enemyIndex = 0,
            dangerLevel = PolishDangerLevel.High,
            expectedIncomingDamage = 12,
            reason = "처치 우선"
        };

        PolishTurnRecordBuilder.AppendAction(
            record,
            decision,
            PolishActionExecutionResult.Success,
            snapshot);
        PolishTurnRecordBuilder.AppendAction(
            record,
            decision,
            PolishActionExecutionResult.InvalidTarget,
            snapshot);

        Assert.AreEqual(2, record.actions.Count);
        Assert.AreEqual("High", record.dangerLevel);
        Assert.AreEqual(12, record.expectedIncomingDamage);
        Assert.AreEqual("Success", record.executionResults[0]);
        Assert.AreEqual("InvalidTarget", record.executionResults[1]);
        StringAssert.Contains("공격", record.actions[0]);
    }

    private static PolishVisibleBattleSnapshot CreateSnapshot()
    {
        return new PolishVisibleBattleSnapshot
        {
            isPlayerTurn = true,
            playerHp = 61,
            playerMaxHp = 75,
            playerBlock = 7,
            hand = new List<PolishVisibleCardSnapshot>
            {
                new PolishVisibleCardSnapshot
                {
                    cardId = "PHY_ATK_001",
                    displayName = "공격",
                    cardType = "Attack",
                    isUsable = true
                }
            },
            enemies = new List<PolishVisibleEnemySnapshot>
            {
                new PolishVisibleEnemySnapshot
                {
                    objectName = "Thief",
                    hp = 20,
                    maxHp = 40,
                    harpoonStack = 3,
                    intents = new List<string> { "Damage:8" },
                    statuses = new List<PolishVisibleStatusSnapshot>
                    {
                        new PolishVisibleStatusSnapshot
                        {
                            type = "Weakness",
                            value = 40,
                            remainingTurn = 2,
                            isDebuff = true
                        }
                    }
                }
            }
        };
    }
}
