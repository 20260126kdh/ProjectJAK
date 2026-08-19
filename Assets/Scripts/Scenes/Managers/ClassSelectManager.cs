using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

/// <summary>
/// 클래스 선택 씬의 클래스 선택과 상세 정보 UI를 관리합니다.
///
/// 담당 기능
/// - 클래스 선택
/// - 클래스 상세 정보 표시
/// - 상세 정보 패널 열기 및 닫기
/// - 클래스 선택 확정
/// - 타이틀 씬으로 이동
/// </summary>
public class ClassSelectManager : MonoBehaviour
{
    [Header("Scene 설정")]
    [Tooltip("메인 타이틀 씬의 이름입니다.")]
    [SerializeField]
    private string titleSceneName = "TitleScene";

    [Tooltip("전투 씬의 이름입니다.")]
    [SerializeField]
    private string battleSceneName = "BattleScene";

    [Header("클래스 선택 카드")]
    [SerializeField]
    private Button physiqueButton;

    [SerializeField]
    private Button technicianButton;

    [SerializeField]
    private Button captainButton;

    [Header("추후 교체할 9-slice 프레임")]
    [SerializeField]
    private Sprite physiqueFrameSprite;

    [SerializeField]
    private Sprite technicianFrameSprite;

    [SerializeField]
    private Sprite captainFrameSprite;

    [Header("카드 전환 설정")]
    [SerializeField]
    [Min(0f)]
    private float cardTransitionDuration = 0.36f;

    [SerializeField]
    [Min(0f)]
    private float detailContentDelay = 0.2f;

    [SerializeField]
    private Vector2 selectedCardSize = new Vector2(1180f, 660f);

    [SerializeField]
    [Range(0.1f, 1f)]
    private float detailPanelScale = 0.75f;

    [Header("클래스 상세 정보 패널")]
    [SerializeField]
    private GameObject classDetailPanel;

    [Header("클래스 정보들")]
    [SerializeField]
    private ClassInfo[] classInfos;

    [Header("클래스 이미지")]
    [SerializeField]
    private Image characterImage;

    [Header("버튼")]
    [SerializeField]
    private Button confirmButton;

    [Header("텍스트")]
    [SerializeField]
    private TMP_Text classNameText;

    [SerializeField]
    private TMP_Text hpText;

    [SerializeField]
    private TMP_Text passiveText;

    [SerializeField]
    private TMP_Text descriptionText;

    /// <summary>
    /// 현재 선택된 클래스입니다.
    /// </summary>
    private PlayerClass selectedClass = PlayerClass.None;

    /// <summary>
    /// 현재 선택된 클래스의 상세 정보입니다.
    /// </summary>
    private ClassInfo selectedClassInfo;

    private ClassSelectionCardUI[] selectionCards;
    private Vector2 cardCenterPosition;
    private Coroutine openDetailCoroutine;
    private VideoPlayer classVideoPlayer;
    private RawImage classVideoImage;
    private RectTransform classVideoRect;
    private RawImage classVideoBackground;
    private AspectRatioFitter classVideoBackgroundAspectFitter;
    private Material classVideoBlurMaterial;
    private GameObject classVideoBorder;
    private Image[] classVideoBorderEdges;

    private const float PhysiqueVideoCenterX = 0.60f;
    private const float TechnicianVideoCenterX = 0.59f;

    /// <summary>
    /// 현재 선택된 클래스를 반환합니다.
    /// </summary>
    public PlayerClass SelectedClass => selectedClass;

    /// <summary>
    /// 클래스 상세 정보 패널이 열려 있는지 반환합니다.
    /// </summary>
    public bool IsDetailPanelOpen
    {
        get
        {
            return (classDetailPanel != null && classDetailPanel.activeSelf)
                || openDetailCoroutine != null;
        }
    }

    /// <summary>
    /// 현재 클래스 선택을 확정할 수 있는 상태인지 반환합니다.
    /// </summary>
    public bool CanConfirmSelection
    {
        get
        {
            return classDetailPanel != null
                && classDetailPanel.activeSelf
                && selectedClass != PlayerClass.None
                && selectedClassInfo != null;
        }
    }

    /// <summary>
    /// 시작 시 상세 패널과 선택 상태를 초기화합니다.
    /// </summary>
    private void Start()
    {
        InitializeSelectionCards();
        InitializeClassVideo();
        ResetSelection();
    }

    private void OnDestroy()
    {
        if (classVideoPlayer != null)
        {
            classVideoPlayer.prepareCompleted -= OnClassVideoPrepared;
            classVideoPlayer.errorReceived -= OnClassVideoError;
        }

        if (classVideoBlurMaterial != null)
        {
            Destroy(classVideoBlurMaterial);
        }
    }

    /// <summary>
    /// 피지크 클래스를 선택합니다.
    /// Unity Button의 On Click에서 사용합니다.
    /// </summary>
    public void SelectPhysique()
    {
        SelectClass(PlayerClass.Physique);
    }

    /// <summary>
    /// 테크니션 클래스를 선택합니다.
    /// Unity Button의 On Click에서 사용합니다.
    /// </summary>
    public void SelectTechnician()
    {
        SelectClass(PlayerClass.Technician);
    }

    /// <summary>
    /// 캡틴 클래스를 선택합니다.
    /// Unity Button의 On Click에서 사용합니다.
    /// </summary>
    public void SelectCaptain()
    {
        SelectClass(PlayerClass.Captain);
    }

    /// <summary>
    /// 클래스를 선택하고 상세 정보 패널을 표시합니다.
    /// </summary>
    /// <param name="playerClass">선택할 플레이어 클래스</param>
    public void SelectClass(PlayerClass playerClass)
    {
        if (playerClass == PlayerClass.None || playerClass == PlayerClass.All)
        {
            Debug.LogWarning(
                $"[ClassSelectManager] 선택할 수 없는 클래스입니다: {playerClass}"
            );

            return;
        }

        ClassInfo classInfo = FindClassInfo(playerClass);

        if (classInfo == null)
        {
            Debug.LogError(
                $"[ClassSelectManager] {playerClass} 클래스 정보를 찾을 수 없습니다."
            );

            return;
        }

        selectedClass = playerClass;
        selectedClassInfo = classInfo;

        ShowClassInfo(classInfo);
        ApplySelectedCardLayout(playerClass);
        ScheduleDetailPanelOpen();
    }

    /// <summary>
    /// 클래스 상세 정보 패널을 표시합니다.
    /// </summary>
    public void OpenDetailPanel()
    {
        if (classDetailPanel == null)
        {
            Debug.LogError(
                "[ClassSelectManager] Class Detail Panel이 연결되지 않았습니다."
            );

            return;
        }

        classDetailPanel.SetActive(true);
        classDetailPanel.transform.SetAsLastSibling();

        RectTransform detailRect =
            classDetailPanel.GetComponent<RectTransform>();

        if (detailRect != null)
        {
            detailRect.localScale = Vector3.one * detailPanelScale;
        }
    }

    /// <summary>
    /// 클래스 상세 정보 패널을 닫고 선택 상태를 초기화합니다.
    /// Back 버튼과 Esc 단축키에서 공통으로 사용합니다.
    /// </summary>
    public void CloseDetailPanel()
    {
        ResetSelection();
    }

    /// <summary>
    /// 선택 카드의 확대가 끝난 뒤 상세 정보 패널을 표시합니다.
    /// </summary>
    private void ScheduleDetailPanelOpen()
    {
        if (openDetailCoroutine != null)
        {
            StopCoroutine(openDetailCoroutine);
        }

        openDetailCoroutine = StartCoroutine(OpenDetailPanelAfterTransition());
    }

    private IEnumerator OpenDetailPanelAfterTransition()
    {
        if (cardTransitionDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(cardTransitionDuration);
        }

        if (detailContentDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(detailContentDelay);
        }

        OpenDetailPanel();
        SetConfirmButtonInteractable(true);
        openDetailCoroutine = null;
    }

    /// <summary>
    /// 현재 선택된 클래스를 반환합니다.
    /// 기존 코드와의 호환을 위해 유지합니다.
    /// </summary>
    public PlayerClass GetSelectedClass()
    {
        return selectedClass;
    }

    /// <summary>
    /// 클래스 선택을 확정하고 전투 씬으로 이동합니다.
    /// Confirm 버튼과 Enter 단축키에서 공통으로 사용합니다.
    /// </summary>
    public void ConfirmSelection()
    {
        if (!CanConfirmSelection)
        {
            Debug.LogWarning(
                "[ClassSelectManager] 확정할 수 있는 클래스가 선택되지 않았습니다."
            );

            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "[ClassSelectManager] GameManager.Instance를 찾을 수 없습니다."
            );

            return;
        }

        if (GameManager.Instance.PlayerData == null)
        {
            Debug.LogError(
                "[ClassSelectManager] GameManager에 PlayerData가 연결되지 않았습니다."
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(battleSceneName))
        {
            Debug.LogError(
                "[ClassSelectManager] Battle Scene Name이 비어 있습니다."
            );

            return;
        }

        GameManager.Instance.PlayerData.SetClass(selectedClass);
        GameManager.Instance.PlayerData.SetHP(selectedClassInfo.maxHP);

        Debug.Log(
            $"[ClassSelectManager] 클래스 선택 확정: {selectedClass}, " +
            $"최대 체력: {selectedClassInfo.maxHP}"
        );

        ScreenFadeController.LoadBattleScene(battleSceneName);
    }

    /// <summary>
    /// 메인 타이틀 씬으로 이동합니다.
    /// 클래스 목록 화면의 Esc 단축키에서 사용합니다.
    /// </summary>
    public void ReturnToTitle()
    {
        if (string.IsNullOrWhiteSpace(titleSceneName))
        {
            Debug.LogError(
                "[ClassSelectManager] Title Scene Name이 비어 있습니다."
            );

            return;
        }

        ScreenFadeController.LoadTitleScene(titleSceneName);
    }

    /// <summary>
    /// 지정한 클래스의 정보를 찾습니다.
    /// </summary>
    /// <param name="playerClass">찾을 플레이어 클래스</param>
    /// <returns>찾은 클래스 정보, 없으면 null</returns>
    private ClassInfo FindClassInfo(PlayerClass playerClass)
    {
        if (classInfos == null || classInfos.Length == 0)
        {
            Debug.LogError(
                "[ClassSelectManager] Class Infos가 비어 있습니다."
            );

            return null;
        }

        foreach (ClassInfo info in classInfos)
        {
            if (info == null)
            {
                continue;
            }

            if (info.playerClass == playerClass)
            {
                return info;
            }
        }

        return null;
    }

    /// <summary>
    /// 선택한 클래스 정보를 UI에 표시합니다.
    /// </summary>
    /// <param name="classInfo">표시할 클래스 정보</param>
    private void ShowClassInfo(ClassInfo classInfo)
    {
        if (classInfo == null)
        {
            Debug.LogError(
                "[ClassSelectManager] 표시할 ClassInfo가 없습니다."
            );

            return;
        }

        if (characterImage != null)
        {
            characterImage.sprite = classInfo.classImage;
        }

        PlayClassVideo(classInfo.playerClass);

        if (classNameText != null)
        {
            classNameText.text = classInfo.className;
        }

        if (hpText != null)
        {
            hpText.text = $"{classInfo.maxHP} / {classInfo.maxHP}";
        }

        if (passiveText != null)
        {
            passiveText.text = classInfo.passiveDescription;
            passiveText.horizontalAlignment = HorizontalAlignmentOptions.Left;
            passiveText.verticalAlignment = classInfo.playerClass == PlayerClass.Physique
                ? VerticalAlignmentOptions.Middle
                : VerticalAlignmentOptions.Top;
        }

        if (descriptionText != null)
        {
            descriptionText.text = classInfo.description;
            descriptionText.horizontalAlignment = HorizontalAlignmentOptions.Left;
            descriptionText.verticalAlignment = classInfo.playerClass == PlayerClass.Technician
                ? VerticalAlignmentOptions.Middle
                : VerticalAlignmentOptions.Top;
        }
    }

    /// <summary>
    /// 클래스 선택 상태와 상세 패널을 초기화합니다.
    /// </summary>
    private void ResetSelection()
    {
        if (openDetailCoroutine != null)
        {
            StopCoroutine(openDetailCoroutine);
            openDetailCoroutine = null;
        }

        selectedClass = PlayerClass.None;
        selectedClassInfo = null;

        StopClassVideo();

        if (classDetailPanel != null)
        {
            classDetailPanel.SetActive(false);
        }

        RestoreDefaultCardLayout();

        SetConfirmButtonInteractable(false);
    }

    /// <summary>
    /// 클래스 상세 이미지 칸에 영상을 표시할 UI와 재생기를 준비합니다.
    /// </summary>
    private void InitializeClassVideo()
    {
        if (characterImage == null)
        {
            Debug.LogError(
                "[ClassSelectManager] 클래스 영상 표시 영역이 연결되지 않았습니다."
            );
            return;
        }

        Mask videoMask = characterImage.GetComponent<Mask>();
        if (videoMask == null)
        {
            videoMask = characterImage.gameObject.AddComponent<Mask>();
        }
        videoMask.showMaskGraphic = true;

        GameObject videoObject = new GameObject(
            "ClassVideo",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(RawImage)
        );

        classVideoRect = videoObject.GetComponent<RectTransform>();
        classVideoRect.SetParent(characterImage.rectTransform, false);
        classVideoRect.anchorMin = Vector2.zero;
        classVideoRect.anchorMax = Vector2.one;
        classVideoRect.anchoredPosition = Vector2.zero;
        classVideoRect.sizeDelta = Vector2.zero;

        classVideoImage = videoObject.GetComponent<RawImage>();
        classVideoImage.raycastTarget = false;
        classVideoImage.color = Color.white;

        InitializeClassVideoBorder();
        InitializeClassVideoBackground();

        classVideoPlayer = gameObject.AddComponent<VideoPlayer>();
        classVideoPlayer.playOnAwake = false;
        classVideoPlayer.source = VideoSource.VideoClip;
        classVideoPlayer.renderMode = VideoRenderMode.APIOnly;
        classVideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        classVideoPlayer.isLooping = true;
        classVideoPlayer.skipOnDrop = true;
        classVideoPlayer.waitForFirstFrame = true;
        classVideoPlayer.prepareCompleted += OnClassVideoPrepared;
        classVideoPlayer.errorReceived += OnClassVideoError;
    }

    private void InitializeClassVideoBorder()
    {
        RectTransform characterRect = characterImage.rectTransform;
        classVideoBorder = new GameObject(
            "ClassVideoBorder",
            typeof(RectTransform)
        );

        RectTransform borderRect =
            classVideoBorder.GetComponent<RectTransform>();
        borderRect.SetParent(characterRect.parent, false);
        borderRect.anchorMin = characterRect.anchorMin;
        borderRect.anchorMax = characterRect.anchorMax;
        borderRect.pivot = characterRect.pivot;
        borderRect.anchoredPosition = characterRect.anchoredPosition;
        borderRect.sizeDelta = characterRect.sizeDelta;
        borderRect.SetSiblingIndex(characterRect.GetSiblingIndex() + 1);

        classVideoBorderEdges = new[]
        {
            CreateVideoBorderEdge(
                "Top",
                borderRect,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, 4f)
            ),
            CreateVideoBorderEdge(
                "Bottom",
                borderRect,
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 4f)
            ),
            CreateVideoBorderEdge(
                "Left",
                borderRect,
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(4f, 0f)
            ),
            CreateVideoBorderEdge(
                "Right",
                borderRect,
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(4f, 0f)
            )
        };

        classVideoBorder.SetActive(false);
    }

    private static Image CreateVideoBorderEdge(
        string edgeName,
        RectTransform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 sizeDelta)
    {
        GameObject edgeObject = new GameObject(
            edgeName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        RectTransform edgeRect = edgeObject.GetComponent<RectTransform>();
        edgeRect.SetParent(parent, false);
        edgeRect.anchorMin = anchorMin;
        edgeRect.anchorMax = anchorMax;
        edgeRect.anchoredPosition = Vector2.zero;
        edgeRect.sizeDelta = sizeDelta;

        Image edgeImage = edgeObject.GetComponent<Image>();
        edgeImage.raycastTarget = false;
        return edgeImage;
    }

    private void InitializeClassVideoBackground()
    {
        if (classDetailPanel == null)
        {
            Debug.LogError(
                "[ClassSelectManager] 클래스 영상 배경을 배치할 상세 패널이 없습니다."
            );
            return;
        }

        GameObject backgroundObject = new GameObject(
            "ClassVideoBackground",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(RawImage),
            typeof(AspectRatioFitter)
        );

        RectTransform backgroundRect =
            backgroundObject.GetComponent<RectTransform>();
        backgroundRect.SetParent(classDetailPanel.transform, false);
        backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
        backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
        backgroundRect.anchoredPosition = Vector2.zero;
        backgroundRect.sizeDelta = new Vector2(1920f, 1080f);
        backgroundRect.SetAsFirstSibling();

        classVideoBackground = backgroundObject.GetComponent<RawImage>();
        classVideoBackground.raycastTarget = false;

        classVideoBackgroundAspectFitter =
            backgroundObject.GetComponent<AspectRatioFitter>();
        classVideoBackgroundAspectFitter.aspectMode =
            AspectRatioFitter.AspectMode.EnvelopeParent;

        Shader blurShader = Shader.Find("UI/Class Video Blur");
        if (blurShader != null)
        {
            classVideoBlurMaterial = new Material(blurShader);
            classVideoBackground.material = classVideoBlurMaterial;
        }
        else
        {
            Debug.LogError(
                "[ClassSelectManager] 클래스 영상 블러 Shader를 찾을 수 없습니다."
            );
        }

        GameObject overlayObject = new GameObject(
            "ClassVideoDarkOverlay",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
        overlayRect.SetParent(classDetailPanel.transform, false);
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.anchoredPosition = Vector2.zero;
        overlayRect.sizeDelta = Vector2.zero;
        overlayRect.SetSiblingIndex(1);

        Image overlayImage = overlayObject.GetComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.28f);
        overlayImage.raycastTarget = false;
    }

    private void PlayClassVideo(PlayerClass playerClass)
    {
        if (classVideoPlayer == null || classVideoImage == null)
        {
            return;
        }

        string resourcePath = GetClassVideoResourcePath(playerClass);
        VideoClip videoClip = Resources.Load<VideoClip>(resourcePath);

        if (videoClip == null)
        {
            Debug.LogError(
                $"[ClassSelectManager] 클래스 선택 영상을 찾을 수 없습니다: {resourcePath}"
            );
            return;
        }

        ApplyClassVideoBorder(playerClass);

        classVideoPlayer.Stop();
        classVideoImage.texture = null;
        classVideoPlayer.clip = videoClip;
        classVideoPlayer.Prepare();
    }

    private void ApplyClassVideoBorder(PlayerClass playerClass)
    {
        if (classVideoBorder == null || classVideoBorderEdges == null)
        {
            return;
        }

        Color borderColor = GetClassVideoBorderColor(playerClass);

        foreach (Image borderEdge in classVideoBorderEdges)
        {
            borderEdge.color = borderColor;
        }

        classVideoBorder.SetActive(true);
    }

    private static string GetClassVideoResourcePath(PlayerClass playerClass)
    {
        switch (playerClass)
        {
            case PlayerClass.Physique:
                return "Video/ClassChoice/Class_Choice_PHY";

            case PlayerClass.Technician:
                return "Video/ClassChoice/Class_Choice_TEC";

            case PlayerClass.Captain:
                return "Video/ClassChoice/Class_Choice_CAP";

            default:
                return string.Empty;
        }
    }

    private static Color GetClassVideoBorderColor(PlayerClass playerClass)
    {
        switch (playerClass)
        {
            case PlayerClass.Physique:
                return new Color(0.52f, 0.90f, 0.90f, 1f);

            case PlayerClass.Technician:
                return new Color(0.95f, 0.64f, 0.82f, 1f);

            case PlayerClass.Captain:
                return new Color(0.72f, 0.90f, 0.52f, 1f);

            default:
                return Color.white;
        }
    }

    private void OnClassVideoPrepared(VideoPlayer preparedPlayer)
    {
        if (preparedPlayer.clip == null || classVideoImage == null)
        {
            return;
        }

        classVideoImage.texture = preparedPlayer.texture;

        if (classVideoBackground != null)
        {
            classVideoBackground.texture = preparedPlayer.texture;
        }

        if (preparedPlayer.clip.height > 0)
        {
            classVideoImage.uvRect = CalculateClassVideoUvRect(
                preparedPlayer.clip,
                selectedClass
            );
        }

        if (classVideoBackgroundAspectFitter != null &&
            preparedPlayer.clip.height > 0)
        {
            classVideoBackgroundAspectFitter.aspectRatio =
                (float)preparedPlayer.clip.width / preparedPlayer.clip.height;
        }

        preparedPlayer.Play();
    }

    private Rect CalculateClassVideoUvRect(
        VideoClip videoClip,
        PlayerClass playerClass)
    {
        if (characterImage == null || videoClip.height == 0)
        {
            return new Rect(0f, 0f, 1f, 1f);
        }

        Rect viewport = characterImage.rectTransform.rect;
        if (viewport.height <= 0f)
        {
            return new Rect(0f, 0f, 1f, 1f);
        }

        float sourceAspect = (float)videoClip.width / videoClip.height;
        float viewportAspect = viewport.width / viewport.height;

        if (sourceAspect > viewportAspect)
        {
            float visibleWidth = viewportAspect / sourceAspect;
            float centerX = GetClassVideoCenterX(playerClass);
            float left = Mathf.Clamp(
                centerX - visibleWidth * 0.5f,
                0f,
                1f - visibleWidth
            );
            return new Rect(left, 0f, visibleWidth, 1f);
        }

        float visibleHeight = sourceAspect / viewportAspect;
        float bottom = (1f - visibleHeight) * 0.5f;
        return new Rect(0f, bottom, 1f, visibleHeight);
    }

    private static float GetClassVideoCenterX(PlayerClass playerClass)
    {
        switch (playerClass)
        {
            case PlayerClass.Physique:
                return PhysiqueVideoCenterX;

            case PlayerClass.Technician:
                return TechnicianVideoCenterX;

            default:
                return 0.5f;
        }
    }

    private static void OnClassVideoError(
        VideoPlayer failedPlayer,
        string message)
    {
        Debug.LogError(
            $"[ClassSelectManager] 클래스 선택 영상 재생 실패: {message}"
        );
    }

    private void StopClassVideo()
    {
        if (classVideoPlayer != null)
        {
            classVideoPlayer.Stop();
            classVideoPlayer.clip = null;
        }

        if (classVideoImage != null)
        {
            classVideoImage.texture = null;
        }

        if (classVideoBackground != null)
        {
            classVideoBackground.texture = null;
        }

        if (classVideoBorder != null)
        {
            classVideoBorder.SetActive(false);
        }
    }

    /// <summary>
    /// Confirm 버튼의 활성화 상태를 설정합니다.
    /// </summary>
    /// <param name="isInteractable">버튼 활성화 여부</param>
    private void SetConfirmButtonInteractable(bool isInteractable)
    {
        if (confirmButton != null)
        {
            confirmButton.interactable = isInteractable;
        }
    }

    /// <summary>
    /// 기존 클래스 버튼에 카드 프레임과 전환 동작을 연결합니다.
    /// </summary>
    private void InitializeSelectionCards()
    {
        if (physiqueButton == null ||
            technicianButton == null ||
            captainButton == null)
        {
            Debug.LogError(
                "[ClassSelectManager] 클래스 선택 버튼 참조가 누락되었습니다."
            );

            return;
        }

        selectionCards = new[]
        {
            InitializeCard(
                physiqueButton,
                physiqueFrameSprite,
                new Color(0.48f, 0.10f, 0.10f, 0.94f)
            ),
            InitializeCard(
                technicianButton,
                technicianFrameSprite,
                new Color(0.05f, 0.36f, 0.18f, 0.94f)
            ),
            InitializeCard(
                captainButton,
                captainFrameSprite,
                new Color(0.28f, 0.10f, 0.48f, 0.94f)
            )
        };

        cardCenterPosition =
            selectionCards[1].DefaultPosition;
    }

    private ClassSelectionCardUI InitializeCard(
        Button button,
        Sprite frameSprite,
        Color fallbackColor)
    {
        ClassSelectionCardUI card =
            button.GetComponent<ClassSelectionCardUI>();

        if (card == null)
        {
            card = button.gameObject.AddComponent<ClassSelectionCardUI>();
        }

        card.Initialize(
            button,
            frameSprite,
            fallbackColor
        );

        return card;
    }

    private void ApplySelectedCardLayout(PlayerClass playerClass)
    {
        if (selectionCards == null || selectionCards.Length != 3)
        {
            return;
        }

        int selectedIndex = GetCardIndex(playerClass);

        if (selectedIndex < 0)
        {
            return;
        }

        for (int index = 0; index < selectionCards.Length; index++)
        {
            bool isSelected = index == selectedIndex;
            ClassSelectionCardUI card = selectionCards[index];

            if (!isSelected)
            {
                card.SetVisible(false);
                continue;
            }

            card.SetVisible(true);
            card.AnimateTo(
                cardCenterPosition,
                selectedCardSize,
                cardTransitionDuration,
                true
            );
        }
    }

    private void RestoreDefaultCardLayout()
    {
        if (selectionCards == null)
        {
            return;
        }

        foreach (ClassSelectionCardUI card in selectionCards)
        {
            card.SetVisible(true);
            card.RestoreSiblingOrder();
            card.AnimateTo(
                card.DefaultPosition,
                card.DefaultSize,
                cardTransitionDuration,
                false
            );
        }
    }

    private int GetCardIndex(PlayerClass playerClass)
    {
        switch (playerClass)
        {
            case PlayerClass.Physique:
                return 0;

            case PlayerClass.Technician:
                return 1;

            case PlayerClass.Captain:
                return 2;

            default:
                return -1;
        }
    }
}
