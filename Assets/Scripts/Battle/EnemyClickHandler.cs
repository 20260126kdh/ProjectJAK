using UnityEngine;

/// <summary>
/// 적 오브젝트 클릭을 감지하여 BattleManager에 카드 사용을 요청하는 클래스입니다.
/// 프리팹에서도 사용할 수 있도록 BattleManager는 실행 중 태그로 찾습니다.
/// </summary>
public class EnemyClickHandler : MonoBehaviour
{
    [Header("Battle Manager 태그")]
    [SerializeField]
    private string battleManagerTag = "BattleManager";

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

    /// <summary>
    /// BattleManager 태그를 가진 오브젝트를 찾아 저장합니다.
    /// </summary>
    private void FindBattleManager()
    {
        GameObject battleManagerObject = GameObject.FindGameObjectWithTag(battleManagerTag);

        if (battleManagerObject == null)
        {
            Debug.LogError("[EnemyClickHandler] BattleManager 태그를 가진 오브젝트를 찾지 못했습니다.");
            return;
        }

        battleManager = battleManagerObject.GetComponent<BattleManager>();

        if (battleManager == null)
        {
            Debug.LogError("[EnemyClickHandler] BattleManager 태그 오브젝트에 BattleManager 컴포넌트가 없습니다.");
        }
    }

    /// <summary>
    /// 적 오브젝트를 클릭했을 때 호출됩니다.
    /// </summary>
    private void OnMouseDown()
    {
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