using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 적에게 적용된 상태 효과 아이콘 하나를 표시합니다.
///
/// 상태 효과 아이콘과 상태 수치,
/// 남은 지속 턴을 화면에 표시합니다.
/// </summary>
public class EnemyStatusIconUI : MonoBehaviour
{
    [Header("상태 효과 아이콘")]
    [SerializeField]
    private Image iconImage;

    [Header("상태 효과 수치")]
    [SerializeField]
    private TMP_Text valueText;

    [Header("남은 지속 턴")]
    [SerializeField]
    private TMP_Text turnText;

    private StatusEffectType statusEffectType;

    /// <summary>
    /// 현재 이 아이콘이 표시하는 상태 효과 종류입니다.
    /// </summary>
    public StatusEffectType StatusEffectType =>
        statusEffectType;

    /// <summary>
    /// 상태 효과 아이콘을 처음 생성할 때 호출합니다.
    /// </summary>
    public void Initialize(
        StatusEffectData statusEffect,
        Sprite iconSprite)
    {
        if (statusEffect == null)
        {
            Debug.LogWarning(
                "[EnemyStatusIconUI] 상태 효과 데이터가 없습니다.",
                this
            );

            return;
        }

        statusEffectType =
            statusEffect.statusEffectType;

        if (iconImage != null)
        {
            iconImage.sprite =
                iconSprite;

            iconImage.enabled =
                iconSprite != null;
        }

        Refresh(
            statusEffect
        );
    }

    /// <summary>
    /// 상태 효과의 현재 수치와 남은 턴을 갱신합니다.
    /// </summary>
    public void Refresh(
        StatusEffectData statusEffect)
    {
        if (statusEffect == null)
        {
            return;
        }

        RefreshValue(
        statusEffect.value,
        statusEffect.statusEffectType
        );

        RefreshTurn(
            statusEffect.remainingTurn,
            statusEffect.isPermanent,
            statusEffect.statusEffectType
        );
    }

    /// <summary>
    /// 상태 효과 종류에 따라 수치가 필요한 경우에만 표시합니다.
    /// 힘, 속도, 무감각, 불운, 마비, 중독 등
    /// 실제 수치가 게임 계산에 사용되는 상태만 표시합니다.
    /// </summary>
    private void RefreshValue(
        int value,
        StatusEffectType effectType)
    {
        if (valueText == null)
        {
            return;
        }

        bool shouldShow =
            ShouldShowValue(
                effectType
            ) &&
            value != 0;

        valueText.gameObject.SetActive(
            shouldShow
        );

        if (!shouldShow)
        {
            valueText.text = string.Empty;
            return;
        }

        valueText.text =
            value.ToString();
    }

    /// <summary>
    /// 아이콘에 상태 효과 수치를 표시해야 하는지 반환합니다.
    /// 고정 비율 효과는 수치를 표시하지 않습니다.
    /// </summary>
    private bool ShouldShowValue(
        StatusEffectType effectType)
    {
        switch (effectType)
        {
            case StatusEffectType.Might:
            case StatusEffectType.Guard:
            case StatusEffectType.Resist:
            case StatusEffectType.Jinx:
            case StatusEffectType.Paralyze:
            case StatusEffectType.Toxic:
            case StatusEffectType.MightReduction:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// 턴 감소형 상태 효과에만 남은 지속 턴을 표시합니다.
    /// 영구 효과와 수치형 효과에는 지속 턴을 표시하지 않습니다.
    /// </summary>
    private void RefreshTurn(
        int remainingTurn,
        bool isPermanent,
        StatusEffectType effectType)
    {
        if (turnText == null)
        {
            return;
        }

        bool shouldShow =
            !isPermanent &&
            ShouldShowRemainingTurn(
                effectType
            ) &&
            remainingTurn > 0;

        turnText.gameObject.SetActive(
            shouldShow
        );

        if (!shouldShow)
        {
            turnText.text = string.Empty;
            return;
        }

        turnText.text =
            remainingTurn.ToString();
    }

    /// <summary>
    /// 아이콘에 남은 지속 턴을 표시해야 하는지 반환합니다.
    /// 효과 강도가 고정되어 있고 턴이 감소하는 상태와
    /// 현재 턴에만 유지되는 일시 효과를 분류합니다.
    /// </summary>
    private bool ShouldShowRemainingTurn(
        StatusEffectType effectType)
    {
        switch (effectType)
        {
            /*
             * 현재 턴에만 유지되는 일시형 버프
             */
            case StatusEffectType.Lifesteal:
            case StatusEffectType.Echo:

            /*
             * 남은 턴이 감소하는 디버프
             */
            case StatusEffectType.Weaken:
            case StatusEffectType.Vulnerable:
            case StatusEffectType.Cripple:
            case StatusEffectType.NoBlock:
            case StatusEffectType.Broken:
            case StatusEffectType.MightReduction:
                return true;

            default:
                return false;
        }
    }
}
