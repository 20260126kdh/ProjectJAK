using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 카드에 들어있는 효과 목록을 순서대로 실행하는 클래스입니다.
/// 현재는 피해, 방어도, 체력 감소, 회복, 상태 효과 부여를 처리합니다.
/// </summary>
public class CardEffectExecutor : MonoBehaviour
{
    [Header("플레이어 태그")]
    [SerializeField]
    private string playerTag = "Player";

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

            case CardEffectType.GainBlock:
                ExecuteGainBlock(effect);
                break;

            case CardEffectType.LoseHealth:
                ExecuteLoseHealth(effect);
                break;

            case CardEffectType.Heal:
                ExecuteHeal(effect);
                break;

            case CardEffectType.ApplyStatus:
                ExecuteApplyStatus(effect, targetEnemy);
                break;

            case CardEffectType.HarpoonerStack:
            case CardEffectType.ApplyHarpoon:
                Debug.Log($"[CardEffectExecutor] 작살 스택 부여 예정 : {effect.value}");
                break;

            case CardEffectType.DrawCard:
                Debug.Log($"[CardEffectExecutor] 카드 드로우 예정 : {effect.value}");
                break;

            case CardEffectType.Summon:
            case CardEffectType.SummonCrew:
                Debug.Log($"[CardEffectExecutor] 소환 예정 : {effect.value}");
                break;

            case CardEffectType.GainBlockOnHealthLossThisTurn:
                ExecuteGainBlockOnHealthLossThisTurn(effect);
                break;

            case CardEffectType.DoubleNextAttackDamage:
                Debug.Log($"[CardEffectExecutor] 다음 공격 강화 예정 : {effect.value}");
                break;

            case CardEffectType.Sacrifice:
                Debug.Log($"[CardEffectExecutor] 소환수 희생 예정 : {effect.value}");
                break;

            case CardEffectType.SacrificeAll:
                Debug.Log($"[CardEffectExecutor] 모든 소환수 희생 예정 : {effect.value}");
                break;

            case CardEffectType.SetMaxHealth:
                Debug.Log($"[CardEffectExecutor] 최대 체력 설정 예정 : {effect.value}");
                break;

            case CardEffectType.MightEqualToSacrificedHealth:
                Debug.Log("[CardEffectExecutor] 희생 체력만큼 힘 획득 예정");
                break;

            case CardEffectType.DealDamageEqualToHarpoonerStack:
                Debug.Log("[CardEffectExecutor] 작살 스택만큼 피해 예정");
                break;
        }
    }

    /// <summary>
    /// 데미지 효과를 실행합니다.
    /// 현재는 단일 Enemy 대상만 실제 처리합니다.
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
    /// 방어도 획득 효과를 실행합니다.
    /// </summary>
    private void ExecuteGainBlock(CardEffectData effect)
    {
        PlayerCombat playerCombat = FindPlayerCombat();

        if (playerCombat == null)
        {
            Debug.LogWarning("[CardEffectExecutor] PlayerCombat을 찾지 못했습니다.");
            return;
        }

        playerCombat.GainBlock(effect.value);
    }

    /// <summary>
    /// 체력 감소 효과를 실행합니다.
    /// </summary>
    private void ExecuteLoseHealth(CardEffectData effect)
    {
        PlayerCombat playerCombat = FindPlayerCombat();

        if (playerCombat == null)
        {
            Debug.LogWarning("[CardEffectExecutor] PlayerCombat을 찾지 못했습니다.");
            return;
        }

        playerCombat.LoseHealth(effect.value);
    }

    /// <summary>
    /// 체력 회복 효과를 실행합니다.
    /// </summary>
    private void ExecuteHeal(CardEffectData effect)
    {
        PlayerCombat playerCombat = FindPlayerCombat();

        if (playerCombat == null)
        {
            Debug.LogWarning("[CardEffectExecutor] PlayerCombat을 찾지 못했습니다.");
            return;
        }

        playerCombat.Heal(effect.value);
    }

    /// <summary>
    /// 이번 턴 체력 손실 여부에 따라 방어도를 획득합니다.
    /// </summary>
    private void ExecuteGainBlockOnHealthLossThisTurn(CardEffectData effect)
    {
        PlayerCombat playerCombat = FindPlayerCombat();

        if (playerCombat == null)
        {
            Debug.LogWarning("[CardEffectExecutor] PlayerCombat을 찾지 못했습니다.");
            return;
        }

        if (playerCombat.DamagedThisTurn)
        {
            playerCombat.GainBlock(effect.value);
            Debug.Log($"[CardEffectExecutor] 체력 손실 조건 방어도 획득 : {effect.value}");
        }
        else
        {
            Debug.Log("[CardEffectExecutor] 이번 턴 체력 손실이 없어 추가 방어도를 얻지 않습니다.");
        }
    }

    /// <summary>
    /// 상태 효과 부여를 실행합니다.
    /// 현재는 Self와 Enemy 대상만 처리합니다.
    /// </summary>
    private void ExecuteApplyStatus(CardEffectData effect, Enemy targetEnemy)
    {
        StatusEffectHandler statusEffectHandler = null;

        if (effect.target == CardTargetType.Self)
        {
            PlayerCombat playerCombat = FindPlayerCombat();

            if (playerCombat == null)
            {
                Debug.LogWarning("[CardEffectExecutor] PlayerCombat을 찾지 못했습니다.");
                return;
            }

            statusEffectHandler = playerCombat.GetComponent<StatusEffectHandler>();
        }
        else if (effect.target == CardTargetType.Enemy)
        {
            if (targetEnemy == null)
            {
                Debug.LogWarning("[CardEffectExecutor] 상태 효과를 부여할 Enemy가 없습니다.");
                return;
            }

            statusEffectHandler = targetEnemy.GetComponent<StatusEffectHandler>();
        }
        else
        {
            Debug.LogWarning($"[CardEffectExecutor] 현재 ApplyStatus는 Self/Enemy만 처리합니다. 현재 대상 : {effect.target}");
            return;
        }

        if (statusEffectHandler == null)
        {
            Debug.LogWarning("[CardEffectExecutor] 대상에 StatusEffectHandler가 없습니다.");
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

    /// <summary>
    /// Player 태그를 가진 오브젝트에서 PlayerCombat을 찾습니다.
    /// </summary>
    /// <summary>
    /// Player 태그를 가진 오브젝트들 중 PlayerCombat이 붙은 오브젝트를 찾습니다.
    /// </summary>
    private PlayerCombat FindPlayerCombat()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag(playerTag);

        foreach (GameObject playerObject in playerObjects)
        {
            PlayerCombat playerCombat = playerObject.GetComponent<PlayerCombat>();

            if (playerCombat != null)
            {
                return playerCombat;
            }
        }

        return null;
    }
}