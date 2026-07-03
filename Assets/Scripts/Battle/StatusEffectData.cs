using System;

/// <summary>
/// 캐릭터, 적, 소환수에게 적용된 상태 효과 1개의 정보를 저장하는 클래스입니다.
/// </summary>
[Serializable]
public class StatusEffectData
{
    /// <summary>
    /// 상태 효과 종류입니다.
    /// 예: Might, Weaken, Toxic 등
    /// </summary>
    public StatusEffectType statusEffectType;

    /// <summary>
    /// 상태 효과 수치입니다.
    /// 예: 힘 2, 중독 5, 약화 1 등
    /// </summary>
    public int value;

    /// <summary>
    /// 남은 턴 수입니다.
    /// 영구 효과라면 -1을 사용합니다.
    /// </summary>
    public int remainingTurn;

    /// <summary>
    /// 영구 유지 효과인지 여부입니다.
    /// </summary>
    public bool isPermanent;

    /// <summary>
    /// 상태 효과 데이터를 생성합니다.
    /// </summary>
    public StatusEffectData(StatusEffectType statusEffectType, int value, int remainingTurn, bool isPermanent)
    {
        this.statusEffectType = statusEffectType;
        this.value = value;
        this.remainingTurn = remainingTurn;
        this.isPermanent = isPermanent;
    }
}