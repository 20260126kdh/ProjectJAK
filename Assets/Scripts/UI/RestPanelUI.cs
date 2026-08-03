using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 휴식 단계에서 표시되는 Rest 패널을 관리합니다.
///
/// 기능:
/// - 최대 체력의 20% 회복
/// - 카드 강화 패널 진입
/// - 휴식 완료 후 보스 전투 시작
/// </summary>
public class RestPanelUI : MonoBehaviour
{
    [Header("Rest Panel")]
    [SerializeField]
    private GameObject restPanel;

    [Header("Battle Manager")]
    [SerializeField]
    private BattleManager battleManager;

    [Header("Upgrade Panel UI")]
    [SerializeField]
    private UpgradePanelUI upgradePanelUI;

    [Header("휴식 버튼")]
    [SerializeField]
    private Button restButton;

    [Header("강화 버튼")]
    [SerializeField]
    private Button upgradeButton;

    [Header("다음 전투 버튼")]
    [SerializeField]
    private Button nextBattleButton;

    [Header("휴식 설정")]
    [Range(0f, 1f)]
    [SerializeField]
    private float healRate = 0.2f;

    [Header("현재 휴식 상태")]
    [SerializeField]
    private bool hasRested;

    [SerializeField]
    private bool hasUpgraded;

    [SerializeField]
    private bool isMovingToNextBattle;

    private void Awake()
    {
        HideRestPanel();
    }

    /// <summary>
    /// 휴식 패널을 표시하고
    /// 이번 휴식 단계의 버튼 상태를 초기화합니다.
    /// </summary>
    public void ShowRestPanel()
    {
        if (restPanel == null)
        {
            Debug.LogError(
                "[RestPanelUI] Rest Panel이 연결되지 않았습니다."
            );

            return;
        }

        hasRested = false;
        hasUpgraded = false;
        isMovingToNextBattle = false;

        if (restButton != null)
        {
            restButton.interactable = true;
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable = true;
        }

        if (nextBattleButton != null)
        {
            nextBattleButton.interactable = true;
        }

        restPanel.SetActive(true);

        Debug.Log("[RestPanelUI] 휴식 패널 표시");
    }

    /// <summary>
    /// 휴식 패널을 숨깁니다.
    /// </summary>
    public void HideRestPanel()
    {
        if (restPanel == null)
        {
            return;
        }

        restPanel.SetActive(false);
    }

    /// <summary>
    /// 강화 패널에서 휴식 패널로 돌아옵니다.
    /// 기존 회복 및 강화 사용 상태는 초기화하지 않습니다.
    /// </summary>
    public void ReturnToRestPanel()
    {
        if (restPanel == null)
        {
            Debug.LogError(
                "[RestPanelUI] Rest Panel이 연결되지 않았습니다."
            );

            return;
        }

        restPanel.SetActive(true);

        if (restButton != null)
        {
            restButton.interactable = !hasRested;
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable = !hasUpgraded;
        }

        if (nextBattleButton != null)
        {
            nextBattleButton.interactable =
                !isMovingToNextBattle;
        }

        Debug.Log("[RestPanelUI] 휴식 패널 복귀");
    }

    /// <summary>
    /// 이번 휴식 단계에서 카드 강화를 완료 처리합니다.
    /// </summary>
    public void CompleteUpgrade()
    {
        hasUpgraded = true;

        if (upgradeButton != null)
        {
            upgradeButton.interactable = false;
        }

        ReturnToRestPanel();

        Debug.Log("[RestPanelUI] 카드 강화 완료");
    }

    /// <summary>
    /// 휴식 버튼에서 호출합니다.
    /// 플레이어 최대 체력의 20%를 회복하고
    /// 휴식 버튼을 비활성화합니다.
    /// </summary>
    public void OnClickRest()
    {
        if (hasRested)
        {
            Debug.LogWarning(
                "[RestPanelUI] 이번 휴식 단계에서 이미 회복했습니다."
            );

            return;
        }

        if (GameManager.Instance == null ||
            GameManager.Instance.PlayerData == null)
        {
            Debug.LogError(
                "[RestPanelUI] PlayerData를 찾지 못했습니다."
            );

            return;
        }

        PlayerData playerData =
            GameManager.Instance.PlayerData;

        int healAmount =
            Mathf.FloorToInt(
                playerData.MaxHP * healRate
            );

        playerData.Heal(healAmount);

        hasRested = true;

        if (restButton != null)
        {
            restButton.interactable = false;
        }

        Debug.Log(
            $"[RestPanelUI] 휴식 완료 / " +
            $"최대 체력의 {healRate * 100f}% 회복 / " +
            $"회복 시도량: {healAmount} / " +
            $"현재 체력: {playerData.CurrentHP}/{playerData.MaxHP}"
        );
    }

    /// <summary>
    /// 강화 패널을 표시하고 휴식 패널을 숨깁니다.
    /// </summary>
    public void OnClickUpgrade()
    {
        if (upgradePanelUI == null)
        {
            Debug.LogError(
                "[RestPanelUI] UpgradePanelUI가 연결되지 않았습니다."
            );

            return;
        }

        HideRestPanel();

        upgradePanelUI.ShowPanel();

        Debug.Log("[RestPanelUI] 카드 강화 패널 열기");
    }

    /// <summary>
    /// 다음 전투 버튼에서 호출합니다.
    /// 휴식 단계를 완료하고 보스 전투를 시작합니다.
    /// </summary>
    public void OnClickNextBattle()
    {
        if (isMovingToNextBattle)
        {
            return;
        }

        if (battleManager == null)
        {
            Debug.LogError(
                "[RestPanelUI] BattleManager가 연결되지 않았습니다."
            );

            return;
        }

        if (StageManager.Instance == null)
        {
            Debug.LogError(
                "[RestPanelUI] StageManager.Instance가 없습니다."
            );

            return;
        }

        if (StageManager.Instance.CurrentPhase !=
            StagePhase.Rest)
        {
            Debug.LogWarning(
                $"[RestPanelUI] 현재 휴식 단계가 아닙니다. " +
                $"현재 단계: {StageManager.Instance.CurrentPhase}"
            );

            return;
        }

        isMovingToNextBattle = true;

        if (restButton != null)
        {
            restButton.interactable = false;
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable = false;
        }

        if (nextBattleButton != null)
        {
            nextBattleButton.interactable = false;
        }

        /*
        * 먼저 StageManager를 보스 전투 단계로 변경해야
        * BattleManager가 보스 전투 데이터를 가져올 수 있습니다.
        */
        StageManager.Instance.RestComplete();

        /*
         * 휴식에서 적용한 회복과 카드 강화 상태,
         * 그리고 BossBattle로 변경된 진행도를 저장합니다.
         *
         * 이어하기 시 휴식 화면이 아니라
         * 보스 전투 시작 지점부터 다시 시작합니다.
         */
        SaveProgressBeforeBossBattle();

        HideRestPanel();

        battleManager.StartNextBattle();

        Debug.Log(
            "[RestPanelUI] 휴식 완료 - 보스 전투 시작"
        );
    }

    /// <summary>
    /// 휴식 완료 후 보스 전투를 시작하기 전에
    /// 현재 플레이어 체력, 강화된 덱, 스테이지 진행도를 저장합니다.
    /// </summary>
    private void SaveProgressBeforeBossBattle()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning(
                "[RestPanelUI] SaveManager.Instance가 없어 " +
                "보스 전투 시작 전 자동 저장을 처리하지 못했습니다."
            );

            return;
        }

        bool saveSucceeded =
            SaveManager.Instance.SaveCurrentGame();

        if (!saveSucceeded)
        {
            Debug.LogWarning(
                "[RestPanelUI] 보스 전투 시작 전 자동 저장에 실패했습니다."
            );

            return;
        }

        Debug.Log(
            "[RestPanelUI] 보스 전투 시작 전 자동 저장 완료"
        );
    }
}