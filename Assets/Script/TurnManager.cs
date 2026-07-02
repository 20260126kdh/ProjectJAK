using System.Collections;
using UnityEngine;

public enum Turn { Player, Enemy }

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }
    public Turn CurrentTurn { get; private set; } = Turn.Player;

    [SerializeField] TurnBanner banner;
    [SerializeField] GameObject turnOverButton;     // Turn Over 버튼 오브젝트
    [SerializeField] float enemyTurnDuration = 2.5f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartPlayerTurn();
    }

    [SerializeField] CardDrawer cardDrawer;   // 상단 필드에 추가

    public void StartPlayerTurn()
    {
        CurrentTurn = Turn.Player;
        Debug.Log("플레이어 턴 시작");
        banner.ShowPlayer();
        turnOverButton.SetActive(true);
        cardDrawer.DrawHand();   // 카드 드로우 시작
    }

    public void EndPlayerTurn()
    {
        if (CurrentTurn != Turn.Player) return;

        CurrentTurn = Turn.Enemy;
        Debug.Log("적 턴으로 전환");
        turnOverButton.SetActive(false);

        cardDrawer.DiscardHand();   // 보존 카드 빼고 버림패로

        StartCoroutine(EnemyTurnRoutine());
    }

    IEnumerator EnemyTurnRoutine()
    {
        Debug.Log("적 턴 시작");
        banner.ShowEnemy();

        yield return new WaitForSeconds(enemyTurnDuration);

        Debug.Log("적 턴 종료");
        StartPlayerTurn();
    }
}