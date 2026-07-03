using TMPro;
using UnityEngine;

/// <summary>
/// 전투 중 턴 상태와 카드 사용 제한을 관리하는 클래스입니다.
/// 플레이어 턴 여부, 공격/방어 카드 사용 횟수를 담당합니다.
/// </summary>
public class TurnManager : MonoBehaviour
{
    [Header("현재 플레이어 턴 여부")]
    [SerializeField]
    private bool isPlayerTurn;

    [Header("공격/방어 카드 최대 사용 횟수")]
    [SerializeField]
    private int maxAttackDefenseCardUseCount = 2;

    [Header("현재 공격/방어 카드 사용 횟수")]
    [SerializeField]
    private int currentAttackDefenseCardUseCount;

    [Header("공격/방어 카드 사용 횟수 UI")]
    [SerializeField]
    private TextMeshProUGUI attackDefenseUseCountText;

    /// <summary>
    /// 현재 플레이어 턴인지 반환합니다.
    /// </summary>
    public bool IsPlayerTurn => isPlayerTurn;

    /// <summary>
    /// 현재 공격/방어 카드 사용 횟수를 반환합니다.
    /// </summary>
    public int CurrentAttackDefenseCardUseCount => currentAttackDefenseCardUseCount;

    /// <summary>
    /// 공격/방어 카드 최대 사용 횟수를 반환합니다.
    /// </summary>
    public int MaxAttackDefenseCardUseCount => maxAttackDefenseCardUseCount;

    private void Start()
    {
        StartPlayerTurn();
    }

    /// <summary>
    /// 플레이어 턴을 시작합니다.
    /// 공격/방어 카드 사용 횟수를 초기화합니다.
    /// </summary>
    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        currentAttackDefenseCardUseCount = 0;

        UpdateAttackDefenseUseCountUI();

        Debug.Log("[TurnManager] 플레이어 턴 시작");
    }

    /// <summary>
    /// 플레이어 턴을 종료합니다.
    /// </summary>
    public void EndPlayerTurn()
    {
        if (!isPlayerTurn)
        {
            Debug.LogWarning("[TurnManager] 현재 플레이어 턴이 아닙니다.");
            return;
        }

        isPlayerTurn = false;

        Debug.Log("[TurnManager] 플레이어 턴 종료");
    }

    /// <summary>
    /// 해당 카드를 현재 턴에 사용할 수 있는지 확인합니다.
    /// Skill 카드는 사용 횟수 제한을 받지 않습니다.
    /// Attack, Defense 카드는 합산하여 제한을 받습니다.
    /// </summary>
    public bool CanUseCard(CardData cardData)
    {
        if (!isPlayerTurn)
        {
            Debug.LogWarning("[TurnManager] 플레이어 턴이 아니므로 카드를 사용할 수 없습니다.");
            return false;
        }

        if (cardData == null)
        {
            Debug.LogWarning("[TurnManager] 확인할 카드 데이터가 없습니다.");
            return false;
        }

        if (cardData.cardType == CardType.Skill)
        {
            return true;
        }

        if (cardData.cardType == CardType.Attack || cardData.cardType == CardType.Defense)
        {
            if (currentAttackDefenseCardUseCount >= maxAttackDefenseCardUseCount)
            {
                Debug.LogWarning("[TurnManager] 공격/방어 카드는 한 턴에 최대 2장까지만 사용할 수 있습니다.");
                return false;
            }

            return true;
        }

        Debug.LogWarning($"[TurnManager] 알 수 없는 카드 타입입니다 : {cardData.cardType}");
        return false;
    }

    /// <summary>
    /// 카드 사용 성공 후 사용 횟수를 기록합니다.
    /// Skill 카드는 사용 횟수에 포함하지 않습니다.
    /// </summary>
    public void RecordCardUse(CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogWarning("[TurnManager] 사용 기록할 카드 데이터가 없습니다.");
            return;
        }

        if (cardData.cardType == CardType.Attack || cardData.cardType == CardType.Defense)
        {
            currentAttackDefenseCardUseCount++;

            UpdateAttackDefenseUseCountUI();

            Debug.Log($"[TurnManager] 공격/방어 카드 사용 횟수 : {currentAttackDefenseCardUseCount}/{maxAttackDefenseCardUseCount}");
        }
    }

    /// <summary>
    /// 공격/방어 카드 사용 횟수 UI를 갱신합니다.
    /// </summary>
    private void UpdateAttackDefenseUseCountUI()
    {
        if (attackDefenseUseCountText == null)
        {
            Debug.LogWarning("[TurnManager] 공격/방어 카드 사용 횟수 UI가 연결되지 않았습니다.");
            return;
        }

        attackDefenseUseCountText.text = $"{currentAttackDefenseCardUseCount} / {maxAttackDefenseCardUseCount}";
    }
}