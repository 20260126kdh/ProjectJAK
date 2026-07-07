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
    public void AddStatusEffect(StatusEffectType statusEffectType, int value, int remainingTurn, bool isPermanent)
    {
        StatusEffectData existingEffect = statusEffects.Find(effect => effect.statusEffectType == statusEffectType);

        if (existingEffect != null)
        {
            existingEffect.value += value;

            if (!existingEffect.isPermanent)
            {
                existingEffect.remainingTurn = Mathf.Max(existingEffect.remainingTurn, remainingTurn);
            }

            Debug.Log($"[StatusEffectHandler] 상태 효과 중첩 : {statusEffectType} / 현재 수치 : {existingEffect.value}");
            return;
        }

        StatusEffectData newEffect = new StatusEffectData(statusEffectType, value, remainingTurn, isPermanent);
        statusEffects.Add(newEffect);

        Debug.Log($"[StatusEffectHandler] 상태 효과 추가 : {statusEffectType} / 수치 : {value}");
    }

    /// <summary>
    /// 특정 상태 효과 수치를 반환합니다.
    /// 없으면 0을 반환합니다.
    /// </summary>
    public int GetStatusValue(StatusEffectType statusEffectType)
    {
        StatusEffectData effect = statusEffects.Find(status => status.statusEffectType == statusEffectType);

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
        return statusEffects.Exists(effect => effect.statusEffectType == statusEffectType);
    }

    /// <summary>
    /// 턴 종료 시 지속 턴을 감소시킵니다.
    /// 영구 효과는 감소하지 않습니다.
    /// </summary>
    public void DecreaseTurnDuration()
    {
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            if (statusEffects[i].isPermanent)
            {
                continue;
            }

            statusEffects[i].remainingTurn--;

            if (statusEffects[i].remainingTurn <= 0)
            {
                Debug.Log($"[StatusEffectHandler] 상태 효과 제거 : {statusEffects[i].statusEffectType}");
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