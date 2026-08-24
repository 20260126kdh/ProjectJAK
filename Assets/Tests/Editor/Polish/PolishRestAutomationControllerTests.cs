using NUnit.Framework;
using UnityEngine;

/// <summary>
/// 자동 휴식 단계의 강화 카드 선택 기준을 검증합니다.
/// </summary>
public class PolishRestAutomationControllerTests
{
    [Test]
    public void ChooseUpgradeIndex_SelectsUpgradeableHighestRarityCard()
    {
        CardData common = CreateCard(CardRarity.Common, CardEffectType.GainBlock);
        CardData epic = CreateCard(CardRarity.Epic, CardEffectType.DealDamage);
        CardData invalid = CreateCard(CardRarity.Epic, CardEffectType.DrawCard);

        int selected = PolishRestAutomationController.
            ChooseUpgradeIndex(new[] { common, invalid, epic });

        Assert.AreEqual(2, selected);
        Object.DestroyImmediate(common);
        Object.DestroyImmediate(epic);
        Object.DestroyImmediate(invalid);
    }

    private static CardData CreateCard(
        CardRarity rarity,
        CardEffectType effectType)
    {
        CardData card = ScriptableObject.CreateInstance<CardData>();
        card.cardRarity = rarity;
        card.effects.Add(new CardEffectData { effectType = effectType });
        return card;
    }
}
