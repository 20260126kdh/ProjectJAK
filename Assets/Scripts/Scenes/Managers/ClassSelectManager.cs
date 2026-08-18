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
    private AspectRatioFitter classVideoAspectFitter;

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

        if (characterImage.GetComponent<RectMask2D>() == null)
        {
            characterImage.gameObject.AddComponent<RectMask2D>();
        }

        GameObject videoObject = new GameObject(
            "ClassVideo",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(RawImage),
            typeof(AspectRatioFitter)
        );

        RectTransform videoRect = videoObject.GetComponent<RectTransform>();
        videoRect.SetParent(characterImage.rectTransform, false);
        videoRect.anchorMin = new Vector2(0.5f, 0.5f);
        videoRect.anchorMax = new Vector2(0.5f, 0.5f);
        videoRect.anchoredPosition = Vector2.zero;
        videoRect.sizeDelta = characterImage.rectTransform.rect.size;

        classVideoImage = videoObject.GetComponent<RawImage>();
        classVideoImage.raycastTarget = false;
        classVideoImage.color = Color.white;

        classVideoAspectFitter = videoObject.GetComponent<AspectRatioFitter>();
        classVideoAspectFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;

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

        classVideoPlayer.Stop();
        classVideoImage.texture = null;
        classVideoPlayer.clip = videoClip;
        classVideoPlayer.Prepare();
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

    private void OnClassVideoPrepared(VideoPlayer preparedPlayer)
    {
        if (preparedPlayer.clip == null || classVideoImage == null)
        {
            return;
        }

        classVideoImage.texture = preparedPlayer.texture;

        if (classVideoAspectFitter != null && preparedPlayer.clip.height > 0)
        {
            classVideoAspectFitter.aspectRatio =
                (float)preparedPlayer.clip.width / preparedPlayer.clip.height;
        }

        preparedPlayer.Play();
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
