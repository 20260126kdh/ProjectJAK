using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어에게 적용된 상태 효과 UI를 관리합니다.
///
/// 상태 효과가 추가되면 아이콘을 생성하고,
/// 수치나 남은 턴이 변경되면 아이콘을 갱신하며,
/// 상태 효과가 제거되면 해당 아이콘도 제거합니다.
/// </summary>
public class PlayerStatusUI : MonoBehaviour
{
    [Header("Player Combat")]
    [SerializeField]
    private PlayerCombat playerCombat;

    [Header("상태 효과 UI 루트")]
    [SerializeField]
    private GameObject statusRoot;

    [Header("상태 효과 아이콘 생성 위치")]
    [SerializeField]
    private Transform statusIconContainer;

    [Header("상태 효과 아이콘 프리팹")]
    [SerializeField]
    private EnemyStatusIconUI statusIconPrefab;

    [Header("상태 효과 아이콘 데이터베이스")]
    [SerializeField]
    private StatusEffectIconDatabase statusEffectIconDatabase;

    private StatusEffectHandler statusEffectHandler;

    private bool isSubscribed;

    private readonly Dictionary<
        StatusEffectType,
        EnemyStatusIconUI> createdIcons =
        new Dictionary<
            StatusEffectType,
            EnemyStatusIconUI>();

    private void Awake()
    {
        FindStatusEffectHandler();

        if (statusEffectIconDatabase == null)
        {
            Debug.LogWarning(
                "[PlayerStatusUI] " +
                "StatusEffectIconDatabase가 연결되지 않았습니다.",
                this
            );
        }
    }

    private void OnEnable()
    {
        /*
         * 플레이어가 씬 시작 이후 생성되는 구조일 수 있으므로
         * OnEnable에서도 한 번 더 찾습니다.
         */
        if (statusEffectHandler == null)
        {
            FindStatusEffectHandler();
        }

        Subscribe();
        RefreshStatusUI();
    }

    private void Start()
    {
        /*
         * PlayerSpawner가 Start 시점에 플레이어를 생성하는 경우를
         * 대비하여 다시 한 번 연결합니다.
         */
        if (statusEffectHandler == null)
        {
            FindStatusEffectHandler();
            Subscribe();
            RefreshStatusUI();
        }
    }

    private void Update()
    {
        /*
         * 플레이어가 PlayerSpawner를 통해 늦게 생성되는 경우
         * StatusEffectHandler를 찾을 때까지 계속 연결을 시도합니다.
         */
        if (statusEffectHandler != null)
        {
            return;
        }

        FindStatusEffectHandler();

        if (statusEffectHandler == null)
        {
            return;
        }

        Subscribe();
        RefreshStatusUI();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    /// <summary>
    /// PlayerCombat과 StatusEffectHandler를 찾습니다.
    /// </summary>
    private void FindStatusEffectHandler()
    {
        if (playerCombat == null)
        {
            playerCombat =
                FindFirstObjectByType<PlayerCombat>();
        }

        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[PlayerStatusUI] PlayerCombat을 찾지 못했습니다.",
                this
            );

            return;
        }

        statusEffectHandler =
            playerCombat.GetComponent<StatusEffectHandler>();

        if (statusEffectHandler == null)
        {
            Debug.LogWarning(
                "[PlayerStatusUI] PlayerCombat 오브젝트에서 " +
                "StatusEffectHandler를 찾지 못했습니다.",
                playerCombat
            );
        }
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

        isSubscribed = true;
    }

    /// <summary>
    /// 상태 효과 변경 이벤트 연결을 해제합니다.
    /// </summary>
    private void Unsubscribe()
    {
        if (statusEffectHandler == null ||
            !isSubscribed)
        {
            return;
        }

        statusEffectHandler.StatusEffectsChanged -=
            RefreshStatusUI;

        isSubscribed = false;
    }

    /// <summary>
    /// 현재 상태 효과 목록을 기준으로
    /// 플레이어 상태 효과 UI를 갱신합니다.
    /// </summary>
    private void RefreshStatusUI()
    {
        if (statusEffectHandler == null)
        {
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

        RefreshContainerLayout();
    }

    /// <summary>
    /// 더 이상 적용되지 않은 상태 효과 아이콘을 제거합니다.
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
    /// 새로운 플레이어 상태 효과 아이콘을 생성합니다.
    /// </summary>
    private void CreateStatusIcon(
        StatusEffectData statusEffect)
    {
        if (statusIconPrefab == null)
        {
            Debug.LogWarning(
                "[PlayerStatusUI] 상태 효과 아이콘 프리팹이 없습니다.",
                this
            );

            return;
        }

        if (statusIconContainer == null)
        {
            Debug.LogWarning(
                "[PlayerStatusUI] 상태 효과 아이콘 생성 위치가 없습니다.",
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
    /// 상태 효과 종류에 맞는 아이콘을 반환합니다.
    /// </summary>
    private Sprite FindIconSprite(
        StatusEffectType statusEffectType)
    {
        if (statusEffectIconDatabase == null)
        {
            Debug.LogWarning(
                "[PlayerStatusUI] " +
                "StatusEffectIconDatabase가 연결되지 않았습니다.",
                this
            );

            return null;
        }

        Sprite iconSprite =
            statusEffectIconDatabase.GetIcon(
                statusEffectType
            );

        if (iconSprite == null)
        {
            Debug.LogWarning(
                $"[PlayerStatusUI] " +
                $"{statusEffectType} 아이콘을 찾지 못했습니다.",
                this
            );
        }

        return iconSprite;
    }

    /// <summary>
    /// 아이콘 배치를 즉시 갱신합니다.
    /// </summary>
    private void RefreshContainerLayout()
    {
        RectTransform containerRect =
            statusIconContainer as RectTransform;

        if (containerRect == null)
        {
            return;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            containerRect
        );
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