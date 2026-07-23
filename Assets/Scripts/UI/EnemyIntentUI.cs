using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// 적이 다음 턴에 실행할 행동을 화면에 표시합니다.
///
/// 현재 패턴 턴의 조건을 만족하는 행동들을 가져와
/// 하나의 Intent 문자열로 조합합니다.
///
/// 표시할 행동이 없으면 IntentRoot를 숨깁니다.
/// </summary>
public class EnemyIntentUI : MonoBehaviour
{
    [Header("Intent UI 루트")]
    [SerializeField]
    private GameObject intentRoot;

    [Header("Intent 텍스트")]
    [SerializeField]
    private TMP_Text intentText;

    private EnemyPatternController patternController;

    private void Awake()
    {
        patternController =
            GetComponentInParent<EnemyPatternController>();

        if (patternController == null)
        {
            Debug.LogWarning(
                "[EnemyIntentUI] 부모 오브젝트에서 " +
                "EnemyPatternController를 찾지 못했습니다.",
                this
            );
        }
    }

    private void Start()
    {
        /*
         * EnemyPatternController의 Start에서 Initialize가 실행되므로
         * 첫 프레임이 끝난 뒤 현재 Intent를 한 번 표시합니다.
         */
        RefreshIntent();
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    /// <summary>
    /// 패턴 컨트롤러의 Intent 변경 이벤트를 연결합니다.
    /// </summary>
    private void Subscribe()
    {
        if (patternController == null)
        {
            return;
        }

        patternController.IntentChanged -=
            RefreshIntent;

        patternController.IntentChanged +=
            RefreshIntent;
    }

    /// <summary>
    /// Intent 변경 이벤트 연결을 해제합니다.
    /// </summary>
    private void Unsubscribe()
    {
        if (patternController == null)
        {
            return;
        }

        patternController.IntentChanged -=
            RefreshIntent;
    }

    /// <summary>
    /// 현재 조건을 만족하는 패턴 행동을 읽어
    /// Intent 텍스트를 갱신합니다.
    /// </summary>
    public void RefreshIntent()
    {
        if (patternController == null)
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

        string intentDescription =
            CreateIntentDescription(actions);

        bool hasDescription =
            !string.IsNullOrWhiteSpace(
                intentDescription
            );

        SetIntentRootActive(
            hasDescription
        );

        if (intentText != null)
        {
            intentText.text =
                intentDescription;
        }
    }

    /// <summary>
    /// 여러 행동을 하나의 Intent 문장으로 조합합니다.
    /// </summary>
    private string CreateIntentDescription(
        List<EnemyPatternData> actions)
    {
        StringBuilder builder =
            new StringBuilder();

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

            string actionText =
                CreateActionDescription(action);

            if (string.IsNullOrWhiteSpace(actionText))
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append(" + ");
            }

            builder.Append(actionText);
        }

        return builder.ToString();
    }

    /// <summary>
    /// 행동 하나를 플레이어에게 보여줄 문자열로 변환합니다.
    /// </summary>
    private string CreateActionDescription(
        EnemyPatternData action)
    {
        switch (action.actionType)
        {
            case EnemyPatternActionType.DealDamage:
                return CreateDamageDescription(action);

            case EnemyPatternActionType.GainBlock:
                return $"방어 {action.value}";

            case EnemyPatternActionType.ApplyStatus:
                return CreateStatusDescription(action);

            case EnemyPatternActionType.Heal:
                return $"회복 {action.value}";

            case EnemyPatternActionType.SummonFuneralSpirit:
                return "원혼 소환";

            case EnemyPatternActionType.NoAction:
                return "대기";

            case EnemyPatternActionType.ReadyToStrongAttack:
                return "강공격 준비";

            case EnemyPatternActionType.None:
            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// 피해 행동의 표시 문자열을 생성합니다.
    /// </summary>
    private string CreateDamageDescription(
        EnemyPatternData action)
    {
        int repeatCount =
            Mathf.Max(1, action.repeatCount);

        if (repeatCount <= 1)
        {
            return $"공격 {action.value}";
        }

        return $"공격 {action.value} × {repeatCount}";
    }

    /// <summary>
    /// 상태효과 부여 행동의 표시 문자열을 생성합니다.
    /// </summary>
    private string CreateStatusDescription(
        EnemyPatternData action)
    {
        if (!action.hasStatusType)
        {
            return "상태효과 부여";
        }

        string statusName =
            GetStatusDisplayName(
                action.statusType
            );

        if (action.value > 0)
        {
            return $"{statusName} {action.value}";
        }

        return statusName;
    }

    /// <summary>
    /// 상태효과 enum을 플레이어에게 표시할 이름으로 변환합니다.
    /// </summary>
    private string GetStatusDisplayName(
        StatusEffectType statusEffectType)
    {
        switch (statusEffectType)
        {
            case StatusEffectType.Might:
                return "힘";

            case StatusEffectType.Guard:
                return "수호";

            case StatusEffectType.Resist:
                return "저항";

            case StatusEffectType.Lifesteal:
                return "흡혈";

            case StatusEffectType.Echo:
                return "메아리";

            case StatusEffectType.Immortal:
                return "불사";

            case StatusEffectType.Weaken:
                return "약화";

            case StatusEffectType.Vulnerable:
                return "취약";

            case StatusEffectType.Cripple:
                return "불구";

            case StatusEffectType.NoBlock:
                return "방어 불가";

            case StatusEffectType.Broken:
                return "파괴";

            case StatusEffectType.Jinx:
                return "징크스";

            case StatusEffectType.Paralyze:
                return "마비";

            case StatusEffectType.Toxic:
                return "중독";

            default:
                return statusEffectType.ToString();
        }
    }

    /// <summary>
    /// IntentRoot의 표시 여부를 변경합니다.
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
}