using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 전투 종료 후 표시되는 리워드 패널을 관리합니다.
/// 현재 단계에서는 Continue 버튼으로 BattleScene을 다시 로드합니다.
/// </summary>
public class RewardPanelUI : MonoBehaviour
{
    [Header("리워드 패널")]
    [SerializeField]
    private GameObject rewardPanel;

    [Header("다시 불러올 전투 씬 이름")]
    [SerializeField]
    private string battleSceneName = "BattleScene";

    private void Awake()
    {
        HideRewardPanel();
    }

    public void ShowRewardPanel()
    {
        if (rewardPanel == null)
        {
            Debug.LogError("[RewardPanelUI] Reward Panel이 연결되지 않았습니다.");
            return;
        }

        rewardPanel.SetActive(true);

        Debug.Log("[RewardPanelUI] 리워드 패널 표시");
    }

    public void HideRewardPanel()
    {
        if (rewardPanel == null)
        {
            return;
        }

        rewardPanel.SetActive(false);
    }

    /// <summary>
    /// Continue 버튼에서 호출합니다.
    /// 현재는 다음 전투를 위해 BattleScene을 다시 로드합니다.
    /// </summary>
    public void OnClickContinue()
    {
        Debug.Log("[RewardPanelUI] Next 클릭 - 다음 전투 로드");

        SceneManager.LoadScene(battleSceneName);
    }
}