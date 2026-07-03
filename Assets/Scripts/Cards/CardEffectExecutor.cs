using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 카드에 들어있는 효과 목록을 순서대로 실행하는 클래스입니다.
/// 현재는 피해와 상태 효과 부여를 처리합니다.
/// </summary>
public class CardEffectExecutor : MonoBehaviour
{
    /// <summary>
    /// 카드 효과 목록을 실행합니다.
    /// </summary>
    public void ExecuteEffects(CardData cardData, Enemy targetEnemy)
    {
        if (cardData == null)
        {
            Debug.LogWarning("[CardEffectExecutor] 실행할 카드 데이터가 없습니다.");
            return;
        }

        if (cardData.effects == null || cardData.effects.Count == 0)
        {
            Debug.LogWarning($"[CardEffectExecutor] {cardData.cardName} 카드에 효과가 없습니다.");
            return;
        }

        List<CardEffectData> orderedEffects = cardData.effects
            .OrderBy(effect => effect.order)
            .ToList();

        foreach (CardEffectData effect in orderedEffects)
        {
            ExecuteSingleEffect(effect, targetEnemy);
        }
    }

    /// <summary>
    /// 카드 효과 하나를 실행합니다.
    /// </summary>
    private void ExecuteSingleEffect(CardEffectData effect, Enemy targetEnemy)
    {
        switch (effect.effectType)
        {
            case CardEffectType.DealDamage:
                ExecuteDealDamage(effect, targetEnemy);
                break;

            case CardEffectType.ApplyStatus:
                ExecuteApplyStatus(effect, targetEnemy);
                break;

            case CardEffectType.GainBlock:
                Debug.Log($"[CardEffectExecutor] 방어도 획득 예정 : {effect.value}");
                break;

            case CardEffectType.ApplyHarpoon:
                Debug.Log($"[CardEffectExecutor] 작살 스택 부여 예정 : {effect.value}");
                break;

            case CardEffectType.DrawCard:
                Debug.Log($"[CardEffectExecutor] 카드 드로우 예정 : {effect.value}");
                break;

            case CardEffectType.Summon:
                Debug.Log($"[CardEffectExecutor] 소환 예정 : {effect.value}");
                break;
        }
    }

    /// <summary>
    /// 데미지 효과를 실행합니다.
    /// </summary>
    private void ExecuteDealDamage(CardEffectData effect, Enemy targetEnemy)
    {
        if (targetEnemy == null)
        {
            Debug.LogWarning("[CardEffectExecutor] 공격 대상 Enemy가 연결되지 않았습니다.");
            return;
        }

        targetEnemy.TakeDamage(effect.value);
    }

    /// <summary>
    /// 상태 효과 부여를 실행합니다.
    /// 현재는 Enemy 대상만 처리합니다.
    /// </summary>
    private void ExecuteApplyStatus(CardEffectData effect, Enemy targetEnemy)
    {
        if (effect.target != CardTargetType.Enemy)
        {
            Debug.LogWarning($"[CardEffectExecutor] 현재 ApplyStatus는 Enemy 대상만 처리합니다. 현재 대상 : {effect.target}");
            return;
        }

        if (targetEnemy == null)
        {
            Debug.LogWarning("[CardEffectExecutor] 상태 효과를 부여할 Enemy가 없습니다.");
            return;
        }

        StatusEffectHandler statusEffectHandler = targetEnemy.GetComponent<StatusEffectHandler>();

        if (statusEffectHandler == null)
        {
            Debug.LogWarning("[CardEffectExecutor] 대상 Enemy에 StatusEffectHandler가 없습니다.");
            return;
        }

        statusEffectHandler.AddStatusEffect(
            effect.statusEffectType,
            effect.value,
            1,
            false
        );

        Debug.Log($"[CardEffectExecutor] 상태 효과 부여 : {effect.statusEffectType} / 수치 : {effect.value}");
    }
}