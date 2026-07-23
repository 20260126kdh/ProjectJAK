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
            statusEffect.value
        );

        RefreshTurn(
            statusEffect.remainingTurn,
            statusEffect.isPermanent,
            statusEffect.statusEffectType
        );
    }

    /// <summary>
    /// 상태 효과 수치를 표시합니다.
    /// 수치가 0 이하라면 숫자를 숨깁니다.
    /// </summary>
    private void RefreshValue(
        int value)
    {
        if (valueText == null)
        {
            return;
        }

        bool shouldShow =
            value > 0;

        valueText.gameObject.SetActive(
            shouldShow
        );

        if (!shouldShow)
        {
            return;
        }

        valueText.text =
            value.ToString();
    }

    /// <summary>
    /// 상태 효과의 남은 지속 턴을 표시합니다.
    ///
    /// 영구 상태 효과와 중독은
    /// 지속 턴 숫자를 표시하지 않습니다.
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
            effectType != StatusEffectType.Toxic &&
            remainingTurn > 0;

        turnText.gameObject.SetActive(
            shouldShow
        );

        if (!shouldShow)
        {
            return;
        }

        turnText.text =
            remainingTurn.ToString();
    }
}