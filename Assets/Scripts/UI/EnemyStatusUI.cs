using UnityEngine.UI;
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
    private const string TopBarBackgroundName =
        "TopBarBackground";

    private static readonly Color TopBarBackgroundColor =
        new Color(0.45f, 0.45f, 0.45f, 0.85f);

    private static readonly Vector2 TopBarBackgroundExpansion =
        new Vector2(20f, 10f);

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

    private readonly Dictionary<
        StatusEffectType,
        EnemyStatusIconUI> createdIcons =
        new Dictionary<
            StatusEffectType,
            EnemyStatusIconUI>();

    private void Awake()
    {
        ConfigureTopBarBackground();

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

        if (statusEffectIconDatabase == null)
        {
            Debug.LogWarning(
                "[EnemyStatusUI] " +
                "StatusEffectIconDatabase가 연결되지 않았습니다.",
                this
            );
        }
    }

    /// <summary>
    /// 상태·작살·Intent·방어도 UI 뒤에 공통 회색 배경을 표시합니다.
    /// 체력바는 별도 루트이므로 배경 적용 대상에서 제외됩니다.
    /// </summary>
    private void ConfigureTopBarBackground()
    {
        Transform backgroundTransform =
            transform.Find(TopBarBackgroundName);

        GameObject backgroundObject;

        if (backgroundTransform == null)
        {
            backgroundObject = new GameObject(
                TopBarBackgroundName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

            backgroundObject.layer = gameObject.layer;
            backgroundObject.transform.SetParent(
                transform,
                false
            );
        }
        else
        {
            backgroundObject =
                backgroundTransform.gameObject;
        }

        backgroundObject.transform.SetAsFirstSibling();

        RectTransform backgroundRect =
            backgroundObject.GetComponent<RectTransform>();

        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.anchoredPosition = Vector2.zero;
        backgroundRect.sizeDelta = TopBarBackgroundExpansion;

        Image backgroundImage =
            backgroundObject.GetComponent<Image>();

        backgroundImage.sprite = null;
        backgroundImage.type = Image.Type.Simple;
        backgroundImage.color = TopBarBackgroundColor;
        backgroundImage.raycastTarget = false;
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
        /*
         * StatusRoot에는 상태 효과 아이콘뿐 아니라
         * 작살 스택 UI도 함께 들어갑니다.
         *
         * 따라서 상태 효과가 없어도
         * StatusRoot 자체는 끄지 않습니다.
         */
        SetStatusRootActive(true);

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

        RectTransform containerRect =
    statusIconContainer as RectTransform;

        if (containerRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                containerRect
            );
        }
    }

    /// <summary>
    /// 상태 효과 종류에 해당하는 아이콘 Sprite를
    /// 데이터베이스에서 가져옵니다.
    /// </summary>
    private Sprite FindIconSprite(
        StatusEffectType statusEffectType)
    {
        if (statusEffectIconDatabase == null)
        {
            Debug.LogWarning(
                "[EnemyStatusUI] " +
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
                $"[EnemyStatusUI] " +
                $"{statusEffectType} 아이콘을 찾지 못했습니다.",
                this
            );
        }

        return iconSprite;
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
