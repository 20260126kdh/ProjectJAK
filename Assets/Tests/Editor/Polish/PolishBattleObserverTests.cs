using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// AutoPlayer 공개 정보 스냅샷에 미래 정보가 노출되지 않는지 검증합니다.
/// </summary>
public class PolishBattleObserverTests
{
    [Test]
    public void VisibleSnapshot_DoesNotExposeFutureOrRandomInformation()
    {
        string[] forbiddenTerms =
        {
            "drawpile",
            "discardpile",
            "reward",
            "seed",
            "random",
            "nextpattern"
        };

        Type[] snapshotTypes =
        {
            typeof(PolishVisibleBattleSnapshot),
            typeof(PolishVisibleCardSnapshot),
            typeof(PolishVisibleEnemySnapshot),
            typeof(PolishVisibleCrewSnapshot),
            typeof(PolishVisibleStatusSnapshot)
        };

        foreach (Type type in snapshotTypes)
        {
            string[] memberNames = type
                .GetMembers(BindingFlags.Instance | BindingFlags.Public)
                .Select(member => member.Name.ToLowerInvariant())
                .ToArray();

            foreach (string forbiddenTerm in forbiddenTerms)
            {
                Assert.IsFalse(
                    memberNames.Any(name => name.Contains(forbiddenTerm)),
                    $"금지 정보 노출: {type.Name}.{forbiddenTerm}");
            }
        }
    }

    [Test]
    public void IntentTexts_Property_IsReadOnlyContract()
    {
        PropertyInfo property = typeof(EnemyIntentUI).GetProperty("VisibleIntentTexts");
        Assert.IsNotNull(property);
        Assert.IsTrue(property.PropertyType.IsGenericType);
        Assert.AreEqual(
            typeof(System.Collections.Generic.IReadOnlyList<>),
            property.PropertyType.GetGenericTypeDefinition());
    }

    [Test]
    public void ResolveRequiredTarget_UsesExactCardEffectTargets()
    {
        CardData enemySkill = ScriptableObject.CreateInstance<CardData>();
        enemySkill.effects.Add(new CardEffectData
        {
            effectType = CardEffectType.ApplyStatus,
            target = CardTargetType.Enemy
        });
        CardData selfSkill = ScriptableObject.CreateInstance<CardData>();
        selfSkill.effects.Add(new CardEffectData
        {
            effectType = CardEffectType.GainBlock,
            target = CardTargetType.Self
        });
        CardData crewSkill = ScriptableObject.CreateInstance<CardData>();
        crewSkill.effects.Add(new CardEffectData
        {
            effectType = CardEffectType.Sacrifice,
            target = CardTargetType.Undead,
            value = 1
        });

        Assert.AreEqual(
            PolishDecisionTarget.Enemy,
            PolishBattleObserver.ResolveRequiredTarget(enemySkill));
        Assert.AreEqual(
            PolishDecisionTarget.Player,
            PolishBattleObserver.ResolveRequiredTarget(selfSkill));
        Assert.AreEqual(
            PolishDecisionTarget.Crew,
            PolishBattleObserver.ResolveRequiredTarget(crewSkill));

        UnityEngine.Object.DestroyImmediate(enemySkill);
        UnityEngine.Object.DestroyImmediate(selfSkill);
        UnityEngine.Object.DestroyImmediate(crewSkill);
    }
}
