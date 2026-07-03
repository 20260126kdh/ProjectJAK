using System;

/// <summary>
/// 카드 효과 1개의 데이터를 담는 클래스입니다.
/// CardData 안의 effects 리스트에 들어갑니다.
/// </summary>
[Serializable]
public class CardEffectData
{
    /// <summary>
    /// 효과 실행 순서입니다.
    /// 낮은 숫자부터 실행됩니다.
    /// </summary>
    public int order;

    /// <summary>
    /// 카드 효과 종류입니다.
    /// 예: 피해, 방어도 획득, 상태 부여, 드로우 등
    /// </summary>
    public CardEffectType effectType;

    /// <summary>
    /// 상태 효과 종류입니다.
    /// effectType이 ApplyStatus일 때 사용합니다.
    /// </summary>
    public StatusEffectType statusEffectType;

    /// <summary>
    /// 효과 수치입니다.
    /// 예: 피해량, 방어도, 드로우 장수, 상태 스택 수 등
    /// </summary>
    public int value;

    /// <summary>
    /// 효과 대상입니다.
    /// 예: 자신, 적, 모든 적, 소환수
    /// </summary>
    public CardTargetType target;
}