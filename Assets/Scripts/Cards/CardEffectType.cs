/// <summary>
/// 카드가 즉시 실행할 수 있는 효과 종류입니다.
/// 상태 효과 자체는 StatusEffectType에서 관리합니다.
/// </summary>
public enum CardEffectType
{
    // 기본 효과
    DealDamage,
    GainBlock,
    Heal,
    LoseHealth,
    DrawCard,

    // 상태 효과 부여
    ApplyStatus,

    // 작살 스택
    ApplyHarpoon,
    HarpoonerStack,
    DealDamageEqualToHarpoonerStack,

    // 조건부 효과
    GainBlockOnHealthLossThisTurn,
    DoubleNextAttackDamage,

    // 소환 / 캡틴 전용
    Summon,
    SummonCrew,
    CrewDealDamage,
    Sacrifice,
    SacrificeAll,
    SetMaxHealth,
    MightEqualToSacrificedHealth
}