using System;

/// <summary>
/// 인간형 AutoPlayer가 한 번의 관찰 뒤 선택한 단일 행동입니다.
/// </summary>
[Serializable]
public class PolishHumanDecision
{
    public PolishDecisionType decisionType;
    public PolishDecisionTarget target;
    public int handIndex = -1;
    public int enemyIndex = -1;
    public int crewOrder = -1;
    public PolishDangerLevel dangerLevel;
    public int expectedIncomingDamage;
    public string reason;
}

/// <summary>
/// 인간형 AutoPlayer가 실행할 수 있는 공통 행동 종류입니다.
/// </summary>
public enum PolishDecisionType
{
    UseCard,
    EndTurn
}

/// <summary>
/// 선택 카드의 화면 클릭 대상입니다.
/// </summary>
public enum PolishDecisionTarget
{
    None,
    Player,
    Enemy,
    Crew
}

/// <summary>
/// 현재 공개 Intent와 생존 상태로 평가한 위험 단계입니다.
/// </summary>
public enum PolishDangerLevel
{
    Safe,
    Caution,
    High,
    Lethal
}
