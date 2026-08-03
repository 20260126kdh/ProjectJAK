using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// TitleScene의 이어하기 버튼 상태와
/// 저장 데이터 불러오기를 관리합니다.
///
/// 담당 기능:
/// - 유효한 저장 데이터 존재 여부 확인
/// - 이어하기 버튼 활성화 및 비활성화
/// - 플레이어 데이터 복원
/// - 스테이지 진행도 복원
/// - 덱 복원용 데이터 임시 보관
/// - BattleScene 이동
/// </summary>
public class TitleContinueController : MonoBehaviour
{
    [Header("이어하기 버튼")]
    [SerializeField]
    private Button continueButton;

    [Header("Scene 설정")]
    [Tooltip("이어하기 후 이동할 전투 씬 이름입니다.")]
    [SerializeField]
    private string battleSceneName =
        "BattleScene";

    [Header("Debug")]
    [SerializeField]
    private bool debugMode = true;

    /// <summary>
    /// 현재 이어하기 가능한 저장 데이터가 있는지 반환합니다.
    /// </summary>
    public bool CanContinue =>
        SaveManager.Instance != null &&
        SaveManager.Instance.HasValidSaveData;

    /// <summary>
    /// 시작 시 이어하기 버튼 상태를 갱신합니다.
    /// </summary>
    private void Start()
    {
        RefreshContinueButton();
    }

    /// <summary>
    /// 현재 저장 데이터 존재 여부에 따라
    /// 이어하기 버튼 오브젝트의 표시 상태를 갱신합니다.
    ///
    /// 저장 데이터 있음:
    /// - 이어하기 버튼 표시
    /// - 버튼 입력 활성화
    ///
    /// 저장 데이터 없음:
    /// - 이어하기 버튼 오브젝트 비활성화
    /// </summary>
    public void RefreshContinueButton()
    {
        bool hasValidSaveData =
            CanContinue;

        if (continueButton == null)
        {
            Debug.LogWarning(
                "[TitleContinueController] " +
                "ContinueButton이 연결되지 않았습니다."
            );

            return;
        }

        /*
         * 저장 데이터가 없으면 버튼을 비활성화하는 것이 아니라
         * 버튼 오브젝트 자체를 숨깁니다.
         *
         * ButtonRoot의 Vertical Layout Group을 사용 중이라면
         * 숨겨진 버튼의 자리도 자동으로 제거됩니다.
         */
        continueButton.gameObject.SetActive(
            hasValidSaveData
        );

        /*
         * 다시 활성화될 때 클릭 가능한 상태를 보장합니다.
         */
        if (hasValidSaveData)
        {
            continueButton.interactable = true;
        }

        if (debugMode)
        {
            Debug.Log(
                $"[TitleContinueController] " +
                $"이어하기 버튼 표시 상태 갱신: " +
                $"{hasValidSaveData}"
            );
        }
    }

    /// <summary>
    /// 저장 데이터를 불러오고 BattleScene으로 이동합니다.
    ///
    /// 이어하기 버튼에서 호출합니다.
    /// </summary>
    public void ContinueGame()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError(
                "[TitleContinueController] " +
                "SaveManager.Instance를 찾지 못했습니다."
            );

            RefreshContinueButton();
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "[TitleContinueController] " +
                "GameManager.Instance를 찾지 못했습니다."
            );

            return;
        }

        if (GameManager.Instance.PlayerData == null)
        {
            Debug.LogError(
                "[TitleContinueController] " +
                "PlayerData를 찾지 못했습니다."
            );

            return;
        }

        if (StageManager.Instance == null)
        {
            Debug.LogError(
                "[TitleContinueController] " +
                "StageManager.Instance를 찾지 못했습니다."
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(
                battleSceneName))
        {
            Debug.LogError(
                "[TitleContinueController] " +
                "Battle Scene Name이 비어 있습니다."
            );

            return;
        }

        GameSaveData saveData =
            SaveManager.Instance.LoadSaveData();

        if (saveData == null)
        {
            Debug.LogWarning(
                "[TitleContinueController] " +
                "유효한 저장 데이터를 불러오지 못했습니다."
            );

            RefreshContinueButton();
            return;
        }

        PlayerClass savedClass =
            (PlayerClass)saveData.playerClass;

        StagePhase savedPhase =
            (StagePhase)saveData.currentPhase;

        /*
         * 플레이어의 클래스와 체력을 복원합니다.
         */
        GameManager.Instance.PlayerData.SetClass(
            savedClass
        );

        GameManager.Instance.PlayerData.RestoreHP(
            saveData.currentHP,
            saveData.maxHP
        );

        /*
         * 스테이지 진행도를 복원합니다.
         *
         * 실패하면 BattleScene으로 이동하지 않습니다.
         */
        bool stageRestoreSucceeded =
            StageManager.Instance.RestoreProgress(
                saveData.currentStage,
                saveData.currentBattleCount,
                savedPhase,
                saveData.currentBossSequence,
                saveData.isGameClear
            );

        if (!stageRestoreSucceeded)
        {
            Debug.LogError(
                "[TitleContinueController] " +
                "스테이지 진행도 복원에 실패했습니다."
            );

            return;
        }

        /*
         * 덱 복원은 BattleScene의 DeckManager가 담당합니다.
         * 씬 전환 사이에 사용할 데이터를 임시 저장합니다.
         */
        bool contextSetSucceeded =
            ContinueLoadContext.SetPendingSaveData(
                saveData
            );

        if (!contextSetSucceeded)
        {
            Debug.LogError(
                "[TitleContinueController] " +
                "이어하기 데이터 임시 저장에 실패했습니다."
            );

            return;
        }

        if (debugMode)
        {
            Debug.Log(
                $"[TitleContinueController] 이어하기 시작 / " +
                $"Class: {savedClass} / " +
                $"HP: {saveData.currentHP}/{saveData.maxHP} / " +
                $"Stage: {saveData.currentStage} / " +
                $"Battle: {saveData.currentBattleCount} / " +
                $"Phase: {savedPhase} / " +
                $"Cards: {saveData.cards.Count}"
            );
        }

        SceneManager.LoadScene(
            battleSceneName
        );
    }

#if UNITY_EDITOR

    /// <summary>
    /// Inspector 참조가 비어 있으면
    /// 현재 씬에서 ContinueButton을 자동으로 찾습니다.
    /// </summary>
    private void OnValidate()
    {
        if (continueButton != null)
        {
            return;
        }

        Button[] buttons =
            FindObjectsByType<Button>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        for (int i = 0;
             i < buttons.Length;
             i++)
        {
            Button button =
                buttons[i];

            if (button == null)
            {
                continue;
            }

            if (button.gameObject.name ==
                "ContinueButton")
            {
                continueButton =
                    button;

                break;
            }
        }
    }

#endif
}