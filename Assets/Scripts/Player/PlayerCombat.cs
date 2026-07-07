using UnityEngine;

/// <summary>
/// 전투 중 플레이어의 능력치를 관리하는 클래스입니다.
/// HP, Block 등의 전투 수치를 관리하며,
/// 실제 데이터는 PlayerData와 연동합니다.
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    [Header("Player Data")]
    [SerializeField]
    private PlayerData playerData;

    [Header("현재 방어도")]
    [SerializeField]
    private int currentBlock;

    [Header("이번 턴 체력 손실 여부")]
    [SerializeField]
    private bool damagedThisTurn;

    /// <summary>
    /// 현재 방어도
    /// </summary>
    public int CurrentBlock => currentBlock;

    /// <summary>
    /// 이번 턴 체력을 잃었는지 여부
    /// </summary>
    public bool DamagedThisTurn => damagedThisTurn;

    private void Awake()
    {
        if (playerData == null)
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("[PlayerCombat] GameManager.Instance가 없습니다.");
                return;
            }

            playerData = GameManager.Instance.PlayerData;

            if (playerData == null)
            {
                Debug.LogError("[PlayerCombat] GameManager에서 PlayerData를 찾지 못했습니다.");
            }
        }
    }

    /// <summary>
    /// 방어도를 획득합니다.
    /// </summary>
    public void GainBlock(int amount)
    {
        currentBlock += amount;

        Debug.Log($"[PlayerCombat] 방어도 획득 : +{amount} (현재 {currentBlock})");
    }

    /// <summary>
    /// 체력을 잃습니다.
    /// 방어도를 먼저 차감합니다.
    /// </summary>
    public void LoseHealth(int amount)
    {
        damagedThisTurn = true;

        StatusEffectHandler statusEffectHandler = GetComponent<StatusEffectHandler>();

        if (statusEffectHandler != null && statusEffectHandler.HasStatusEffect(StatusEffectType.Vulnerable))
        {
            int increasedDamage = Mathf.FloorToInt(amount * 1.4f);

            Debug.Log($"[PlayerCombat] 취약 적용 : {amount} → {increasedDamage}");

            amount = increasedDamage;
        }

        if (currentBlock > 0)
        {
            int absorbed = Mathf.Min(currentBlock, amount);

            currentBlock -= absorbed;
            amount -= absorbed;
        }

        if (amount > 0)
        {
            playerData.TakeDamage(amount);
        }

        Debug.Log($"[PlayerCombat] 피해 : {amount}");
    }

    /// <summary>
    /// 체력을 회복합니다.
    /// </summary>
    public void Heal(int amount)
    {
        playerData.Heal(amount);

        Debug.Log($"[PlayerCombat] 회복 : {amount}");
    }

    /// <summary>
    /// 방어도를 초기화합니다.
    /// 새 플레이어 턴 시작 시 호출됩니다.
    /// </summary>
    public void ClearBlock()
    {
        currentBlock = 0;

        Debug.Log("[PlayerCombat] 방어도 초기화");
    }

    /// <summary>
    /// 턴 종료 시 호출됩니다.
    /// </summary>
    public void EndTurn()
    {
        damagedThisTurn = false;
    }

    /// <summary>
    /// 전투 종료 시 초기화합니다.
    /// </summary>
    public void ResetCombat()
    {
        currentBlock = 0;
        damagedThisTurn = false;
    }
}