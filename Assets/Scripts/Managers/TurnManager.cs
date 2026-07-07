using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 전투 중 턴 상태와 카드 사용 제한을 관리하는 클래스입니다.
/// 플레이어 턴, 적 턴, 공격/방어 카드 사용 횟수, 다음 턴 드로우를 담당합니다.
/// </summary>
public class TurnManager : MonoBehaviour
{
    [Header("현재 플레이어 턴 여부")]
    [SerializeField]
    private bool isPlayerTurn;

    [Header("Hand Manager")]
    [SerializeField]
    private HandManager handManager;

    [Header("손패 최대 장수")]
    [SerializeField]
    private int maxHandCount = 4;

    [Header("공격/방어 카드 최대 사용 횟수")]
    [SerializeField]
    private int maxAttackDefenseCardUseCount = 2;

    [Header("현재 공격/방어 카드 사용 횟수")]
    [SerializeField]
    private int currentAttackDefenseCardUseCount;

    [Header("공격/방어 카드 사용 횟수 UI")]
    [SerializeField]
    private TextMeshProUGUI attackDefenseUseCountText;

    [Header("적 턴 설정")]
    [SerializeField]
    private float enemyTurnDelay = 0.7f;

    public bool IsPlayerTurn => isPlayerTurn;
    public int CurrentAttackDefenseCardUseCount => currentAttackDefenseCardUseCount;
    public int MaxAttackDefenseCardUseCount => maxAttackDefenseCardUseCount;

    private void Start()
    {
        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        currentAttackDefenseCardUseCount = 0;

        PlayerCombat playerCombat = FindFirstObjectByType<PlayerCombat>();

        if (playerCombat != null)
        {
            playerCombat.ClearBlock();
        }
        else
        {
            Debug.LogWarning("[TurnManager] PlayerCombat을 찾지 못해 방어도를 초기화하지 못했습니다.");
        }

        DrawCardsForNewTurn();
        UpdateAttackDefenseUseCountUI();

        Debug.Log("[TurnManager] 플레이어 턴 시작");
    }

    public void EndPlayerTurnAndStartNextTurn()
    {
        if (!isPlayerTurn)
        {
            Debug.LogWarning("[TurnManager] 현재 플레이어 턴이 아닙니다.");
            return;
        }

        EndPlayerTurn();
        StartCoroutine(EnemyTurnRoutine());
    }

    public void EndPlayerTurn()
    {
        if (!isPlayerTurn)
        {
            Debug.LogWarning("[TurnManager] 현재 플레이어 턴이 아닙니다.");
            return;
        }

        DecreasePlayerStatusDuration();

        isPlayerTurn = false;

        Debug.Log("[TurnManager] 플레이어 턴 종료");
    }

    private IEnumerator EnemyTurnRoutine()
    {
        Debug.Log("[TurnManager] 적 턴 시작");

        yield return new WaitForSeconds(enemyTurnDelay);

        ExecuteEnemyTurn();

        DecreaseEnemyStatusDuration();

        yield return new WaitForSeconds(enemyTurnDelay);

        Debug.Log("[TurnManager] 적 턴 종료");

        StartPlayerTurn();
    }

    private void ExecuteEnemyTurn()
    {
        PlayerCombat playerCombat = FindFirstObjectByType<PlayerCombat>();

        if (playerCombat == null)
        {
            Debug.LogError("[TurnManager] 씬에서 PlayerCombat을 찾지 못했습니다.");
            return;
        }

        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        if (enemies.Length == 0)
        {
            Debug.LogWarning("[TurnManager] 행동할 Enemy가 없습니다.");
            return;
        }

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null)
                continue;

            enemy.TakeTurn(playerCombat);
        }
    }

    private void DrawCardsForNewTurn()
    {
        if (handManager == null)
        {
            Debug.LogError("[TurnManager] HandManager가 연결되지 않았습니다.");
            return;
        }

        int currentHandCount = handManager.HandCards.Count;
        int drawCount = maxHandCount - currentHandCount;

        if (drawCount <= 0)
        {
            Debug.Log("[TurnManager] 손패가 이미 최대 장수입니다.");
            return;
        }

        handManager.DrawCards(drawCount);

        Debug.Log($"[TurnManager] 새 턴 드로우 : {drawCount}장");
    }

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

    private void UpdateAttackDefenseUseCountUI()
    {
        if (attackDefenseUseCountText == null)
        {
            Debug.LogWarning("[TurnManager] 공격/방어 카드 사용 횟수 UI가 연결되지 않았습니다.");
            return;
        }

        attackDefenseUseCountText.text = $"{currentAttackDefenseCardUseCount} / {maxAttackDefenseCardUseCount}";
    }

    private void DecreasePlayerStatusDuration()
    {
        PlayerCombat playerCombat = FindFirstObjectByType<PlayerCombat>();

        if (playerCombat == null)
        {
            Debug.LogWarning("[TurnManager] PlayerCombat을 찾지 못해 플레이어 상태효과 턴을 감소시키지 못했습니다.");
            return;
        }

        StatusEffectHandler statusEffectHandler = playerCombat.GetComponent<StatusEffectHandler>();

        if (statusEffectHandler == null)
        {
            return;
        }

        statusEffectHandler.DecreaseTurnDuration();

        Debug.Log("[TurnManager] 플레이어 상태효과 지속 턴 감소");
    }

    private void DecreaseEnemyStatusDuration()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null)
                continue;

            if (!enemy.gameObject.activeSelf)
                continue;

            StatusEffectHandler statusEffectHandler = enemy.GetComponent<StatusEffectHandler>();

            if (statusEffectHandler == null)
                continue;

            statusEffectHandler.DecreaseTurnDuration();
        }

        Debug.Log("[TurnManager] 적 상태효과 지속 턴 감소");
    }
}