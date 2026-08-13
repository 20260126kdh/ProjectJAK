using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 플레이어 사망 시 작살 이동, 지퍼형 적색 전환과
/// 사망 UI 표시를 순서대로 처리합니다.
/// </summary>
public sealed class PlayerDeathTransitionController : MonoBehaviour
{
    private const string TitleSceneName = "Main_TitleScene";
    private static bool isPlaying;

    private DeathZipperGraphic zipperGraphic;
    private RectTransform harpoonRect;
    private GameObject deathPanel;
    private TMP_FontAsset interfaceFont;
    private float previousTimeScale = 1f;
    private bool isReturningToTitle;

    /// <summary>
    /// 현재 지퍼 메시가 작살 통과 시간을 계산할 때 사용하는 이동 시간입니다.
    /// </summary>
    public static float ActiveTravelDuration { get; private set; } = 0.55f;

    /// <summary>
    /// 사망 전환이 실행 중인지 반환합니다.
    /// </summary>
    public static bool IsPlaying => isPlaying;

    /// <summary>
    /// 사망 전환 전용 Canvas를 생성하고 연출을 시작합니다.
    /// 중복 호출은 무시합니다.
    /// </summary>
    public static void Play(PlayerDeathTransitionSettings settings)
    {
        if (isPlaying)
        {
            return;
        }

        GameObject root = new GameObject(
            "PlayerDeathTransition",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(PlayerDeathTransitionController)
        );

        PlayerDeathTransitionController controller =
            root.GetComponent<PlayerDeathTransitionController>();
        controller.BuildAndPlay(settings);
    }

    private void BuildAndPlay(PlayerDeathTransitionSettings settings)
    {
        isPlaying = true;
        ActiveTravelDuration = Mathf.Max(0.01f, settings.harpoonTravelDuration);
        previousTimeScale = Time.timeScale;
        interfaceFont = FindInterfaceFont();
        Time.timeScale = 0f;

        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = short.MaxValue - 1;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        zipperGraphic = CreateStretchGraphic<DeathZipperGraphic>(
            "DeathRedOpening",
            transform
        );
        zipperGraphic.color = settings.redColor;
        zipperGraphic.raycastTarget = true;

        CreateHarpoon(settings);
        CreateDeathPanel(settings);
        StartCoroutine(PlaySequence(settings));
    }

    private void CreateHarpoon(PlayerDeathTransitionSettings settings)
    {
        GameObject harpoon = new GameObject(
            "DeathHarpoon",
            typeof(RectTransform),
            typeof(Image)
        );
        harpoon.transform.SetParent(transform, false);
        harpoonRect = harpoon.GetComponent<RectTransform>();
        harpoonRect.anchorMin = new Vector2(0f, 0.5f);
        harpoonRect.anchorMax = new Vector2(0f, 0.5f);
        harpoonRect.pivot = new Vector2(0.5f, 0.5f);
        harpoonRect.sizeDelta = settings.harpoonSize;
        harpoonRect.anchoredPosition =
            new Vector2(-settings.harpoonScreenMargin, settings.harpoonHeight);

        Image image = harpoon.GetComponent<Image>();
        image.sprite = settings.harpoonSprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        image.color = settings.harpoonSprite != null
            ? Color.white
            : new Color(1f, 0.75f, 0.32f, 1f);

        if (settings.harpoonSprite == null)
        {
            harpoonRect.sizeDelta = new Vector2(180f, 18f);
        }
    }

    private void CreateDeathPanel(PlayerDeathTransitionSettings settings)
    {
        deathPanel = new GameObject(
            "DeathUI",
            typeof(RectTransform),
            typeof(CanvasGroup),
            typeof(Image)
        );
        deathPanel.transform.SetParent(transform, false);
        RectTransform panelRect = deathPanel.GetComponent<RectTransform>();
        Stretch(panelRect);

        Image background = deathPanel.GetComponent<Image>();
        background.sprite = settings.deathUiSprite;
        background.preserveAspect = true;
        background.color = settings.deathUiSprite != null
            ? Color.white
            : settings.redColor;

        if (settings.deathUiSprite == null)
        {
            CreateText(
                "DeathTitle",
                panelRect,
                "당신은 죽었습니다.",
                72f,
                new Vector2(0f, 120f),
                interfaceFont
            );

            int stage = StageManager.Instance != null
                ? StageManager.Instance.CurrentStage
                : 1;
            CreateText(
                "FinalStage",
                panelRect,
                $"최종 진행 스테이지  {stage}",
                38f,
                new Vector2(0f, 15f),
                interfaceFont
            );

            Button titleButton = CreateTitleButton(
                panelRect,
                interfaceFont
            );
            titleButton.onClick.AddListener(ReturnToTitle);
        }

        CanvasGroup group = deathPanel.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    private IEnumerator PlaySequence(PlayerDeathTransitionSettings settings)
    {
        zipperGraphic.SetOpening(-0.1f, settings.openingDuration, false);
        harpoonRect.gameObject.SetActive(false);

        yield return WaitRealtime(settings.deathDelay);
        harpoonRect.gameObject.SetActive(true);

        float elapsed = 0f;
        float canvasWidth = 1920f;
        float startX = -settings.harpoonScreenMargin;
        float endX = canvasWidth + settings.harpoonScreenMargin;

        while (elapsed < ActiveTravelDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / ActiveTravelDuration);
            harpoonRect.anchoredPosition = new Vector2(
                Mathf.Lerp(startX, endX, progress),
                settings.harpoonHeight
            );
            zipperGraphic.SetOpening(
                progress,
                settings.openingDuration,
                false
            );
            yield return null;
        }

        harpoonRect.gameObject.SetActive(false);
        yield return FinishOpening(settings);
        yield return WaitRealtime(settings.deathUiDelay);
        yield return FadeDeathPanel(settings.deathUiFadeDuration);
    }

    private IEnumerator FinishOpening(PlayerDeathTransitionSettings settings)
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, settings.finishOpeningDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float extraProgress = elapsed / duration;
            zipperGraphic.SetOpening(
                1f + extraProgress * settings.finishOpeningLead,
                settings.openingDuration,
                false
            );
            yield return null;
        }

        zipperGraphic.SetOpening(2f, settings.openingDuration, true);
    }

    private IEnumerator FadeDeathPanel(float duration)
    {
        CanvasGroup group = deathPanel.GetComponent<CanvasGroup>();
        float elapsed = 0f;
        duration = Mathf.Max(0.01f, duration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        group.alpha = 1f;
        group.blocksRaycasts = true;
        group.interactable = true;

        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.GameOver();
        }
    }

    private void ReturnToTitle()
    {
        if (isReturningToTitle)
        {
            return;
        }

        isReturningToTitle = true;
        Time.timeScale = 1f;
        isPlaying = false;

        Debug.Log(
            "[PlayerDeathTransition] 타이틀 복귀 버튼 입력"
        );

        StartCoroutine(ReturnToTitleRoutine());
    }

    /// <summary>
    /// 기존 화면 페이드가 사용 가능하면 페이드 전환을 요청하고,
    /// 다른 전환이 장시간 점유 중이면 타이틀 씬을 직접 불러옵니다.
    /// </summary>
    private IEnumerator ReturnToTitleRoutine()
    {
        const float transitionReleaseTimeout = 0.5f;
        float elapsed = 0f;

        while (ScreenFadeController.IsTransitioning &&
               elapsed < transitionReleaseTimeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!ScreenFadeController.IsTransitioning)
        {
            ScreenFadeController.LoadTitleScene(TitleSceneName);
            yield break;
        }

        Debug.LogWarning(
            "[PlayerDeathTransition] 기존 화면 전환이 해제되지 않아 " +
            "타이틀 씬을 직접 불러옵니다."
        );
        SceneManager.LoadScene(TitleSceneName);
    }

    private void OnDestroy()
    {
        if (isPlaying)
        {
            Time.timeScale = previousTimeScale;
            isPlaying = false;
        }
    }

    private static IEnumerator WaitRealtime(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private static T CreateStretchGraphic<T>(
        string objectName,
        Transform parent) where T : Graphic
    {
        GameObject child = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(T)
        );
        child.transform.SetParent(parent, false);
        RectTransform rect = child.GetComponent<RectTransform>();
        Stretch(rect);
        return child.GetComponent<T>();
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void CreateText(
        string objectName,
        Transform parent,
        string content,
        float fontSize,
        Vector2 position,
        TMP_FontAsset font)
    {
        GameObject textObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(1100f, 100f);
        rect.anchoredPosition = position;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = content;
        if (font != null)
        {
            text.font = font;
        }
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
    }

    private static Button CreateTitleButton(
        Transform parent,
        TMP_FontAsset font)
    {
        GameObject buttonObject = new GameObject(
            "ReturnToTitleButton",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(420f, 86f);
        rect.anchoredPosition = new Vector2(0f, -115f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.08f, 0.05f, 0.06f, 0.92f);
        CreateText(
            "Label",
            buttonObject.transform,
            "타이틀로 돌아가기",
            32f,
            Vector2.zero,
            font
        );
        return buttonObject.GetComponent<Button>();
    }

    /// <summary>
    /// 전투 UI에서 사용 중인 Pretendard TMP 폰트를 찾아
    /// 런타임 임시 사망 UI에도 동일하게 사용합니다.
    /// </summary>
    private static TMP_FontAsset FindInterfaceFont()
    {
        TextMeshProUGUI[] texts =
            FindObjectsByType<TextMeshProUGUI>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (TextMeshProUGUI text in texts)
        {
            if (text != null &&
                text.font != null &&
                text.font.name.Contains("Pretendard"))
            {
                return text.font;
            }
        }

        Debug.LogWarning(
            "[PlayerDeathTransition] Pretendard TMP 폰트를 " +
            "전투 UI에서 찾지 못했습니다."
        );
        return null;
    }
}

/// <summary>
/// 플레이어 사망 전환에 필요한 이미지와 시간 값을 전달합니다.
/// </summary>
[System.Serializable]
public struct PlayerDeathTransitionSettings
{
    [Header("교체 이미지")]
    public Sprite harpoonSprite;
    public Sprite deathUiSprite;

    [Header("색상")]
    public Color redColor;

    [Header("연출 시간")]
    [Min(0f)] public float deathDelay;
    [Min(0.01f)] public float harpoonTravelDuration;
    [Min(0.01f)] public float openingDuration;
    [Min(0.01f)] public float finishOpeningDuration;
    [Min(0f)] public float deathUiDelay;
    [Min(0.01f)] public float deathUiFadeDuration;

    [Header("작살 배치")]
    public Vector2 harpoonSize;
    public float harpoonHeight;
    [Min(0f)] public float harpoonScreenMargin;
    [Min(0f)] public float finishOpeningLead;
}
