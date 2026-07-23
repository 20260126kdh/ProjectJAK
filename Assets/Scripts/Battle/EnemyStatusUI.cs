using UnityEngine;

/// <summary>
/// 적의 상태 효과 UI 영역을 관리합니다.
///
/// 상태 효과가 하나 이상 있으면 StatusRoot를 표시하고,
/// 상태 효과가 없으면 StatusRoot를 숨깁니다.
///
/// 부모 오브젝트에서 StatusEffectHandler를 자동으로 찾습니다.
/// </summary>
public class EnemyStatusUI : MonoBehaviour
{
    [Header("상태 효과 UI 루트")]
    [SerializeField]
    private GameObject statusRoot;

    private StatusEffectHandler statusEffectHandler;

    private void Awake()
    {
        statusEffectHandler =
            GetComponentInParent<StatusEffectHandler>();

        if (statusEffectHandler == null)
        {
            Debug.LogWarning(
                "[EnemyStatusUI] 부모 오브젝트에서 " +
                "StatusEffectHandler를 찾지 못했습니다.",
                this
            );
        }
    }

    private void OnEnable()
    {
        Subscribe();
        RefreshStatusRoot();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    /// <summary>
    /// 상태 효과 변경 이벤트를 연결합니다.
    /// </summary>
    private void Subscribe()
    {
        if (statusEffectHandler == null)
        {
            return;
        }

        statusEffectHandler.StatusEffectsChanged -=
            RefreshStatusRoot;

        statusEffectHandler.StatusEffectsChanged +=
            RefreshStatusRoot;
    }

    /// <summary>
    /// 상태 효과 변경 이벤트 연결을 해제합니다.
    /// </summary>
    private void Unsubscribe()
    {
        if (statusEffectHandler == null)
        {
            return;
        }

        statusEffectHandler.StatusEffectsChanged -=
            RefreshStatusRoot;
    }

    /// <summary>
    /// 현재 상태 효과 개수에 따라
    /// StatusRoot의 표시 여부를 변경합니다.
    /// </summary>
    private void RefreshStatusRoot()
    {
        if (statusRoot == null)
        {
            return;
        }

        bool hasStatusEffect =
            statusEffectHandler != null &&
            statusEffectHandler.StatusEffects != null &&
            statusEffectHandler.StatusEffects.Count > 0;

        if (statusRoot.activeSelf == hasStatusEffect)
        {
            return;
        }

        statusRoot.SetActive(hasStatusEffect);
    }
}