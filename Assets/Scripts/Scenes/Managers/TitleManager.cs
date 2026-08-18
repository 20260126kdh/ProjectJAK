using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

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
    private const string TitleVideoResourcePath =
        "Video/Main_Title";

    [Header("Scene Settings")]

    [Tooltip("클래스 선택 씬 이름입니다.")]
    [SerializeField]
    private string classSelectScene =
        "ClassSelectScene";

    private VideoPlayer titleVideoPlayer;

    private void Awake()
    {
        PlayTitleBackgroundVideo();
    }

    /// <summary>
    /// 타이틀 영상을 기존 UI 뒤의 카메라 배경으로 반복 재생합니다.
    /// 영상 오디오는 사용하지 않고 기존 타이틀 BGM을 유지합니다.
    /// </summary>
    private void PlayTitleBackgroundVideo()
    {
        VideoClip titleVideoClip =
            Resources.Load<VideoClip>(
                TitleVideoResourcePath
            );

        if (titleVideoClip == null)
        {
            Debug.LogError(
                $"[TitleManager] 타이틀 영상을 찾지 못했습니다: " +
                $"Resources/{TitleVideoResourcePath}"
            );

            return;
        }

        Camera titleCamera = Camera.main;

        if (titleCamera == null)
        {
            Debug.LogError(
                "[TitleManager] 타이틀 영상을 출력할 Main Camera가 없습니다."
            );

            return;
        }

        titleVideoPlayer =
            GetComponent<VideoPlayer>();

        if (titleVideoPlayer == null)
        {
            titleVideoPlayer =
                gameObject.AddComponent<VideoPlayer>();
        }

        titleVideoPlayer.playOnAwake = false;
        titleVideoPlayer.source = VideoSource.VideoClip;
        titleVideoPlayer.clip = titleVideoClip;
        titleVideoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
        titleVideoPlayer.targetCamera = titleCamera;
        titleVideoPlayer.targetCameraAlpha = 1f;
        titleVideoPlayer.aspectRatio = VideoAspectRatio.FitOutside;
        titleVideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        titleVideoPlayer.isLooping = true;
        titleVideoPlayer.skipOnDrop = true;
        titleVideoPlayer.waitForFirstFrame = true;
        titleVideoPlayer.Play();
    }

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

        /*
         * 저장 후 종료로 타이틀에 돌아온 경우에도 StageManager는
         * DontDestroyOnLoad로 이전 진행 상태를 유지합니다.
         * 새 게임에서는 저장 파일뿐 아니라 런타임 진행도도 초기화해야 합니다.
         */
        if (StageManager.Instance == null)
        {
            Debug.LogError(
                "[TitleManager] StageManager.Instance를 찾지 못해 " +
                "새 게임 시작을 중단합니다."
            );

            return;
        }

        StageManager.Instance.ResetProgress();

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
