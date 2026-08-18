using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

/// <summary>
/// 게임 클리어 씬에서 엔딩 영상을 한 번 재생하고 메인 타이틀로 복귀합니다.
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public sealed class GameClearController : MonoBehaviour
{
    private const string TitleSceneName = "Main_TitleScene";
    private const string EndingVideoResourcePath = "Video/Ending";

    private VideoClip endingClip;
    private Camera targetCamera;

    private VideoPlayer videoPlayer;
    private bool isReturningToTitle;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
        }

        endingClip = Resources.Load<VideoClip>(
            EndingVideoResourcePath
        );

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        ConfigureVideoPlayer();
    }

    private void Start()
    {
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.StopBGM();
        }

        if (endingClip == null)
        {
            Debug.LogError(
                "[GameClearController] 엔딩 영상이 연결되지 않았습니다."
            );
            StartCoroutine(ReturnToTitleAfterDelay());
            return;
        }

        videoPlayer.Prepare();
    }

    private void OnDestroy()
    {
        if (videoPlayer == null)
        {
            return;
        }

        videoPlayer.prepareCompleted -= OnPrepareCompleted;
        videoPlayer.loopPointReached -= OnEndingFinished;
        videoPlayer.errorReceived -= OnVideoError;
    }

    private void ConfigureVideoPlayer()
    {
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.skipOnDrop = true;
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = endingClip;
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        videoPlayer.targetCamera = targetCamera;
        videoPlayer.targetCameraAlpha = 1f;
        videoPlayer.aspectRatio = VideoAspectRatio.FitOutside;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

        videoPlayer.prepareCompleted += OnPrepareCompleted;
        videoPlayer.loopPointReached += OnEndingFinished;
        videoPlayer.errorReceived += OnVideoError;
    }

    private void OnPrepareCompleted(VideoPlayer preparedPlayer)
    {
        preparedPlayer.Play();
    }

    private void OnEndingFinished(VideoPlayer finishedPlayer)
    {
        ReturnToTitle();
    }

    private void OnVideoError(VideoPlayer failedPlayer, string message)
    {
        Debug.LogError(
            $"[GameClearController] 엔딩 영상 재생 오류: {message}"
        );
        ReturnToTitle();
    }

    private IEnumerator ReturnToTitleAfterDelay()
    {
        yield return new WaitForSecondsRealtime(1f);
        ReturnToTitle();
    }

    private void ReturnToTitle()
    {
        if (isReturningToTitle)
        {
            return;
        }

        isReturningToTitle = true;

        if (ScreenFadeController.IsTransitioning)
        {
            SceneManager.LoadScene(TitleSceneName);
            return;
        }

        ScreenFadeController.LoadTitleScene(TitleSceneName);
    }
}
