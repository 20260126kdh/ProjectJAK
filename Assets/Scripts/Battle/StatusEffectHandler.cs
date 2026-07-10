using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 캐릭터, 적, 소환수에게 적용된 상태 효과 목록을 관리하는 클래스입니다.
/// </summary>
public class StatusEffectHandler : MonoBehaviour
{
    [Header("현재 적용된 상태 효과 목록")]
    [SerializeField]
    private List<StatusEffectData> statusEffects = new List<StatusEffectData>();

    /// <summary>
    /// 현재 적용된 상태 효과 목록을 반환합니다.
    /// </summary>
    public List<StatusEffectData> StatusEffects => statusEffects;

    /// <summary>
    /// 상태 효과를 추가합니다.
    /// 이미 같은 상태 효과가 있다면 value를 합산합니다.
    /// </summary>
    public void AddStatusEffect(
        StatusEffectType statusEffectType,
        int value,
        int remainingTurn,
        bool isPermanent)
    {
        StatusEffectData existingEffect = statusEffects.Find(
            effect => effect.statusEffectType == statusEffectType
        );

        if (existingEffect != null)
        {
            existingEffect.value += value;

            if (!existingEffect.isPermanent &&
                statusEffectType != StatusEffectType.Toxic)
            {
                existingEffect.remainingTurn =
                    Mathf.Max(existingEffect.remainingTurn, remainingTurn);
            }

            Debug.Log(
                $"[StatusEffectHandler] 상태 효과 중첩 : " +
                $"{statusEffectType} / 현재 수치 : {existingEffect.value}"
            );

            return;
        }

        StatusEffectData newEffect = new StatusEffectData(
            statusEffectType,
            value,
            remainingTurn,
            isPermanent
        );

        statusEffects.Add(newEffect);

        Debug.Log(
            $"[StatusEffectHandler] 상태 효과 추가 : " +
            $"{statusEffectType} / 수치 : {value}"
        );
    }

    /// <summary>
    /// 특정 상태 효과 수치를 반환합니다.
    /// 없으면 0을 반환합니다.
    /// </summary>
    public int GetStatusValue(StatusEffectType statusEffectType)
    {
        StatusEffectData effect = statusEffects.Find(
            status => status.statusEffectType == statusEffectType
        );

        if (effect == null)
        {
            return 0;
        }

        return effect.value;
    }

    /// <summary>
    /// 특정 상태 효과를 가지고 있는지 확인합니다.
    /// </summary>
    public bool HasStatusEffect(StatusEffectType statusEffectType)
    {
        return statusEffects.Exists(
            effect => effect.statusEffectType == statusEffectType
        );
    }

    /// <summary>
    /// 특정 상태 효과를 즉시 제거합니다.
    /// </summary>
    public void RemoveStatusEffect(StatusEffectType statusEffectType)
    {
        StatusEffectData effect = statusEffects.Find(
            status => status.statusEffectType == statusEffectType
        );

        if (effect == null)
        {
            return;
        }

        statusEffects.Remove(effect);

        Debug.Log(
            $"[StatusEffectHandler] 상태 효과 직접 제거 : {statusEffectType}"
        );
    }

    /// <summary>
    /// 현재 중독 수치만큼의 피해량을 반환하고
    /// 중독 수치를 1 감소시킵니다.
    /// 중독 수치가 0이 되면 중독을 제거합니다.
    /// </summary>
    public int ProcessToxic()
    {
        StatusEffectData toxicEffect = statusEffects.Find(
            effect => effect.statusEffectType == StatusEffectType.Toxic
        );

        if (toxicEffect == null)
        {
            return 0;
        }

        if (toxicEffect.value <= 0)
        {
            statusEffects.Remove(toxicEffect);
            return 0;
        }

        int toxicDamage = toxicEffect.value;

        toxicEffect.value--;

        Debug.Log(
            $"[StatusEffectHandler] 중독 발동 : " +
            $"{toxicDamage} 피해 / 남은 중독 {toxicEffect.value}"
        );

        if (toxicEffect.value <= 0)
        {
            statusEffects.Remove(toxicEffect);

            Debug.Log("[StatusEffectHandler] 중독 제거");
        }

        return toxicDamage;
    }

    /// <summary>
    /// 턴 종료 시 지속 턴을 감소시킵니다.
    /// 영구 효과와 중독은 이 함수에서 감소하지 않습니다.
    /// </summary>
    public void DecreaseTurnDuration()
    {
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            StatusEffectData statusEffect = statusEffects[i];

            if (statusEffect.isPermanent)
            {
                continue;
            }

            // 중독은 remainingTurn이 아니라 value가 감소합니다.
            if (statusEffect.statusEffectType == StatusEffectType.Toxic)
            {
                continue;
            }

            statusEffect.remainingTurn--;

            if (statusEffect.remainingTurn <= 0)
            {
                Debug.Log(
                    $"[StatusEffectHandler] 상태 효과 제거 : " +
                    $"{statusEffect.statusEffectType}"
                );

                statusEffects.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 모든 상태 효과를 제거합니다.
    /// 전투 종료 또는 새 전투 시작 시 호출합니다.
    /// </summary>
    public void ClearAllStatusEffects()
    {
        statusEffects.Clear();

        Debug.Log("[StatusEffectHandler] 모든 상태 효과 초기화");
    }
}