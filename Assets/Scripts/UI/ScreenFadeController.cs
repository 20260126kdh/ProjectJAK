using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 씬 이동과 전투 화면 교체에 사용하는 전역 화면 페이드를 관리합니다.
/// </summary>
public sealed class ScreenFadeController : MonoBehaviour
{
    private const float InitialFadeOutDuration = 0.64f;
    private const int InitialFadeInFrames = 3;
    private const float BattleFadeOutDuration = 0.32f;
    private const float BattleFadeInDuration = 0.24f;
    private const float TitleFadeOutDuration = 0.15f;
    private const float TitleFadeInDuration = 1.56f;

    private static ScreenFadeController instance;

    private CanvasGroup fadeGroup;
    private bool isTransitioning;

    /// <summary>
    /// 현재 화면 전환이 진행 중인지 반환합니다.
    /// </summary>
    public static bool IsTransitioning =>
        instance != null && instance.isTransitioning;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeAfterFirstSceneLoad()
    {
        ScreenFadeController controller = GetOrCreate();

        if (!controller.isTransitioning)
        {
            controller.StartCoroutine(controller.PlayInitialFade());
        }
    }

    /// <summary>
    /// 클래스 선택 화면에서 전투 씬으로 이동하는 페이드를 시작합니다.
    /// </summary>
    /// <param name="sceneName">이동할 전투 씬 이름</param>
    public static void LoadBattleScene(string sceneName)
    {
        BeginSceneTransition(
            sceneName,
            BattleFadeOutDuration,
            BattleFadeInDuration
        );
    }

    /// <summary>
    /// 현재 씬에서 다음 전투 화면으로 교체하는 페이드를 시작합니다.
    /// </summary>
    /// <param name="changeBattle">검은 화면에서 실행할 전투 교체 처리</param>
    public static void ChangeBattle(Action changeBattle)
    {
        ScreenFadeController controller = GetOrCreate();

        if (controller.isTransitioning)
        {
            return;
        }

        controller.StartCoroutine(
            controller.PlayActionTransition(
                changeBattle,
                BattleFadeOutDuration,
                BattleFadeInDuration
            )
        );
    }

    /// <summary>
    /// 현재 화면에서 타이틀 씬으로 이동하는 페이드를 시작합니다.
    /// </summary>
    /// <param name="sceneName">이동할 타이틀 씬 이름</param>
    public static void LoadTitleScene(string sceneName)
    {
        BeginSceneTransition(
            sceneName,
            TitleFadeOutDuration,
            TitleFadeInDuration
        );
    }

    private static void BeginSceneTransition(
        string sceneName,
        float fadeOutDuration,
        float fadeInDuration)
    {
        ScreenFadeController controller = GetOrCreate();

        if (controller.isTransitioning)
        {
            return;
        }

        controller.StartCoroutine(
            controller.PlaySceneTransition(
                sceneName,
                fadeOutDuration,
                fadeInDuration
            )
        );
    }

    private static ScreenFadeController GetOrCreate()
    {
        if (instance != null)
        {
            return instance;
        }

        GameObject root = new GameObject(
            "ScreenFadeController",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(ScreenFadeController)
        );

        DontDestroyOnLoad(root);
        instance = root.GetComponent<ScreenFadeController>();
        instance.CreateOverlay(root.GetComponent<Canvas>());

        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void CreateOverlay(Canvas canvas)
    {
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = short.MaxValue;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject overlay = new GameObject(
            "FadeOverlay",
            typeof(RectTransform),
            typeof(CanvasGroup),
            typeof(Image)
        );
        overlay.transform.SetParent(transform, false);

        RectTransform rectTransform = overlay.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image image = overlay.GetComponent<Image>();
        image.color = Color.black;

        fadeGroup = overlay.GetComponent<CanvasGroup>();
        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false;
        fadeGroup.interactable = false;
    }

    private IEnumerator PlayInitialFade()
    {
        isTransitioning = true;
        SetInputBlock(true);

        yield return null;
        yield return FadeTo(1f, InitialFadeOutDuration);

        for (int frame = InitialFadeInFrames; frame > 0; frame--)
        {
            fadeGroup.alpha = (float)(frame - 1) / InitialFadeInFrames;
            yield return null;
        }

        FinishTransition();
    }

    private IEnumerator PlaySceneTransition(
        string sceneName,
        float fadeOutDuration,
        float fadeInDuration)
    {
        isTransitioning = true;
        SetInputBlock(true);

        yield return FadeTo(1f, fadeOutDuration);
        SceneManager.LoadScene(sceneName);
        yield return null;
        yield return FadeTo(0f, fadeInDuration);

        FinishTransition();
    }

    private IEnumerator PlayActionTransition(
        Action action,
        float fadeOutDuration,
        float fadeInDuration)
    {
        isTransitioning = true;
        SetInputBlock(true);

        yield return FadeTo(1f, fadeOutDuration);

        try
        {
            action?.Invoke();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }

        yield return null;
        yield return FadeTo(0f, fadeInDuration);

        FinishTransition();
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = fadeGroup.alpha;
        float elapsed = 0f;

        if (duration <= 0f)
        {
            fadeGroup.alpha = targetAlpha;
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                Mathf.Clamp01(elapsed / duration)
            );
            yield return null;
        }

        fadeGroup.alpha = targetAlpha;
    }

    private void SetInputBlock(bool shouldBlock)
    {
        fadeGroup.blocksRaycasts = shouldBlock;
        fadeGroup.interactable = shouldBlock;
    }

    private void FinishTransition()
    {
        fadeGroup.alpha = 0f;
        SetInputBlock(false);
        isTransitioning = false;
    }
}
