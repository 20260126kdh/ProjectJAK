using UnityEngine;

/// <summary>
/// 카드 강화 규칙을 처리하는 유틸리티 클래스입니다.
/// 런타임 카드 한 장의 모든 강화 가능한 효과를 변경합니다.
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

        bool changed = false;

        foreach (CardEffectData effect in cardData.effects)
        {
            if (effect == null)
            {
                continue;
            }

            switch (effect.effectType)
            {
                case CardEffectType.DealDamage:
                    UpgradeDamage(effect);
                    changed = true;
                    break;

                case CardEffectType.GainBlock:
                    effect.value += 4;
                    changed = true;
                    break;

                case CardEffectType.ApplyStatus:
                    if (IsBuff(effect.statusEffectType))
                    {
                        effect.value += 2;
                        changed = true;
                    }
                    else if (IsDebuff(effect.statusEffectType))
                    {
                        effect.value += 1;
                        changed = true;
                    }

                    break;

                case CardEffectType.ApplyHarpoon:
                case CardEffectType.HarpoonerStack:
                    effect.value += 2;
                    changed = true;
                    break;
            }
        }

        if (!changed)
        {
            return false;
        }

        cardData.MarkAsUpgraded();

        return true;
    }

    /// <summary>
    /// 공격 효과를 단일 공격과 다단히트로 구분하여 강화합니다.
    /// </summary>
    private static void UpgradeDamage(
        CardEffectData effect)
    {
        if (effect.repeatCount > 1)
        {
            effect.value =
                Mathf.FloorToInt(
                    effect.value * 1.3f
                );

            effect.repeatCount += 1;
        }
        else
        {
            effect.value =
                Mathf.FloorToInt(
                    effect.value * 1.5f
                );
        }
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
    /// 작살은 별도 효과로 처리합니다.
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
                return true;

            default:
                return false;
        }
    }
}