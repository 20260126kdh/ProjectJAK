/// <summary>
/// 캐릭터, 적, 소환수에게 적용될 수 있는 상태 효과 종류입니다.
/// 버프, 디버프, 패시브 효과를 모두 포함합니다.
/// </summary>
public enum StatusEffectType
{
    None,

    // 버프
    Might,
    Guard,
    Resist,
    Lifesteal,
    Echo,
    Immortal,
    Undead,
    KShellguard,
    DToxinSwitch,
    FFesteredSkin,
    HRevelation,
    UnderGround,
    UnderWater,
    ProfanedHalo,
    DevilPower,

    // 디버프
    Weaken,
    Vulnerable,
    Cripple,
    NoBlock,
    Broken,
    Jinx,
    Paralyze,
    Toxic,
    Exit
}