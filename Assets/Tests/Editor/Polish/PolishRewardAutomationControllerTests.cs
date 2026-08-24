using NUnit.Framework;
using UnityEngine;

/// <summary>
/// 자동 보상 선택의 공개 카드 우선순위를 검증합니다.
/// </summary>
public class PolishRewardAutomationControllerTests
{
    [Test]
    public void ChooseBestRewardIndex_SelectsHighestRarity()
    {
        CardData common = CreateCard(CardRarity.Common);
        CardData epic = CreateCard(CardRarity.Epic);
        CardData rare = CreateCard(CardRarity.Rare);

        int selected = PolishRewardAutomationController.
            ChooseBestRewardIndex(new[] { common, epic, rare });

        Assert.AreEqual(1, selected);
        Object.DestroyImmediate(common);
        Object.DestroyImmediate(epic);
        Object.DestroyImmediate(rare);
    }

    [Test]
    public void ChooseBestRewardIndex_ReturnsNegativeOneForInvalidCards()
    {
        int selected = PolishRewardAutomationController.
            ChooseBestRewardIndex(new CardData[] { null, null });

        Assert.AreEqual(-1, selected);
    }

    [TestCase(StagePhase.NormalBattle, true)]
    [TestCase(StagePhase.BossBattle, true)]
    [TestCase(StagePhase.Rest, false)]
    public void ShouldWaitForNextBattle_HandlesFollowUpBoss(
        StagePhase phase,
        bool expected)
    {
        Assert.AreEqual(
            expected,
            PolishRewardAutomationController.ShouldWaitForNextBattle(phase));
    }

    private static CardData CreateCard(CardRarity rarity)
    {
        CardData card = ScriptableObject.CreateInstance<CardData>();
        card.cardRarity = rarity;
        return card;
    }
}
