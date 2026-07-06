using UnityEngine;

/// <summary>
/// 적 오브젝트 클릭을 감지하여 BattleManager에 카드 사용을 요청하는 클래스입니다.
/// 프리팹에서도 사용할 수 있도록 실행 중 BattleManager를 자동으로 찾습니다.
/// </summary>
public class EnemyClickHandler : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField]
    private Enemy enemy;

    private BattleManager battleManager;

    private void Awake()
    {
        if (enemy == null)
        {
            enemy = GetComponent<Enemy>();
        }
    }

    private void Start()
    {
        FindBattleManager();
    }

    private void FindBattleManager()
    {
        battleManager = FindFirstObjectByType<BattleManager>();

        if (battleManager == null)
        {
            Debug.LogError("[EnemyClickHandler] 씬에서 BattleManager를 찾지 못했습니다.");
        }
    }

    private void OnMouseDown()
    {
        Debug.Log("[EnemyClickHandler] Enemy 클릭 감지");

        if (battleManager == null)
        {
            FindBattleManager();
        }

        if (battleManager == null)
        {
            Debug.LogError("[EnemyClickHandler] BattleManager가 없어 카드 사용을 처리할 수 없습니다.");
            return;
        }

        if (enemy == null)
        {
            Debug.LogError("[EnemyClickHandler] Enemy가 연결되지 않았습니다.");
            return;
        }

        battleManager.UseSelectedCardOnEnemy(enemy);
    }
}