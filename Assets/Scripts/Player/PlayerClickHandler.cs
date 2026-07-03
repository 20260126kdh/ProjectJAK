using UnityEngine;

/// <summary>
/// 플레이어 오브젝트 클릭을 감지하여 BattleManager에 카드 사용을 요청하는 클래스입니다.
/// 클래스 프리팹에 붙여 사용합니다.
/// </summary>
public class PlayerClickHandler : MonoBehaviour
{
    [Header("Battle Manager 태그")]
    [SerializeField]
    private string battleManagerTag = "BattleManager";

    [Header("Player Combat")]
    [SerializeField]
    private PlayerCombat playerCombat;

    private BattleManager battleManager;

    private void Awake()
    {
        if (playerCombat == null)
        {
            playerCombat = GetComponent<PlayerCombat>();
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
            Debug.LogError("[PlayerClickHandler] BattleManager 태그를 가진 오브젝트를 찾지 못했습니다.");
            return;
        }

        battleManager = battleManagerObject.GetComponent<BattleManager>();

        if (battleManager == null)
        {
            Debug.LogError("[PlayerClickHandler] BattleManager 태그 오브젝트에 BattleManager 컴포넌트가 없습니다.");
        }
    }

    /// <summary>
    /// 플레이어 오브젝트를 클릭했을 때 호출됩니다.
    /// </summary>
    private void OnMouseDown()
    {
        if (battleManager == null)
        {
            FindBattleManager();
        }

        if (battleManager == null)
        {
            Debug.LogError("[PlayerClickHandler] BattleManager가 없어 카드 사용을 처리할 수 없습니다.");
            return;
        }

        if (playerCombat == null)
        {
            Debug.LogError("[PlayerClickHandler] PlayerCombat이 연결되지 않았습니다.");
            return;
        }

        battleManager.UseSelectedCardOnPlayer(playerCombat);
    }
}