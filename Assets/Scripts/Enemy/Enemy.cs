using UnityEngine;

/// <summary>
/// 전투 중 적의 체력을 관리하는 임시 Enemy 클래스입니다.
/// 현재는 데미지 받기와 사망 로그만 처리합니다.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("적 최대 체력")]
    [SerializeField]
    private int maxHP = 30;

    [Header("적 현재 체력")]
    [SerializeField]
    private int currentHP;

    /// <summary>
    /// 적 현재 체력을 반환합니다.
    /// </summary>
    public int CurrentHP => currentHP;

    /// <summary>
    /// 적 최대 체력을 반환합니다.
    /// </summary>
    public int MaxHP => maxHP;

    private void Awake()
    {
        currentHP = maxHP;
    }

    /// <summary>
    /// 적이 데미지를 받습니다.
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        if (currentHP < 0)
        {
            currentHP = 0;
        }

        Debug.Log($"[Enemy] 데미지 받음 : {damage} / 현재 체력 : {currentHP}");

        if (currentHP <= 0)
        {
            Debug.Log("[Enemy] 적 사망");
        }
    }
}