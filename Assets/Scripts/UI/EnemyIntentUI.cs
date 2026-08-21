using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적이 다음 턴에 실행할 행동을 아이콘으로 표시합니다.
///
/// 현재 패턴 턴의 조건을 만족하는 행동을 읽어
/// 피해, 방어도, 버프, 디버프 네 종류로 분류합니다.
///
/// 피해 아이콘은 모든 공격의 총 최종 피해를 기준으로 결정합니다.
///
/// 다단히트 공격은 화면에:
/// 9 × 3
///
/// 형식으로 표시합니다.
/// </summary>
public class EnemyIntentUI : MonoBehaviour
{
    [Header("Intent UI 루트")]
    [SerializeField]
    private GameObject intentRoot;

    [Header("아이콘 생성 위치")]
    [SerializeField]
    private Transform iconContainer;

    [Header("Intent 아이콘 프리팹")]
    [SerializeField]
    private EnemyIntentIconUI intentIconPrefab;

    [Header("Intent 아이콘 데이터베이스")]
    [SerializeField]
    private EnemyIntentIconDatabase iconDatabase;

    private EnemyPatternController patternController;
    private Enemy ownerEnemy;
    private StatusEffectHandler statusEffectHandler;
    private readonly List<string> visibleIntentTexts = new List<string>();

    /// <summary>
    /// 현재 Intent UI에 실제로 생성된 아이콘의 표시 문자열입니다.
    /// 피해, 방어, 버프와 디버프 순서를 유지합니다.
    /// </summary>
    public IReadOnlyList<string> VisibleIntentTexts =>
        visibleIntentTexts.AsReadOnly();

    /// <summary>
    /// 현재 피해 Intent에 표시할 문자열입니다.
    ///
    /// 예:
    /// 17
    /// 9 × 3
    /// 5 + 9 × 3
    /// </summary>
    private string damageDisplayText;

    private void Awake()
    {
        FindReferences();
    }

    private void Start()
    {
        /*
         * EnemyPatternController의 Start에서
         * Initialize가 실행되므로 시작 시 한 번 갱신합니다.
         */
        RefreshIntent();
    }

    private void OnEnable()
    {
        FindReferences();
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    /// <summary>
    /// 필요한 컴포넌트들을 자동으로 찾습니다.
    /// </summary>
    private void FindReferences()
    {
        if (patternController == null)
        {
            patternController =
                GetComponentInParent<EnemyPatternController>();
        }

        if (ownerEnemy == null)
        {
            ownerEnemy =
                GetComponentInParent<Enemy>();
        }

        if (statusEffectHandler == null &&
            ownerEnemy != null)
        {
            statusEffectHandler =
                ownerEnemy.GetComponent<StatusEffectHandler>();
        }

        if (patternController == null)
        {
            Debug.LogWarning(
                "[EnemyIntentUI] 부모 오브젝트에서 " +
                "EnemyPatternController를 찾지 못했습니다.",
                this
            );
        }

        if (ownerEnemy == null)
        {
            Debug.LogWarning(
                "[EnemyIntentUI] 부모 오브젝트에서 " +
                "Enemy를 찾지 못했습니다.",
                this
            );
        }
    }

    /// <summary>
    /// 패턴 진행과 적 상태 효과 변경 이벤트를 연결합니다.
    /// </summary>
    private void Subscribe()
    {
        if (patternController != null)
        {
            patternController.IntentChanged -=
                RefreshIntent;

            patternController.IntentChanged +=
                RefreshIntent;
        }

        if (statusEffectHandler != null)
        {
            statusEffectHandler.StatusEffectsChanged -=
                RefreshIntent;

            statusEffectHandler.StatusEffectsChanged +=
                RefreshIntent;
        }
    }

    /// <summary>
    /// 패턴 진행과 적 상태 효과 변경 이벤트를 해제합니다.
    /// </summary>
    private void Unsubscribe()
    {
        if (patternController != null)
        {
            patternController.IntentChanged -=
                RefreshIntent;
        }

        if (statusEffectHandler != null)
        {
            statusEffectHandler.StatusEffectsChanged -=
                RefreshIntent;
        }
    }

    /// <summary>
    /// 현재 조건을 만족하는 패턴 행동을 분석해
    /// Intent 아이콘을 다시 생성합니다.
    /// </summary>
    public void RefreshIntent()
    {
        FindReferences();
        ClearGeneratedIcons();
        visibleIntentTexts.Clear();

        if (!CanRefreshIntent())
        {
            SetIntentRootActive(false);
            return;
        }

        if (!patternController.IsInitialized)
        {
            patternController.Initialize();
        }

        List<EnemyPatternData> actions =
            patternController.GetCurrentIntentActions();

        if (actions == null ||
            actions.Count == 0)
        {
            SetIntentRootActive(false);
            return;
        }

        int totalDamage = 0;
        int totalBlock = 0;
        int totalBuffValue = 0;
        int totalDebuffValue = 0;

        bool hasBuff = false;
        bool hasDebuff = false;

        damageDisplayText =
            string.Empty;

        for (int i = 0;
             i < actions.Count;
             i++)
        {
            EnemyPatternData action =
                actions[i];

            if (action == null)
            {
                continue;
            }

            switch (action.actionType)
            {
                case EnemyPatternActionType.DealDamage:
                    {
                        int actionTotalDamage =
                            CalculateTotalDamage(
                                action,
                                out string actionDisplayText
                            );

                        totalDamage +=
                            actionTotalDamage;

                        AddDamageDisplayText(
                            actionDisplayText
                        );

                        break;
                    }

                case EnemyPatternActionType.GainBlock:
                    {
                        totalBlock +=
                            Mathf.Max(
                                0,
                                action.value
                            );

                        break;
                    }

                case EnemyPatternActionType.ApplyStatus:
                    {
                        ClassifyStatusAction(
                            action,
                            ref hasBuff,
                            ref totalBuffValue,
                            ref hasDebuff,
                            ref totalDebuffValue
                        );

                        break;
                    }
            }
        }

        int createdIconCount = 0;

        /*
         * Intent 표시 순서:
         * 피해 → 방어도 → 버프 → 디버프
         */

        if (totalDamage > 0)
        {
            bool wasCreated =
                CreateIntentIcon(
                    iconDatabase.GetDamageIcon(
                        totalDamage
                    ),
                    damageDisplayText
                );

            if (wasCreated)
            {
                createdIconCount++;
                visibleIntentTexts.Add($"Damage:{damageDisplayText}");
            }
        }

        if (totalBlock > 0)
        {
            bool wasCreated =
                CreateIntentIcon(
                    iconDatabase.BlockIcon,
                    totalBlock.ToString()
                );

            if (wasCreated)
            {
                createdIconCount++;
                visibleIntentTexts.Add($"Block:{totalBlock}");
            }
        }

        if (hasBuff)
        {
            string buffDisplayText =
                totalBuffValue > 0
                    ? totalBuffValue.ToString()
                    : string.Empty;

            bool wasCreated =
                CreateIntentIcon(
                    iconDatabase.BuffIcon,
                    buffDisplayText
                );

            if (wasCreated)
            {
                createdIconCount++;
                visibleIntentTexts.Add($"Buff:{buffDisplayText}");
            }
        }

        if (hasDebuff)
        {
            string debuffDisplayText =
                totalDebuffValue > 0
                    ? totalDebuffValue.ToString()
                    : string.Empty;

            bool wasCreated =
                CreateIntentIcon(
                    iconDatabase.DebuffIcon,
                    debuffDisplayText
                );

            if (wasCreated)
            {
                createdIconCount++;
                visibleIntentTexts.Add($"Debuff:{debuffDisplayText}");
            }
        }

        SetIntentRootActive(
            createdIconCount > 0
        );
    }

    /// <summary>
    /// Intent UI를 갱신할 수 있는 상태인지 확인합니다.
    /// </summary>
    private bool CanRefreshIntent()
    {
        if (patternController == null)
        {
            return false;
        }

        if (iconContainer == null)
        {
            Debug.LogWarning(
                "[EnemyIntentUI] " +
                "Icon Container가 연결되지 않았습니다.",
                this
            );

            return false;
        }

        if (intentIconPrefab == null)
        {
            Debug.LogWarning(
                "[EnemyIntentUI] " +
                "Intent Icon Prefab이 연결되지 않았습니다.",
                this
            );

            return false;
        }

        if (iconDatabase == null)
        {
            Debug.LogWarning(
                "[EnemyIntentUI] " +
                "Intent Icon Database가 연결되지 않았습니다.",
                this
            );

            return false;
        }

        return true;
    }

    /// <summary>
    /// 피해 행동 하나의 총 최종 피해를 계산하고,
    /// 화면에 표시할 공격 문자열을 반환합니다.
    ///
    /// 피해 아이콘 판정:
    /// 최종 1회 피해 × 반복 횟수
    ///
    /// 화면 표기:
    /// 최종 1회 피해 × 반복 횟수
    ///
    /// 예:
    /// 기본 피해 9, 힘 1, 반복 3회
    /// → 1회 최종 피해 10
    /// → 총 피해 30
    /// → 화면 표기 "10 × 3"
    /// </summary>
    private int CalculateTotalDamage(
        EnemyPatternData action,
        out string displayText)
    {
        int baseDamage =
            Mathf.Max(
                0,
                action.value
            );

        int repeatCount =
            Mathf.Max(
                1,
                action.repeatCount
            );

        int oneHitDamage =
            baseDamage;

        if (ownerEnemy != null)
        {
            oneHitDamage =
                ownerEnemy.CalculateOutgoingDamage(
                    baseDamage
                );
        }

        oneHitDamage =
            Mathf.Max(
                0,
                oneHitDamage
            );

        if (repeatCount > 1)
        {
            displayText =
                $"{oneHitDamage}x{repeatCount}";
        }
        else
        {
            displayText =
                oneHitDamage.ToString();
        }

        return oneHitDamage *
               repeatCount;
    }

    /// <summary>
    /// 한 턴에 피해 행동이 여러 개 존재할 경우
    /// 각 공격 표기를 하나의 문자열로 조합합니다.
    ///
    /// 예:
    /// 5 피해 후 9 × 3 피해
    /// → 5 + 9 × 3
    /// </summary>
    private void AddDamageDisplayText(
        string actionDisplayText)
    {
        if (string.IsNullOrWhiteSpace(
                actionDisplayText))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(
                damageDisplayText))
        {
            damageDisplayText =
                actionDisplayText;

            return;
        }

        damageDisplayText +=
            $" + {actionDisplayText}";
    }

    /// <summary>
    /// 상태효과 행동을 버프 또는 디버프로 분류합니다.
    /// </summary>
    private void ClassifyStatusAction(
        EnemyPatternData action,
        ref bool hasBuff,
        ref int totalBuffValue,
        ref bool hasDebuff,
        ref int totalDebuffValue)
    {
        if (!action.hasStatusType)
        {
            return;
        }

        int statusValue =
            Mathf.Max(
                0,
                action.value
            );

        if (IsBuffStatus(action.statusType))
        {
            hasBuff = true;

            totalBuffValue +=
                statusValue;

            return;
        }

        if (IsDebuffStatus(action.statusType))
        {
            hasDebuff = true;

            totalDebuffValue +=
                statusValue;
        }
    }

    /// <summary>
    /// 버프로 분류되는 상태효과인지 반환합니다.
    /// </summary>
    private bool IsBuffStatus(
        StatusEffectType statusType)
    {
        switch (statusType)
        {
            case StatusEffectType.Might:
            case StatusEffectType.Guard:
            case StatusEffectType.Resist:
            case StatusEffectType.Lifesteal:
            case StatusEffectType.Echo:
            case StatusEffectType.Immortal:
            case StatusEffectType.Undead:
            case StatusEffectType.KShellguard:
            case StatusEffectType.DToxinSwitch:
            case StatusEffectType.FFesteredSkin:
            case StatusEffectType.HRevelation:
            case StatusEffectType.UnderGround:
            case StatusEffectType.UnderWater:
            case StatusEffectType.ProfanedHalo:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// 디버프로 분류되는 상태효과인지 반환합니다.
    /// </summary>
    private bool IsDebuffStatus(
        StatusEffectType statusType)
    {
        switch (statusType)
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

    /// <summary>
    /// Intent 아이콘 프리팹을 생성합니다.
    ///
    /// 정상적으로 생성됐으면 true를 반환합니다.
    /// </summary>
    private bool CreateIntentIcon(
        Sprite iconSprite,
        string displayText)
    {
        if (iconSprite == null)
        {
            Debug.LogWarning(
                "[EnemyIntentUI] " +
                "표시할 Intent Sprite가 없습니다.",
                this
            );

            return false;
        }

        EnemyIntentIconUI createdIcon =
            Instantiate(
                intentIconPrefab,
                iconContainer
            );

        if (createdIcon == null)
        {
            Debug.LogWarning(
                "[EnemyIntentUI] " +
                "Intent 아이콘 생성에 실패했습니다.",
                this
            );

            return false;
        }

        createdIcon.Initialize(
            iconSprite,
            displayText
        );

        return true;
    }

    /// <summary>
    /// 기존에 생성된 Intent 아이콘들을 제거합니다.
    /// </summary>
    private void ClearGeneratedIcons()
    {
        if (iconContainer == null)
        {
            return;
        }

        for (int i =
                 iconContainer.childCount - 1;
             i >= 0;
             i--)
        {
            Transform child =
                iconContainer.GetChild(i);

            Destroy(
                child.gameObject
            );
        }
    }

    /// <summary>
    /// IntentRoot 표시 여부를 변경합니다.
    /// </summary>
    private void SetIntentRootActive(
        bool isActive)
    {
        if (intentRoot == null)
        {
            return;
        }

        if (intentRoot.activeSelf == isActive)
        {
            return;
        }

        intentRoot.SetActive(
            isActive
        );
    }

    /// <summary>
    /// Inspector에서 현재 Intent를 다시 갱신합니다.
    /// </summary>
    [ContextMenu("Intent 아이콘 새로고침")]
    private void TestRefreshIntent()
    {
        RefreshIntent();
    }
}
