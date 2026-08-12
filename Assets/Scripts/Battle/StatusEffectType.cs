/// <summary>
/// 캐릭터, 적, 소환수에게 적용될 수 있는 상태 효과 종류입니다.
/// 버프, 디버프, 패시브 효과를 모두 포함합니다.
/// </summary>
public enum StatusEffectType
{
    None = 0,

    // 버프
    Might = 1,
    Guard = 2,
    Resist = 3,
    Lifesteal = 4,
    Echo = 5,
    Immortal = 6,
    Undead = 7,
    KShellguard = 8,
    DToxinSwitch = 9,
    FFesteredSkin = 10,
    HRevelation = 11,
    UnderGround = 12,
    UnderWater = 13,
    ProfanedHalo = 14,

    // 디버프
    Weaken = 15,
    Vulnerable = 16,
    Cripple = 17,
    NoBlock = 18,
    Broken = 19,
    Jinx = 20,
    Paralyze = 21,
    Toxic = 22,
    Exit = 23,

    // 기존 직렬화 값을 보존하기 위해 새 효과는 마지막 번호를 사용합니다.
    DevilPower = 24,
    MightReduction = 25
}
