using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// 카드 강화 규칙을 처리하는 유틸리티 클래스입니다.
/// 카드 효과 수치와 화면에 표시되는 설명을 함께 변경합니다.
/// </summary>
public static class CardUpgradeUtility
{
    /// <summary>
    /// 카드의 모든 강화 가능한 효과를 강화합니다.
    ///
    /// 공격:
    /// - 단일 공격: 피해량 1.5배, 소수점 버림
    /// - 다단히트: 피해량 1.3배, 소수점 버림, 타수 +1
    ///
    /// 기타:
    /// - 방어도 +4
    /// - 버프 +2
    /// - 디버프 +1
    /// - 작살 스택 +2
    /// </summary>
    public static bool UpgradeCard(CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogWarning(
                "[CardUpgradeUtility] 카드 데이터가 없습니다."
            );

            return false;
        }

        if (cardData.IsUpgraded)
        {
            Debug.LogWarning(
                "[CardUpgradeUtility] 이미 강화된 카드입니다."
            );

            return false;
        }

        if (cardData.effects == null ||
            cardData.effects.Count <= 0)
        {
            Debug.LogWarning(
                $"[CardUpgradeUtility] 카드 효과가 없습니다: " +
                $"{cardData.cardName}"
            );

            return false;
        }

        bool changed = false;

        /*
         * CSV에서 작성한 기존 문장을 유지하면서
         * 강화된 효과에 해당하는 숫자만 순서대로 변경합니다.
         */
        string upgradedDescription =
            cardData.description;

        int descriptionSearchIndex = 0;

        List<CardEffectData> orderedEffects =
            new List<CardEffectData>(
                cardData.effects
            );

        orderedEffects.Sort(
            (a, b) => a.order.CompareTo(b.order)
        );

        foreach (CardEffectData effect in orderedEffects)
        {
            if (effect == null)
            {
                continue;
            }

            switch (effect.effectType)
            {
                case CardEffectType.DealDamage:
                case CardEffectType.AllCrewsDealDamageAllEnemies:
                    UpgradeDamage(
                        effect,
                        ref upgradedDescription,
                        ref descriptionSearchIndex
                    );

                    changed = true;
                    break;

                case CardEffectType.GainBlock:
                    {
                        int oldValue = effect.value;

                        effect.value += 4;

                        ReplaceNextNumber(
                            ref upgradedDescription,
                            oldValue,
                            effect.value,
                            ref descriptionSearchIndex,
                            cardData.cardName
                        );

                        changed = true;
                        break;
                    }

                case CardEffectType.ApplyStatus:
                    {
                        int increaseAmount =
                            GetStatusUpgradeAmount(
                                effect.statusEffectType
                            );

                        if (increaseAmount <= 0)
                        {
                            break;
                        }

                        int oldValue = effect.value;

                        effect.value += increaseAmount;

                        ReplaceNextNumber(
                            ref upgradedDescription,
                            oldValue,
                            effect.value,
                            ref descriptionSearchIndex,
                            cardData.cardName
                        );

                        changed = true;
                        break;
                    }

                case CardEffectType.ApplyHarpoon:
                case CardEffectType.HarpoonerStack:
                    {
                        int oldValue = effect.value;

                        effect.value += 2;

                        ReplaceNextNumber(
                            ref upgradedDescription,
                            oldValue,
                            effect.value,
                            ref descriptionSearchIndex,
                            cardData.cardName
                        );

                        changed = true;
                        break;
                    }
            }
        }

        /*
         * 악마와의 거래는 강화 시 체력 손실과 힘을 모두 5에서 7로 변경합니다.
         * 일반 버프 강화는 설명의 첫 번째 5를 먼저 변경하므로,
         * 체력 손실 효과와 남은 힘 표기를 함께 보정합니다.
         */
        if (cardData.cardID == "PHY_SKL_002")
        {
            CardEffectData healthLossEffect =
                orderedEffects.Find(effect =>
                    effect != null &&
                    effect.effectType == CardEffectType.LoseHealth
                );

            if (healthLossEffect != null)
            {
                healthLossEffect.value = 7;
            }

            int physiqueSkillSearchIndex = 0;

            ReplaceNextNumber(
                ref upgradedDescription,
                5,
                7,
                ref physiqueSkillSearchIndex,
                cardData.cardName
            );
        }

        if (!changed)
        {
            return false;
        }

        cardData.SetRuntimeDescription(
            upgradedDescription
        );

        cardData.MarkAsUpgraded();

        Debug.Log(
            $"[CardUpgradeUtility] 카드 강화 완료: " +
            $"{cardData.GetDisplayName()} / " +
            $"{cardData.DisplayDescription}"
        );

        return true;
    }

    /// <summary>
    /// 공격 효과를 단일 공격과 다단히트로 구분해 강화하고,
    /// 기존 설명의 피해량과 타수를 함께 변경합니다.
    /// </summary>
    private static void UpgradeDamage(
        CardEffectData effect,
        ref string description,
        ref int searchIndex)
    {
        int oldDamage = effect.value;
        int oldRepeatCount =
            Mathf.Max(1, effect.repeatCount);

        if (oldRepeatCount > 1)
        {
            effect.value =
                Mathf.FloorToInt(
                    oldDamage * 1.3f
                );

            effect.repeatCount =
                oldRepeatCount + 1;

            ReplaceNextNumber(
                ref description,
                oldDamage,
                effect.value,
                ref searchIndex,
                "다단히트 피해량"
            );

            ReplaceNextNumber(
                ref description,
                oldRepeatCount,
                effect.repeatCount,
                ref searchIndex,
                "다단히트 타수"
            );
        }
        else
        {
            effect.value =
                Mathf.FloorToInt(
                    oldDamage * 1.5f
                );

            ReplaceNextNumber(
                ref description,
                oldDamage,
                effect.value,
                ref searchIndex,
                "단일 공격 피해량"
            );
        }
    }

    /// <summary>
    /// 상태 효과에 적용할 강화 증가량을 반환합니다.
    /// 버프는 +2, 디버프는 +1입니다.
    /// </summary>
    private static int GetStatusUpgradeAmount(
        StatusEffectType statusEffectType)
    {
        if (IsBuff(statusEffectType))
        {
            return 2;
        }

        if (IsDebuff(statusEffectType))
        {
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// 설명에서 검색 위치 이후에 등장하는
    /// 첫 번째 동일 숫자를 새로운 숫자로 변경합니다.
    /// </summary>
    private static void ReplaceNextNumber(
        ref string source,
        int oldValue,
        int newValue,
        ref int searchIndex,
        string contextName)
    {
        if (string.IsNullOrEmpty(source))
        {
            return;
        }

        string pattern =
            $@"(?<!\d){oldValue}(?!\d)";

        Match match =
            Regex.Match(
                source.Substring(searchIndex),
                pattern
            );

        if (!match.Success)
        {
            /*
             * 효과 순서와 설명 순서가 다른 카드도 있을 수 있으므로
             * 처음부터 한 번 더 검색합니다.
             */
            match = Regex.Match(
                source,
                pattern
            );

            if (!match.Success)
            {
                Debug.LogWarning(
                    $"[CardUpgradeUtility] 설명에서 숫자를 찾지 못했습니다. " +
                    $"대상: {contextName} / " +
                    $"{oldValue} → {newValue} / " +
                    $"설명: {source}"
                );

                return;
            }

            int absoluteIndex =
                match.Index;

            source =
                source.Remove(
                    absoluteIndex,
                    match.Length
                );

            source =
                source.Insert(
                    absoluteIndex,
                    newValue.ToString()
                );

            searchIndex =
                absoluteIndex +
                newValue.ToString().Length;

            return;
        }

        int replacementIndex =
            searchIndex +
            match.Index;

        source =
            source.Remove(
                replacementIndex,
                match.Length
            );

        source =
            source.Insert(
                replacementIndex,
                newValue.ToString()
            );

        searchIndex =
            replacementIndex +
            newValue.ToString().Length;
    }

    /// <summary>
    /// 강화 시 수치가 2 증가하는 버프인지 확인합니다.
    /// </summary>
    private static bool IsBuff(
        StatusEffectType statusEffectType)
    {
        switch (statusEffectType)
        {
            case StatusEffectType.Might:
            case StatusEffectType.Guard:
            case StatusEffectType.Resist:
            case StatusEffectType.Lifesteal:
            case StatusEffectType.Echo:
            case StatusEffectType.Immortal:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// 강화 시 수치가 1 증가하는 디버프인지 확인합니다.
    /// 작살 스택은 별도로 처리합니다.
    /// </summary>
    private static bool IsDebuff(
        StatusEffectType statusEffectType)
    {
        switch (statusEffectType)
        {
            case StatusEffectType.Weaken:
            case StatusEffectType.Vulnerable:
            case StatusEffectType.Cripple:
            case StatusEffectType.NoBlock:
            case StatusEffectType.Broken:
            case StatusEffectType.Jinx:
            case StatusEffectType.Paralyze:
            case StatusEffectType.Toxic:
            case StatusEffectType.MightReduction:
                return true;

            default:
                return false;
        }
    }
}
