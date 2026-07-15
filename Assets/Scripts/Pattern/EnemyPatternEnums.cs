/// <summary>
/// 적 패턴에서 실행할 행동 종류입니다.
/// </summary>
public enum EnemyPatternActionType
{
    None,

    /// <summary>
    /// 대상에게 피해를 줍니다.
    /// </summary>
    DealDamage,

    /// <summary>
    /// 자신이 방어도를 획득합니다.
    /// </summary>
    GainBlock,

    /// <summary>
    /// 대상에게 상태효과를 부여합니다.
    /// </summary>
    ApplyStatus,

    /// <summary>
    /// 자신의 체력을 회복합니다.
    /// </summary>
    Heal,

    /// <summary>
    /// 장송의 원혼 소환을 요청합니다.
    /// </summary>
    SummonFuneralSpirit
}

/// <summary>
/// 적 패턴 행동의 대상입니다.
/// </summary>
public enum EnemyPatternTargetType
{
    None,

    /// <summary>
    /// 행동을 실행하는 적 자신입니다.
    /// </summary>
    Self,

    /// <summary>
    /// 플레이어입니다.
    /// </summary>
    Player
}

/// <summary>
/// 적 패턴 행동을 실행하기 위한 조건입니다.
/// </summary>
public enum EnemyPatternConditionType
{
    None,

    /// <summary>
    /// 현재 장송의 원혼이 한 마리 이상 존재합니다.
    /// </summary>
    HasActiveFuneralSpirit,

    /// <summary>
    /// 현재 장송의 원혼이 존재하지 않습니다.
    /// </summary>
    NoActiveFuneralSpirit,

    /// <summary>
    /// 현재 원혼 수가 지정된 값보다 적습니다.
    /// </summary>
    FuneralSpiritCountBelow
}