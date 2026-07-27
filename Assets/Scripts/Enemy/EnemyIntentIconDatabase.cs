using UnityEngine;

/// <summary>
/// 적 Intent UI에서 사용할 아이콘들을 보관합니다.
///
/// 피해 아이콘은 총 예상 피해에 따라
/// 4단계로 나누어 반환합니다.
/// </summary>
[CreateAssetMenu(
    fileName = "EnemyIntentIconDatabase",
    menuName = "Enemy/Intent Icon Database"
)]
public class EnemyIntentIconDatabase : ScriptableObject
{
    [Header("피해 1 ~ 10")]
    [SerializeField]
    private Sprite damageTier1Icon;

    [Header("피해 11 ~ 20")]
    [SerializeField]
    private Sprite damageTier2Icon;

    [Header("피해 21 ~ 30")]
    [SerializeField]
    private Sprite damageTier3Icon;

    [Header("피해 31 이상")]
    [SerializeField]
    private Sprite damageTier4Icon;

    [Header("방어도")]
    [SerializeField]
    private Sprite blockIcon;

    [Header("버프")]
    [SerializeField]
    private Sprite buffIcon;

    [Header("디버프")]
    [SerializeField]
    private Sprite debuffIcon;

    /// <summary>
    /// 총 예상 피해에 맞는 피해 아이콘을 반환합니다.
    /// </summary>
    public Sprite GetDamageIcon(
        int totalDamage)
    {
        if (totalDamage <= 0)
        {
            return null;
        }

        if (totalDamage <= 10)
        {
            return damageTier1Icon;
        }

        if (totalDamage <= 20)
        {
            return damageTier2Icon;
        }

        if (totalDamage <= 30)
        {
            return damageTier3Icon;
        }

        return damageTier4Icon;
    }

    /// <summary>
    /// 방어도 Intent 아이콘입니다.
    /// </summary>
    public Sprite BlockIcon =>
        blockIcon;

    /// <summary>
    /// 버프 Intent 아이콘입니다.
    /// </summary>
    public Sprite BuffIcon =>
        buffIcon;

    /// <summary>
    /// 디버프 Intent 아이콘입니다.
    /// </summary>
    public Sprite DebuffIcon =>
        debuffIcon;
}