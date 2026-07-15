using System;

/// <summary>
/// EnemyPatterns.csv에 기록된 적 행동 한 줄의 데이터를 저장합니다.
///
/// 같은 적의 같은 패턴 턴에는 여러 행동이 존재할 수 있으며,
/// ExecutionOrder 순서대로 실행됩니다.
/// </summary>
[Serializable]
public class EnemyPatternData
{
    /// <summary>
    /// 패턴을 사용하는 적의 고유 ID입니다.
    /// </summary>
    public string enemyId;

    /// <summary>
    /// 행동이 실행되는 패턴 순서입니다.
    /// </summary>
    public int patternTurn;

    /// <summary>
    /// 같은 패턴 순서 내에서의 실행 순서입니다.
    /// </summary>
    public int executionOrder;

    /// <summary>
    /// 실행할 행동 종류입니다.
    /// </summary>
    public EnemyPatternActionType actionType;

    /// <summary>
    /// 피해량, 방어도, 상태효과 수치 등
    /// 행동에서 사용하는 기본 값입니다.
    /// </summary>
    public int value;

    /// <summary>
    /// 행동을 반복할 횟수입니다.
    /// </summary>
    public int repeatCount;

    /// <summary>
    /// 행동의 대상입니다.
    /// </summary>
    public EnemyPatternTargetType targetType;

    /// <summary>
    /// 상태효과 행동에서 사용할 상태효과입니다.
    ///
    /// CSV 값이 비어 있거나 None이라면
    /// hasStatusType이 false가 됩니다.
    /// </summary>
    public StatusEffectType statusType;

    /// <summary>
    /// 상태효과가 실제로 지정됐는지 여부입니다.
    /// </summary>
    public bool hasStatusType;

    /// <summary>
    /// 상태효과 지속 턴입니다.
    /// </summary>
    public int duration;

    /// <summary>
    /// 상태효과의 영구 유지 여부입니다.
    /// </summary>
    public bool isPermanent;

    /// <summary>
    /// 행동 실행 조건입니다.
    /// </summary>
    public EnemyPatternConditionType conditionType;

    /// <summary>
    /// 조건 판정에 사용할 추가 수치입니다.
    /// </summary>
    public int conditionValue;
}