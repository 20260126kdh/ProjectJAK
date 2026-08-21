using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// 공통 인간형 판단 엔진의 생존, 처치와 재현 규칙을 검증합니다.
/// </summary>
public class PolishHumanDecisionEngineTests
{
    [Test]
    public void EstimateIncomingDamage_ParsesSingleAndMultiHitIntents()
    {
        PolishVisibleBattleSnapshot snapshot = CreateBaseSnapshot();
        snapshot.enemies.Add(new PolishVisibleEnemySnapshot
        {
            hp = 30,
            intents = new List<string> { "Damage:5 + 9x3", "Buff:1" }
        });

        Assert.AreEqual(32, PolishHumanDecisionEngine.EstimateIncomingDamage(snapshot));
    }

    [Test]
    public void Decide_HighDanger_PrefersDefenseOverWeakAttack()
    {
        PolishVisibleBattleSnapshot snapshot = CreateBaseSnapshot();
        snapshot.playerHp = 20;
        snapshot.enemies.Add(CreateEnemy(40, "Damage:18"));
        snapshot.hand.Add(CreateCard("ATK", "공격", "피해를 3 줍니다.", "Attack"));
        snapshot.hand.Add(CreateCard("DEF", "방어", "방어도를 12 얻습니다.", "Defense"));

        PolishHumanDecision decision = new PolishHumanDecisionEngine(100).Decide(snapshot);

        Assert.AreEqual(PolishDecisionType.UseCard, decision.decisionType);
        Assert.AreEqual(1, decision.handIndex);
        Assert.AreEqual(PolishDecisionTarget.Player, decision.target);
    }

    [Test]
    public void Decide_LethalEnemy_PrefersRemovingIncomingDamage()
    {
        PolishVisibleBattleSnapshot snapshot = CreateBaseSnapshot();
        snapshot.playerHp = 10;
        snapshot.enemies.Add(CreateEnemy(6, "Damage:12"));
        snapshot.hand.Add(CreateCard("ATK", "마무리", "피해를 7 줍니다.", "Attack"));
        snapshot.hand.Add(CreateCard("DEF", "약한 방어", "방어도를 2 얻습니다.", "Defense"));

        PolishHumanDecision decision = new PolishHumanDecisionEngine(200).Decide(snapshot);

        Assert.AreEqual(0, decision.handIndex);
        Assert.AreEqual(PolishDecisionTarget.Enemy, decision.target);
    }

    [Test]
    public void Decide_NoUsableCards_EndsTurn()
    {
        PolishVisibleBattleSnapshot snapshot = CreateBaseSnapshot();
        snapshot.hand.Add(CreateCard("ATK", "공격", "피해를 6 줍니다.", "Attack", false));

        PolishHumanDecision decision = new PolishHumanDecisionEngine(300).Decide(snapshot);

        Assert.AreEqual(PolishDecisionType.EndTurn, decision.decisionType);
    }

    [Test]
    public void Decide_SameSeedAndSnapshot_ReproducesDecision()
    {
        PolishVisibleBattleSnapshot snapshot = CreateBaseSnapshot();
        snapshot.enemies.Add(CreateEnemy(30, "Damage:4"));
        snapshot.hand.Add(CreateCard("A", "공격 A", "피해를 6 줍니다.", "Attack"));
        snapshot.hand.Add(CreateCard("B", "공격 B", "피해를 6 줍니다.", "Attack"));

        PolishHumanDecision first = new PolishHumanDecisionEngine(777).Decide(snapshot);
        PolishHumanDecision second = new PolishHumanDecisionEngine(777).Decide(snapshot);

        Assert.AreEqual(first.handIndex, second.handIndex);
        Assert.AreEqual(first.reason, second.reason);
    }

    private static PolishVisibleBattleSnapshot CreateBaseSnapshot()
    {
        return new PolishVisibleBattleSnapshot
        {
            isPlayerTurn = true,
            playerHp = 75,
            playerMaxHp = 75,
            playerBlock = 0,
            attackDefenseUseLimit = 2
        };
    }

    private static PolishVisibleEnemySnapshot CreateEnemy(int hp, string intent)
    {
        return new PolishVisibleEnemySnapshot
        {
            hp = hp,
            maxHp = hp,
            intents = new List<string> { intent }
        };
    }

    private static PolishVisibleCardSnapshot CreateCard(
        string id,
        string name,
        string description,
        string type,
        bool isUsable = true)
    {
        return new PolishVisibleCardSnapshot
        {
            cardId = id,
            displayName = name,
            description = description,
            cardType = type,
            isUsable = isUsable
        };
    }
}
