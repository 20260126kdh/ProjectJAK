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
    public void Decide_KnownEnemySkillTarget_OverridesDescriptionGuess()
    {
        PolishVisibleBattleSnapshot snapshot = CreateBaseSnapshot();
        snapshot.enemies.Add(CreateEnemy(30, "Damage:5"));
        PolishVisibleCardSnapshot card = CreateCard(
            "SKILL",
            "표식",
            "취약 2를 부여합니다.",
            "Skill");
        card.hasRequiredTarget = true;
        card.requiredTarget = PolishDecisionTarget.Enemy;
        snapshot.hand.Add(card);

        PolishHumanDecision decision =
            new PolishHumanDecisionEngine(10).Decide(snapshot);

        Assert.AreEqual(PolishDecisionTarget.Enemy, decision.target);
        Assert.AreEqual(0, decision.enemyIndex);
    }

    [Test]
    public void Decide_KnownCrewTarget_SelectsLowestHpCrew()
    {
        PolishVisibleBattleSnapshot snapshot = CreateBaseSnapshot();
        snapshot.enemies.Add(CreateEnemy(30, "Damage:5"));
        snapshot.crews.Add(new PolishVisibleCrewSnapshot { order = 1, hp = 5 });
        snapshot.crews.Add(new PolishVisibleCrewSnapshot { order = 2, hp = 2 });
        PolishVisibleCardSnapshot card = CreateCard(
            "CREW",
            "희생",
            "선원 한 명을 희생합니다.",
            "Skill");
        card.hasRequiredTarget = true;
        card.requiredTarget = PolishDecisionTarget.Crew;
        snapshot.hand.Add(card);

        PolishHumanDecision decision =
            new PolishHumanDecisionEngine(11).Decide(snapshot);

        Assert.AreEqual(PolishDecisionTarget.Crew, decision.target);
        Assert.AreEqual(2, decision.crewOrder);
    }

    [Test]
    public void Decide_NoUsableCards_EndsTurn()
    {
        PolishVisibleBattleSnapshot snapshot = CreateBaseSnapshot();
        snapshot.hand.Add(CreateCard("ATK", "공격", "피해를 6 줍니다.", "Attack", false));

        PolishHumanDecision decision = new PolishHumanDecisionEngine(300).Decide(snapshot);

        Assert.AreEqual(PolishDecisionType.EndTurn, decision.decisionType);
        Assert.AreEqual(-1, decision.preserveHandIndex);
    }

    [Test]
    public void ChoosePreserveHandIndex_SelectsUpgradedSkillOnly()
    {
        PolishVisibleBattleSnapshot snapshot = CreateBaseSnapshot();
        snapshot.hand.Add(CreateCard("ATK", "공격", "피해", "Attack", false));
        PolishVisibleCardSnapshot skill =
            CreateCard("SKL", "기술", "효과", "Skill", false);
        skill.isUpgraded = true;
        skill.rarity = CardRarity.Common.ToString();
        snapshot.hand.Add(skill);

        int selected = PolishHumanDecisionEngine.
            ChoosePreserveHandIndex(snapshot, 0);

        Assert.AreEqual(1, selected);
    }

    [Test]
    public void ChoosePreserveHandIndex_DoesNotPreserveLowValueCommonCard()
    {
        PolishVisibleBattleSnapshot snapshot = CreateBaseSnapshot();
        PolishVisibleCardSnapshot attack =
            CreateCard("ATK", "기본 공격", "피해", "Attack", false);
        attack.rarity = CardRarity.Common.ToString();
        snapshot.hand.Add(attack);

        int selected = PolishHumanDecisionEngine.
            ChoosePreserveHandIndex(snapshot, 0);

        Assert.AreEqual(-1, selected);
    }

    [Test]
    public void ChoosePreserveHandIndex_PreservesEpicAttackCard()
    {
        PolishVisibleBattleSnapshot snapshot = CreateBaseSnapshot();
        PolishVisibleCardSnapshot attack =
            CreateCard("ATK", "희귀 공격", "피해", "Attack", false);
        attack.rarity = CardRarity.Epic.ToString();
        snapshot.hand.Add(attack);

        int selected = PolishHumanDecisionEngine.
            ChoosePreserveHandIndex(snapshot, 0);

        Assert.AreEqual(0, selected);
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

    [Test]
    public void Decide_Combo_PlaysVulnerableBeforeDamage()
    {
        PolishVisibleBattleSnapshot snapshot = CreateBaseSnapshot();
        snapshot.enemies.Add(CreateEnemy(40, "Damage:5"));
        PolishVisibleCardSnapshot attack =
            CreateCard("ATK", "강한 공격", "피해를 10 줍니다.", "Attack");
        attack.effects.Add(CreateEffect(CardEffectType.DealDamage, 10));
        PolishVisibleCardSnapshot vulnerable =
            CreateCard("SKL", "빈틈", "취약 2를 부여합니다.", "Skill");
        vulnerable.hasRequiredTarget = true;
        vulnerable.requiredTarget = PolishDecisionTarget.Enemy;
        vulnerable.effects.Add(CreateEffect(
            CardEffectType.ApplyStatus,
            2,
            StatusEffectType.Vulnerable));
        snapshot.hand.Add(attack);
        snapshot.hand.Add(vulnerable);

        PolishHumanDecision decision =
            new PolishHumanDecisionEngine(901).Decide(snapshot);

        Assert.AreEqual(1, decision.handIndex);
        StringAssert.Contains("빈틈 → 강한 공격", decision.reason);
    }

    [Test]
    public void Decide_Combo_PlaysMightBeforeMultiHitAttack()
    {
        PolishVisibleBattleSnapshot snapshot = CreateBaseSnapshot();
        snapshot.enemies.Add(CreateEnemy(80, "Damage:5"));
        PolishVisibleCardSnapshot multiHit =
            CreateCard("ATK", "연속 공격", "피해를 3씩 3회 줍니다.", "Attack");
        multiHit.effects.Add(CreateEffect(CardEffectType.DealDamage, 3, repeatCount: 3));
        PolishVisibleCardSnapshot might =
            CreateCard("SKL", "힘 집중", "힘 2를 얻습니다.", "Skill");
        might.hasRequiredTarget = true;
        might.requiredTarget = PolishDecisionTarget.Player;
        might.effects.Add(CreateEffect(
            CardEffectType.ApplyStatus,
            2,
            StatusEffectType.Might,
            CardTargetType.Self));
        snapshot.hand.Add(multiHit);
        snapshot.hand.Add(might);

        PolishHumanDecision decision =
            new PolishHumanDecisionEngine(902).Decide(snapshot);

        Assert.AreEqual(1, decision.handIndex);
        StringAssert.Contains("힘 집중 → 연속 공격", decision.reason);
    }

    [Test]
    public void Decide_HealthCostWouldKillPlayer_RejectsCard()
    {
        PolishVisibleBattleSnapshot snapshot = CreateBaseSnapshot();
        snapshot.playerHp = 3;
        snapshot.enemies.Add(CreateEnemy(30, "Damage:7"));
        PolishVisibleCardSnapshot healthCost =
            CreateCard("COST", "악마와의 거래", "체력을 5 잃고 힘을 얻습니다.", "Skill");
        healthCost.effects.Add(CreateEffect(
            CardEffectType.LoseHealth,
            5,
            target: CardTargetType.Self));
        PolishVisibleCardSnapshot defense =
            CreateCard("DEF", "회피", "방어도를 6 얻습니다.", "Defense");
        defense.effects.Add(CreateEffect(
            CardEffectType.GainBlock,
            6,
            target: CardTargetType.Self));
        snapshot.hand.Add(healthCost);
        snapshot.hand.Add(defense);

        PolishHumanDecision decision =
            new PolishHumanDecisionEngine(903).Decide(snapshot);

        Assert.AreEqual(1, decision.handIndex);
    }

    [Test]
    public void Decide_NoIncomingDamage_DoesNotUsePureImmediateBlock()
    {
        PolishVisibleBattleSnapshot snapshot = CreateBaseSnapshot();
        snapshot.enemies.Add(CreateEnemy(30, "Buff:1"));
        PolishVisibleCardSnapshot defense =
            CreateCard("DEF", "회피", "방어도를 6 얻습니다.", "Defense");
        defense.effects.Add(CreateEffect(
            CardEffectType.GainBlock,
            6,
            target: CardTargetType.Self));
        PolishVisibleCardSnapshot attack =
            CreateCard("ATK", "투창", "피해를 5 줍니다.", "Attack");
        attack.effects.Add(CreateEffect(CardEffectType.DealDamage, 5));
        snapshot.hand.Add(defense);
        snapshot.hand.Add(attack);

        PolishHumanDecision decision =
            new PolishHumanDecisionEngine(904).Decide(snapshot);

        Assert.AreEqual(1, decision.handIndex);
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
            rarity = CardRarity.Common.ToString(),
            isUsable = isUsable
        };
    }

    private static PolishVisibleCardEffectSnapshot CreateEffect(
        CardEffectType effectType,
        int value,
        StatusEffectType statusEffectType = StatusEffectType.None,
        CardTargetType target = CardTargetType.Enemy,
        int repeatCount = 1)
    {
        return new PolishVisibleCardEffectSnapshot
        {
            effectType = effectType,
            statusEffectType = statusEffectType,
            value = value,
            target = target,
            repeatCount = repeatCount
        };
    }
}
