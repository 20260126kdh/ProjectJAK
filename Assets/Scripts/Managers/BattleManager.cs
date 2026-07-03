using UnityEngine;

/// <summary>
/// 전투 전체 흐름을 관리하는 클래스입니다.
/// 현재는 전투 시작 상태, 선택된 카드 정보, 카드 효과 실행을 관리합니다.
/// </summary>
public class BattleManager : MonoBehaviour
{
    [Header("현재 전투 상태")]
    [SerializeField]
    private bool isBattleStarted;

    [Header("Hand Manager")]
    [SerializeField]
    private HandManager handManager;

    [Header("Turn Manager")]
    [SerializeField]
    private TurnManager turnManager;

    [Header("Card Effect Executor")]
    [SerializeField]
    private CardEffectExecutor cardEffectExecutor;

    [Header("적 태그")]
    [SerializeField]
    private string enemyTag = "Enemy";

    [Header("현재 선택된 카드 데이터")]
    [SerializeField]
    private CardData selectedCardData;

    private CardUI selectedCardUI;

    public CardData SelectedCardData => selectedCardData;
    public CardUI SelectedCardUI => selectedCardUI;

    private void Start()
    {
        StartBattle();
    }

    /// <summary>
    /// 전투 시작 처리를 수행합니다.
    /// </summary>
    public void StartBattle()
    {
        isBattleStarted = true;
        selectedCardData = null;
        selectedCardUI = null;

        Debug.Log("[BattleManager] 전투 시작");
    }

    /// <summary>
    /// 선택된 카드를 변경합니다.
    /// 같은 카드를 다시 선택하면 선택 해제합니다.
    /// </summary>
    public void SelectCard(CardUI cardUI)
    {
        if (!isBattleStarted)
        {
            Debug.LogWarning("[BattleManager] 아직 전투가 시작되지 않았습니다.");
            return;
        }

        if (cardUI == null)
        {
            Debug.LogWarning("[BattleManager] 선택하려는 CardUI가 비어 있습니다.");
            return;
        }

        if (selectedCardUI == cardUI)
        {
            selectedCardUI.SetDeselected();

            selectedCardUI = null;
            selectedCardData = null;

            Debug.Log("[BattleManager] 카드 선택 해제");
            return;
        }

        if (selectedCardUI != null)
        {
            selectedCardUI.SetDeselected();
        }

        selectedCardUI = cardUI;
        selectedCardData = cardUI.GetCardData();

        selectedCardUI.SetSelected();

        Debug.Log($"[BattleManager] 카드 선택 : {selectedCardData.cardName}");
    }

    /// <summary>
    /// 현재 선택된 카드를 지정한 적에게 사용합니다.
    /// </summary>
    public void UseSelectedCardOnEnemy(Enemy targetEnemy)
    {
        if (!CanUseSelectedCard())
            return;

        if (targetEnemy == null)
        {
            Debug.LogWarning("[BattleManager] 대상 Enemy가 비어 있습니다.");
            return;
        }

        if (cardEffectExecutor == null)
        {
            Debug.LogError("[BattleManager] CardEffectExecutor가 연결되지 않았습니다.");
            return;
        }

        Debug.Log($"[BattleManager] 적에게 카드 사용 : {selectedCardData.cardName}");

        cardEffectExecutor.ExecuteEffects(selectedCardData, targetEnemy);

        FinishCardUse();
    }

    /// <summary>
    /// 현재 선택된 카드를 플레이어 자신에게 사용합니다.
    /// Self 대상 카드 처리를 위해 사용합니다.
    /// </summary>
    public void UseSelectedCardOnPlayer(PlayerCombat targetPlayer)
    {
        if (!CanUseSelectedCard())
            return;

        if (targetPlayer == null)
        {
            Debug.LogWarning("[BattleManager] 대상 PlayerCombat이 비어 있습니다.");
            return;
        }

        if (cardEffectExecutor == null)
        {
            Debug.LogError("[BattleManager] CardEffectExecutor가 연결되지 않았습니다.");
            return;
        }

        Debug.Log($"[BattleManager] 플레이어에게 카드 사용 : {selectedCardData.cardName}");

        cardEffectExecutor.ExecuteEffects(selectedCardData, null);

        FinishCardUse();
    }

    /// <summary>
    /// 선택된 카드를 사용할 수 있는지 확인합니다.
    /// 전투 시작 여부, 카드 선택 여부, 턴 상태, 카드 사용 제한을 검사합니다.
    /// </summary>
    private bool CanUseSelectedCard()
    {
        if (!isBattleStarted)
        {
            Debug.LogWarning("[BattleManager] 아직 전투가 시작되지 않았습니다.");
            return false;
        }

        if (selectedCardData == null)
        {
            Debug.LogWarning("[BattleManager] 사용할 카드가 선택되지 않았습니다.");
            return false;
        }

        if (turnManager == null)
        {
            Debug.LogError("[BattleManager] TurnManager가 연결되지 않았습니다.");
            return false;
        }

        return turnManager.CanUseCard(selectedCardData);
    }

    /// <summary>
    /// 카드 사용 성공 후 공통 처리를 수행합니다.
    /// 사용 횟수 기록, 손패 제거, 버림 더미 이동, 선택 해제를 처리합니다.
    /// </summary>
    private void FinishCardUse()
    {
        if (turnManager != null)
        {
            turnManager.RecordCardUse(selectedCardData);
        }

        if (handManager != null)
        {
            handManager.DiscardUsedCard(selectedCardData);
        }
        else
        {
            Debug.LogError("[BattleManager] HandManager가 연결되지 않았습니다.");
        }

        ClearSelectedCard();
    }

    /// <summary>
    /// 현재 선택된 카드를 강제로 해제합니다.
    /// </summary>
    public void ClearSelectedCard()
    {
        if (selectedCardUI != null)
        {
            selectedCardUI.SetDeselected();
        }

        selectedCardUI = null;
        selectedCardData = null;

        Debug.Log("[BattleManager] 선택 카드 초기화");
    }

    /// <summary>
    /// Enemy 태그를 가진 오브젝트를 찾아 Enemy 컴포넌트를 반환합니다.
    /// </summary>
    private Enemy FindTargetEnemyByTag()
    {
        GameObject enemyObject = GameObject.FindGameObjectWithTag(enemyTag);

        if (enemyObject == null)
        {
            return null;
        }

        Enemy enemy = enemyObject.GetComponent<Enemy>();

        if (enemy == null)
        {
            Debug.LogWarning("[BattleManager] Enemy 태그 오브젝트에 Enemy 컴포넌트가 없습니다.");
            return null;
        }

        return enemy;
    }
}