using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 메인 타이틀 화면을 관리합니다.
///
/// 담당 기능:
/// - 새 게임 시작
/// - 기존 이어하기 저장 데이터 제거
/// - 게임 진행 데이터 초기화
/// - 클래스 선택 씬 이동
/// </summary>
public class TitleManager : MonoBehaviour
{
    [Header("Scene Settings")]

    [Tooltip("클래스 선택 씬 이름입니다.")]
    [SerializeField]
    private string classSelectScene =
        "ClassSelectScene";

    /// <summary>
    /// 새 게임 버튼에서 호출합니다.
    ///
    /// 기존 저장 파일과 이어하기 임시 데이터를 제거한 뒤
    /// 플레이어 데이터를 초기화하고 클래스 선택 씬으로 이동합니다.
    /// </summary>
    public void StartGame()
    {
        if (string.IsNullOrWhiteSpace(
                classSelectScene))
        {
            Debug.LogError(
                "[TitleManager] Class Select Scene 이름이 비어 있습니다."
            );

            return;
        }

        /*
         * 새 게임을 시작하면 기존 이어하기 데이터를
         * 더 이상 사용할 수 없도록 제거합니다.
         */
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ClearCapturedBattleSnapshot();

            bool deleteSucceeded =
                SaveManager.Instance.DeleteSaveData();

            if (!deleteSucceeded)
            {
                Debug.LogError(
                    "[TitleManager] 기존 저장 데이터 삭제에 실패하여 " +
                    "새 게임 시작을 중단합니다."
                );

                return;
            }
        }
        else
        {
            Debug.LogWarning(
                "[TitleManager] SaveManager.Instance가 없어 " +
                "기존 저장 파일을 삭제하지 못했습니다."
            );
        }

        /*
         * 이전 이어하기 과정에서 남아 있을 수 있는
         * 임시 로드 데이터를 제거합니다.
         */
        ContinueLoadContext.Clear();

        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "[TitleManager] GameManager.Instance를 찾지 못했습니다."
            );

            return;
        }

        GameManager.Instance.InitializeGame();

        Debug.Log(
            "[TitleManager] 새 게임 시작 - " +
            "기존 저장 데이터 삭제 및 게임 초기화 완료"
        );

        SceneManager.LoadScene(
            classSelectScene
        );
    }
}