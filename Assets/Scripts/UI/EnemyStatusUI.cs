using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적의 상태 효과 UI 전체를 관리합니다.
///
/// 상태 효과가 추가되면 아이콘을 생성하고,
/// 상태 효과 수치나 남은 턴이 바뀌면 갱신하며,
/// 상태 효과가 제거되면 아이콘도 제거합니다.
///
/// 상태 효과가 하나도 없으면
/// StatusRoot를 숨깁니다.
/// </summary>
public class EnemyStatusUI : MonoBehaviour
{
    [Header("상태 효과 UI 루트")]
    [SerializeField]
    private GameObject statusRoot;

    [Header("상태 효과 아이콘 생성 위치")]
    [SerializeField]
    private Transform statusIconContainer;

    [Header("상태 효과 아이콘 프리팹")]
    [SerializeField]
    private EnemyStatusIconUI statusIconPrefab;

    [Header("상태 효과 아이콘 목록")]
    [SerializeField]
    private List<StatusEffectIconData> statusIconDataList =
        new List<StatusEffectIconData>();

    private StatusEffectHandler statusEffectHandler;

    private readonly Dictionary<
        StatusEffectType,
        EnemyStatusIconUI> createdIcons =
        new Dictionary<
            StatusEffectType,
            EnemyStatusIconUI>();

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
        RefreshStatusUI();
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
            RefreshStatusUI;

        statusEffectHandler.StatusEffectsChanged +=
            RefreshStatusUI;
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
            RefreshStatusUI;
    }

    /// <summary>
    /// 현재 상태 효과 목록을 기준으로
    /// 모든 상태 효과 UI를 갱신합니다.
    /// </summary>
    private void RefreshStatusUI()
    {
        if (statusEffectHandler == null)
        {
            SetStatusRootActive(false);
            return;
        }

        List<StatusEffectData> currentEffects =
            statusEffectHandler.StatusEffects;

        RemoveUnusedIcons(
            currentEffects
        );

        CreateOrRefreshIcons(
            currentEffects
        );

        bool hasStatusEffect =
            currentEffects != null &&
            currentEffects.Count > 0;

        SetStatusRootActive(
            hasStatusEffect
        );
    }

    /// <summary>
    /// 더 이상 적용되어 있지 않은
    /// 상태 효과 아이콘을 제거합니다.
    /// </summary>
    private void RemoveUnusedIcons(
        List<StatusEffectData> currentEffects)
    {
        List<StatusEffectType> removeTargets =
            new List<StatusEffectType>();

        foreach (
            KeyValuePair<
                StatusEffectType,
                EnemyStatusIconUI> pair
            in createdIcons)
        {
            bool stillExists =
                currentEffects != null &&
                currentEffects.Exists(
                    effect =>
                        effect != null &&
                        effect.statusEffectType ==
                        pair.Key
                );

            if (!stillExists)
            {
                removeTargets.Add(
                    pair.Key
                );
            }
        }

        foreach (
            StatusEffectType removeTarget
            in removeTargets)
        {
            EnemyStatusIconUI icon =
                createdIcons[removeTarget];

            createdIcons.Remove(
                removeTarget
            );

            if (icon != null)
            {
                Destroy(
                    icon.gameObject
                );
            }
        }
    }

    /// <summary>
    /// 현재 적용 중인 상태 효과 아이콘을
    /// 생성하거나 갱신합니다.
    /// </summary>
    private void CreateOrRefreshIcons(
        List<StatusEffectData> currentEffects)
    {
        if (currentEffects == null)
        {
            return;
        }

        foreach (
            StatusEffectData statusEffect
            in currentEffects)
        {
            if (statusEffect == null)
            {
                continue;
            }

            if (createdIcons.TryGetValue(
                statusEffect.statusEffectType,
                out EnemyStatusIconUI existingIcon))
            {
                if (existingIcon != null)
                {
                    existingIcon.Refresh(
                        statusEffect
                    );
                }

                continue;
            }

            CreateStatusIcon(
                statusEffect
            );
        }
    }

    /// <summary>
    /// 새로운 상태 효과 아이콘을 생성합니다.
    /// </summary>
    private void CreateStatusIcon(
        StatusEffectData statusEffect)
    {
        if (statusIconPrefab == null)
        {
            Debug.LogWarning(
                "[EnemyStatusUI] 상태 효과 아이콘 프리팹이 없습니다.",
                this
            );

            return;
        }

        if (statusIconContainer == null)
        {
            Debug.LogWarning(
                "[EnemyStatusUI] 상태 효과 아이콘 생성 위치가 없습니다.",
                this
            );

            return;
        }

        EnemyStatusIconUI newIcon =
            Instantiate(
                statusIconPrefab,
                statusIconContainer
            );

        Sprite iconSprite =
            FindIconSprite(
                statusEffect.statusEffectType
            );

        newIcon.Initialize(
            statusEffect,
            iconSprite
        );

        createdIcons.Add(
            statusEffect.statusEffectType,
            newIcon
        );
    }

    /// <summary>
    /// 상태 효과 종류에 해당하는
    /// 아이콘 Sprite를 찾습니다.
    /// </summary>
    private Sprite FindIconSprite(
        StatusEffectType statusEffectType)
    {
        StatusEffectIconData iconData =
            statusIconDataList.Find(
                data =>
                    data != null &&
                    data.statusEffectType ==
                    statusEffectType
            );

        if (iconData == null)
        {
            Debug.LogWarning(
                $"[EnemyStatusUI] {statusEffectType}에 연결된 " +
                "상태 효과 아이콘이 없습니다.",
                this
            );

            return null;
        }

        return iconData.iconSprite;
    }

    /// <summary>
    /// 상태 효과 UI 루트의 표시 여부를 변경합니다.
    /// </summary>
    private void SetStatusRootActive(
        bool isActive)
    {
        if (statusRoot == null)
        {
            return;
        }

        if (statusRoot.activeSelf == isActive)
        {
            return;
        }

        statusRoot.SetActive(
            isActive
        );
    }
}