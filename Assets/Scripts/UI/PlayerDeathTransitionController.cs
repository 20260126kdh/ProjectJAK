using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 플레이어 사망 시 작살 충돌, 충돌점 중심의 적색 전환과
/// 사망 UI 표시를 순서대로 처리합니다.
/// </summary>
public sealed class PlayerDeathTransitionController : MonoBehaviour
{
    private const string TitleSceneName = "Main_TitleScene";
    private static bool isPlaying;

    private DeathImpactSpreadGraphic impactSpreadGraphic;
    private RectTransform harpoonRect;
    private CanvasGroup harpoonGroup;
    private RectTransform harpoonTipPoint;
    private RectTransform trailRect;
    private CanvasGroup trailGroup;
    private RectTransform impactEffectRect;
    private CanvasGroup impactEffectGroup;
    private Vector2 impactPoint;
    private Vector2 visualImpactPoint;
    private GameObject deathPanel;
    private TMP_FontAsset interfaceFont;
    private float previousTimeScale = 1f;
    private bool isReturningToTitle;

    /// <summary>
    /// 현재 사망 작살의 이동 시간입니다.
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
    public static void Play(
        PlayerDeathTransitionSettings settings,
        Vector3 playerWorldPosition)
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
        controller.BuildAndPlay(settings, playerWorldPosition);
    }

    private void BuildAndPlay(
        PlayerDeathTransitionSettings settings,
        Vector3 playerWorldPosition)
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

        impactPoint = GetImpactPoint(playerWorldPosition, settings);

        impactSpreadGraphic = CreateStretchGraphic<DeathImpactSpreadGraphic>(
            "DeathImpactSpread",
            transform
        );
        impactSpreadGraphic.color = settings.redColor;
        impactSpreadGraphic.raycastTarget = true;
        impactSpreadGraphic.SetSpread(impactPoint, 0f);

        CreateHarpoon(settings);
        CreateDeathPanel(settings);
        StartCoroutine(PlaySequence(settings));
    }

    private void CreateHarpoon(PlayerDeathTransitionSettings settings)
    {
        GameObject harpoon = new GameObject(
            "DeathHarpoon",
            typeof(RectTransform),
            typeof(Image),
            typeof(CanvasGroup)
        );
        harpoon.transform.SetParent(transform, false);
        harpoonRect = harpoon.GetComponent<RectTransform>();
        harpoonRect.anchorMin = new Vector2(0.5f, 0.5f);
        harpoonRect.anchorMax = new Vector2(0.5f, 0.5f);
        harpoonRect.pivot = new Vector2(1f, 0.5f);
        harpoonRect.sizeDelta = settings.harpoonSize;
        Vector2 startPoint = GetHarpoonStartPoint(settings);
        harpoonRect.anchoredPosition = startPoint;
        Vector2 direction = impactPoint - startPoint;
        harpoonRect.localRotation = Quaternion.Euler(
            0f,
            0f,
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg
        );
        harpoonGroup = harpoon.GetComponent<CanvasGroup>();

        Image image = harpoon.GetComponent<Image>();
        image.sprite = settings.harpoonSprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        image.color = settings.harpoonSprite != null
            ? Color.white
            : new Color(1f, 0.75f, 0.32f, 1f);

        if (settings.harpoonSprite == null)
        {
            image.color = Color.clear;
            CreateFallbackHarpoon(harpoonRect);
        }

        CreateHarpoonTipPoint(settings);
        CreateTrailEffect(settings);
        CreateImpactEffect(settings);
    }

    private void CreateHarpoonTipPoint(
        PlayerDeathTransitionSettings settings)
    {
        GameObject tipPoint = new GameObject(
            "HarpoonTipPoint",
            typeof(RectTransform)
        );
        tipPoint.transform.SetParent(harpoonRect, false);
        harpoonTipPoint = tipPoint.GetComponent<RectTransform>();
        harpoonTipPoint.anchorMin = new Vector2(1f, 0.5f);
        harpoonTipPoint.anchorMax = new Vector2(1f, 0.5f);
        harpoonTipPoint.pivot = new Vector2(0.5f, 0.5f);
        harpoonTipPoint.sizeDelta = Vector2.one;
        harpoonTipPoint.anchoredPosition = new Vector2(
            -GetHarpoonTipInset(settings),
            0f
        );
    }

    private void CreateTrailEffect(
        PlayerDeathTransitionSettings settings)
    {
        if (settings.deathTrailSprite == null)
        {
            return;
        }

        GameObject trail = new GameObject(
            "DeathHarpoonTrail",
            typeof(RectTransform),
            typeof(Image),
            typeof(CanvasGroup)
        );
        trail.transform.SetParent(harpoonRect, false);
        trailRect = trail.GetComponent<RectTransform>();
        trailRect.anchorMin = new Vector2(1f, 0.5f);
        trailRect.anchorMax = new Vector2(1f, 0.5f);
        trailRect.pivot = new Vector2(1f, 0.5f);
        trailRect.sizeDelta = new Vector2(680f, 355f);
        // 원본 트레일의 가시 중심이 이미지 중앙보다 아래에 있어
        // 작살이 회전하면 이펙트가 화면 위로 벌어지므로 실제 가시 중심을 축에 맞춘다.
        trailRect.anchoredPosition = new Vector2(
            -GetHarpoonTipInset(settings),
            82f
        );

        Image trailImage = trail.GetComponent<Image>();
        trailImage.sprite = settings.deathTrailSprite;
        trailImage.preserveAspect = true;
        trailImage.raycastTarget = false;
        trailGroup = trail.GetComponent<CanvasGroup>();
        trailGroup.alpha = 0.82f;
    }

    private void CreateImpactEffect(PlayerDeathTransitionSettings settings)
    {
        if (settings.deathImpactSprite == null)
        {
            return;
        }

        GameObject impact = new GameObject(
            "DeathHarpoonImpact",
            typeof(RectTransform),
            typeof(Image),
            typeof(CanvasGroup)
        );
        impact.transform.SetParent(harpoonTipPoint, false);
        impactEffectRect = impact.GetComponent<RectTransform>();
        impactEffectRect.anchorMin = new Vector2(0.5f, 0.5f);
        impactEffectRect.anchorMax = new Vector2(0.5f, 0.5f);
        impactEffectRect.pivot = new Vector2(0.5f, 0.5f);
        impactEffectRect.sizeDelta = new Vector2(150f, 150f);
        impactEffectRect.anchoredPosition = Vector2.zero;

        Image impactImage = impact.GetComponent<Image>();
        impactImage.sprite = settings.deathImpactSprite;
        impactImage.preserveAspect = true;
        impactImage.raycastTarget = false;
        impactEffectGroup = impact.GetComponent<CanvasGroup>();
        impactEffectGroup.alpha = 0f;
        impact.SetActive(false);
    }

    private static void CreateFallbackHarpoon(Transform parent)
    {
        CreateHarpoonPart(
            "Shaft",
            parent,
            new Vector2(250f, 16f),
            new Vector2(-125f, 0f),
            0f,
            new Color(0.18f, 0.08f, 0.035f, 1f)
        );
        CreateHarpoonPart(
            "Head",
            parent,
            new Vector2(38f, 38f),
            new Vector2(-14f, 0f),
            45f,
            new Color(0.72f, 0.72f, 0.68f, 1f)
        );
    }

    private static void CreateHarpoonPart(
        string objectName,
        Transform parent,
        Vector2 size,
        Vector2 position,
        float rotation,
        Color color)
    {
        GameObject part = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image)
        );
        part.transform.SetParent(parent, false);
        RectTransform rect = part.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        rect.localRotation = Quaternion.Euler(0f, 0f, rotation);

        Image image = part.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
    }

    private static Vector2 GetImpactPoint(
        Vector3 playerWorldPosition,
        PlayerDeathTransitionSettings settings)
    {
        Camera worldCamera = Camera.main;
        if (worldCamera == null || Screen.width <= 0 || Screen.height <= 0)
        {
            return new Vector2(-480f, -80f) + settings.impactOffset;
        }

        Vector3 screenPoint = worldCamera.WorldToScreenPoint(playerWorldPosition);
        return new Vector2(
            (screenPoint.x / Screen.width - 0.5f) * 1920f,
            (screenPoint.y / Screen.height - 0.5f) * 1080f
        ) + settings.impactOffset;
    }

    private static Vector2 GetHarpoonStartPoint(
        PlayerDeathTransitionSettings settings)
    {
        return new Vector2(
            960f + settings.harpoonScreenMargin,
            540f + settings.harpoonScreenMargin
        );
    }

    private static float GetHarpoonTipInset(
        PlayerDeathTransitionSettings settings)
    {
        /*
         * 작살 PNG의 실제 불투명 작살촉은 이미지 오른쪽 끝에서
         * 원본 폭의 약 2.65% 안쪽에 있으므로 그만큼 보정합니다.
         */
        return settings.harpoonSprite != null
            ? settings.harpoonSize.x * 0.0265f
            : 0f;
    }

    private Vector2 GetVisualImpactPoint()
    {
        if (impactEffectRect != null)
        {
            return transform.InverseTransformPoint(
                impactEffectRect.position
            );
        }

        if (harpoonTipPoint == null)
        {
            return impactPoint;
        }

        return transform.InverseTransformPoint(harpoonTipPoint.position);
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
        Sprite backgroundSprite = settings.deathUiSprite != null
            ? settings.deathUiSprite
            : settings.deathBackgroundSprite;
        background.sprite = backgroundSprite;
        // 새 배경은 3:2 원본을 16:9 화면 전체에 채워 빈 여백을 남기지 않는다.
        // 기존 완성형 사망 UI Sprite의 비율 보존 동작은 그대로 유지한다.
        background.preserveAspect = settings.deathUiSprite != null;
        background.color = backgroundSprite != null
            ? Color.white
            : settings.redColor;

        if (settings.deathUiSprite == null)
        {
            RectTransform summaryPanel = CreateDeathSummaryPanel(
                panelRect,
                settings.deathPanelSprite
            );

            CreateText(
                "DeathTitleLabel",
                summaryPanel,
                "당신은 죽었습니다.",
                54f,
                new Vector2(0f, 250f),
                interfaceFont,
                new Color(1f, 0.25f, 0.18f, 1f),
                new Vector2(500f, 110f)
            );

            CreateText(
                "DeathProgressLabel",
                summaryPanel,
                BuildDeathProgressText(),
                36f,
                new Vector2(0f, 25f),
                interfaceFont,
                new Color(1f, 0.93f, 0.78f, 1f),
                new Vector2(500f, 280f)
            );

            CreateDeathCharacter(panelRect, settings);

            Button titleButton = CreateTitleButton(
                summaryPanel,
                interfaceFont,
                settings
            );
            titleButton.onClick.AddListener(ReturnToTitle);
        }

        CanvasGroup group = deathPanel.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    private static RectTransform CreateDeathSummaryPanel(
        Transform parent,
        Sprite panelSprite)
    {
        GameObject opaqueInterior = new GameObject(
            "DeathSummaryOpaqueInterior",
            typeof(RectTransform),
            typeof(Image)
        );
        opaqueInterior.transform.SetParent(parent, false);

        RectTransform interiorRect =
            opaqueInterior.GetComponent<RectTransform>();
        interiorRect.anchorMin = new Vector2(0.5f, 0.5f);
        interiorRect.anchorMax = new Vector2(0.5f, 0.5f);
        // 생성 이미지의 장식 테두리보다 안쪽에만 배치해 외곽 투명을 유지합니다.
        interiorRect.sizeDelta = new Vector2(500f, 770f);
        interiorRect.anchoredPosition = Vector2.zero;

        Image interiorImage = opaqueInterior.GetComponent<Image>();
        interiorImage.color = new Color(0.012f, 0.008f, 0.009f, 1f);
        interiorImage.raycastTarget = false;

        GameObject panel = new GameObject(
            "DeathSummaryPanel",
            typeof(RectTransform),
            typeof(Image)
        );
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(630f, 840f);
        rect.anchoredPosition = Vector2.zero;

        Image image = panel.GetComponent<Image>();
        image.sprite = panelSprite;
        image.preserveAspect = false;
        image.color = panelSprite != null
            ? Color.white
            : new Color(0.015f, 0.012f, 0.012f, 0.96f);
        image.raycastTarget = false;

        return rect;
    }

    private static string BuildDeathProgressText()
    {
        StageManager stageManager = StageManager.Instance;
        if (stageManager == null)
        {
            return
                "최종 진행도\n\n" +
                "1 스테이지  ·  1번째 전투\n" +
                "<color=#F2F2F2>일반 전투</color>";
        }

        int battleNumber = stageManager.CurrentBattleCount + 1;
        string battleType =
            "<color=#F2F2F2>일반 전투</color>";

        if (stageManager.CurrentPhase == StagePhase.BossBattle)
        {
            battleType = stageManager.IsStage3ArielBattle
                ? "<color=#FFD36A>진 보스 전투</color>"
                : "<color=#FF4B3E>보스 전투</color>";

            // 3스테이지 아리엘은 모르바엘 다음에 이어지는 추가 보스입니다.
            if (stageManager.IsStage3ArielBattle)
            {
                battleNumber++;
            }
        }

        return
            $"최종 진행도\n\n" +
            $"{stageManager.CurrentStage} 스테이지  ·  " +
            $"{battleNumber}번째 전투\n{battleType}";
    }

    private static void CreateDeathCharacter(
        Transform parent,
        PlayerDeathTransitionSettings settings)
    {
        if (settings.deathCharacterSprite == null)
        {
            return;
        }

        GameObject character = new GameObject(
            "DeathCharacter",
            typeof(RectTransform),
            typeof(Image)
        );
        character.transform.SetParent(parent, false);

        RectTransform rect = character.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
        rect.sizeDelta = settings.deathCharacterSize.sqrMagnitude > 0f
            ? settings.deathCharacterSize
            : new Vector2(520f, 360f);
        rect.anchoredPosition = settings.deathCharacterPosition.sqrMagnitude > 0f
            ? settings.deathCharacterPosition
            : new Vector2(35f, 30f);

        Image image = character.GetComponent<Image>();
        image.sprite = settings.deathCharacterSprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    private IEnumerator PlaySequence(PlayerDeathTransitionSettings settings)
    {
        harpoonRect.gameObject.SetActive(false);

        yield return WaitRealtime(settings.deathDelay);
        harpoonRect.gameObject.SetActive(true);

        float elapsed = 0f;
        Vector2 startPoint = GetHarpoonStartPoint(settings);

        while (elapsed < ActiveTravelDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / ActiveTravelDuration);
            harpoonRect.anchoredPosition = Vector2.Lerp(
                startPoint,
                impactPoint,
                progress
            );
            if (trailRect != null)
            {
                float pulse = 1f + Mathf.Sin(progress * Mathf.PI * 8f) * 0.06f;
                trailRect.localScale = new Vector3(pulse, 1f, 1f);
            }
            yield return null;
        }

        harpoonRect.anchoredPosition = impactPoint;
        visualImpactPoint = GetVisualImpactPoint();
        StartCoroutine(FadeArrivalVisuals(settings.impactHoldDuration));
        yield return SpreadImpact(settings);
        harpoonRect.gameObject.SetActive(false);
        yield return WaitRealtime(settings.deathUiDelay);
        yield return FadeDeathPanel(settings.deathUiFadeDuration);
    }

    private IEnumerator FadeArrivalVisuals(float duration)
    {
        duration = Mathf.Max(0.01f, duration);
        if (impactEffectRect != null)
        {
            impactEffectRect.gameObject.SetActive(true);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            harpoonGroup.alpha = 1f - progress;
            if (trailGroup != null)
            {
                trailGroup.alpha = 0.82f * (1f - progress);
            }
            if (impactEffectGroup != null)
            {
                impactEffectGroup.alpha = Mathf.Sin(progress * Mathf.PI);
                impactEffectRect.localScale = Vector3.one *
                    Mathf.Lerp(0.65f, 1.25f, progress);
            }
            yield return null;
        }

        harpoonRect.gameObject.SetActive(false);
        if (impactEffectRect != null)
        {
            impactEffectRect.gameObject.SetActive(false);
        }
    }

    private IEnumerator SpreadImpact(PlayerDeathTransitionSettings settings)
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, settings.impactSpreadDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            impactSpreadGraphic.SetSpread(
                visualImpactPoint,
                Mathf.Clamp01(elapsed / duration)
            );
            yield return null;
        }

        impactSpreadGraphic.SetSpread(visualImpactPoint, 1f);
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

    private static TextMeshProUGUI CreateText(
        string objectName,
        Transform parent,
        string content,
        float fontSize,
        Vector2 position,
        TMP_FontAsset font,
        Color? color = null,
        Vector2? size = null)
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
        rect.sizeDelta = size ?? new Vector2(1100f, 100f);
        rect.anchoredPosition = position;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = content;
        if (font != null)
        {
            text.font = font;
        }
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color ?? Color.white;
        text.outlineColor = new Color32(0, 0, 0, 255);
        text.outlineWidth = 0.18f;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateTitleButton(
        Transform parent,
        TMP_FontAsset font,
        PlayerDeathTransitionSettings settings)
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
        rect.sizeDelta = new Vector2(410f, 88f);
        rect.anchoredPosition = new Vector2(0f, -270f);

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = settings.titleButtonNormalSprite;
        image.color = settings.titleButtonNormalSprite != null
            ? Color.white
            : new Color(0.12f, 0.31f, 0.45f, 0.96f);

        Button button = buttonObject.GetComponent<Button>();
        button.transition = Selectable.Transition.SpriteSwap;
        SpriteState spriteState = button.spriteState;
        spriteState.highlightedSprite = settings.titleButtonHoverSprite;
        spriteState.pressedSprite = settings.titleButtonPressedSprite;
        button.spriteState = spriteState;
        CreateText(
            "Label",
            buttonObject.transform,
            "타이틀로 돌아가기",
            34f,
            Vector2.zero,
            font,
            new Color(0.78f, 0.94f, 0.86f, 1f)
        );
        return button;
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
    public Sprite deathTrailSprite;
    public Sprite deathImpactSprite;
    public Sprite deathUiSprite;
    [Tooltip("붉은 확산 완료 후 기존 사망 UI 뒤에 표시할 공통 배경")]
    public Sprite deathBackgroundSprite;
    [Tooltip("사망 문구와 최종 스테이지에 공통으로 사용하는 프레임")]
    public Sprite deathPanelSprite;
    [Tooltip("왼쪽 아래에 표시할 현재 클래스의 사망 이미지")]
    public Sprite deathCharacterSprite;
    public Sprite titleButtonNormalSprite;
    public Sprite titleButtonHoverSprite;
    public Sprite titleButtonPressedSprite;

    [Header("사망 캐릭터 배치")]
    public Vector2 deathCharacterSize;
    public Vector2 deathCharacterPosition;

    [Header("색상")]
    public Color redColor;

    [Header("연출 시간")]
    [Min(0f)] public float deathDelay;
    [Min(0.01f)] public float harpoonTravelDuration;
    [Min(0.01f)] public float openingDuration;
    [Min(0.01f)] public float finishOpeningDuration;
    [Min(0f)] public float deathUiDelay;
    [Min(0.01f)] public float deathUiFadeDuration;
    [Min(0f)] public float impactHoldDuration;
    [Min(0.01f)] public float impactSpreadDuration;

    [Header("작살 배치")]
    public Vector2 harpoonSize;
    public float harpoonHeight;
    public Vector2 impactOffset;
    [Min(0f)] public float harpoonScreenMargin;
    [Min(0f)] public float finishOpeningLead;
}

/// <summary>
/// 작살 충돌 지점을 중심으로 붉은 원이 화면 전체까지
/// 확산되는 사망 전환 UI 메시를 그립니다.
/// </summary>
public sealed class DeathImpactSpreadGraphic : MaskableGraphic
{
    private const int SegmentCount = 96;
    private Vector2 impactPoint;
    private float spreadProgress;

    /// <summary>
    /// 확산 중심과 진행률을 갱신합니다.
    /// </summary>
    public void SetSpread(Vector2 center, float progress)
    {
        impactPoint = center;
        spreadProgress = Mathf.Clamp01(progress);
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
        if (spreadProgress <= 0f)
        {
            return;
        }

        Rect rect = rectTransform.rect;
        float radius = GetMaximumRadius(rect, impactPoint) *
            Mathf.SmoothStep(0f, 1f, spreadProgress);
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;
        vertex.position = impactPoint;
        vertexHelper.AddVert(vertex);

        for (int index = 0; index <= SegmentCount; index++)
        {
            float angle = Mathf.PI * 2f * index / SegmentCount;
            vertex.position = impactPoint + new Vector2(
                Mathf.Cos(angle),
                Mathf.Sin(angle)
            ) * radius;
            vertexHelper.AddVert(vertex);
            if (index > 0)
            {
                vertexHelper.AddTriangle(0, index, index + 1);
            }
        }
    }

    private static float GetMaximumRadius(Rect rect, Vector2 center)
    {
        float leftBottom = Vector2.Distance(
            center,
            new Vector2(rect.xMin, rect.yMin)
        );
        float leftTop = Vector2.Distance(
            center,
            new Vector2(rect.xMin, rect.yMax)
        );
        float rightBottom = Vector2.Distance(
            center,
            new Vector2(rect.xMax, rect.yMin)
        );
        float rightTop = Vector2.Distance(
            center,
            new Vector2(rect.xMax, rect.yMax)
        );
        return Mathf.Max(leftBottom, leftTop, rightBottom, rightTop) * 1.02f;
    }
}
