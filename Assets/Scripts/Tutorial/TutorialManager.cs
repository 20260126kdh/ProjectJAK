using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 첫 전투의 클래스별 튜토리얼 대화 UI와 입력 차단을 관리합니다.
/// </summary>
public sealed class TutorialManager : MonoBehaviour
{
    private const int TutorialSortingOrder = 300;

    [Header("클래스별 일반 초상화")]
    [SerializeField]
    private Sprite physiquePortrait;

    [SerializeField]
    private Sprite technicianPortrait;

    [SerializeField]
    private Sprite captainPortrait;

    [Header("클래스별 변화 초상화")]
    [SerializeField]
    private Sprite physiqueChangedPortrait;

    [SerializeField]
    private Sprite technicianChangedPortrait;

    [SerializeField]
    private Sprite captainChangedPortrait;

    [Header("튜토리얼 UI 이미지")]
    [SerializeField]
    private Sprite dialogueFrame;

    [SerializeField]
    private Sprite nextNormal;

    [SerializeField]
    private Sprite nextHover;

    [SerializeField]
    private Sprite nextPressed;

    [Header("튜토리얼 글꼴")]
    [SerializeField]
    private TMP_FontAsset dialogueFont;

    private readonly List<TutorialDialogueData> dialogueSteps =
        new List<TutorialDialogueData>();

    private GameObject tutorialRoot;
    private Image portraitImage;
    private TMP_Text dialogueText;
    private int currentDialogueIndex;
    private int lastDialogueAdvanceFrame = -1;
    private bool isTutorialActive;
    private bool isWaitingForCardUse;
    private bool isWaitingForPreserve;
    private bool isWaitingForPreserveModeStart;
    private bool isWaitingForNextTurn;
    private bool isFinalDialogueSequence;
    private bool isPreserveCardSelected;
    private string requiredCardID;
    private CardUI highlightedCardUI;
    private Coroutine highlightCoroutine;
    private Coroutine preserveConfirmHighlightCoroutine;
    private Coroutine endTurnHighlightCoroutine;
    private readonly List<TutorialTargetMarker> targetMarkers =
        new List<TutorialTargetMarker>();

    /// <summary>
    /// 현재 튜토리얼 대화가 입력을 차단하고 있는지 반환합니다.
    /// </summary>
    public bool IsTutorialActive => isTutorialActive;

    /// <summary>
    /// 카드 행동을 기다리는 단계까지 포함해 튜토리얼이 진행 중인지 반환합니다.
    /// </summary>
    public bool IsTutorialRunning =>
        isTutorialActive ||
        isWaitingForCardUse ||
        isWaitingForPreserve ||
        isWaitingForNextTurn;

    /// <summary>
    /// 현재 튜토리얼에서 보존 모드 진입을 허용하는지 반환합니다.
    /// </summary>
    public bool CanStartPreserve => isWaitingForPreserveModeStart;

    /// <summary>
    /// 현재 튜토리얼 보존 단계에서 E 입력을 처리할 수 있는지 반환합니다.
    /// </summary>
    public bool CanHandlePreserveInput => isWaitingForPreserve;

    /// <summary>
    /// 튜토리얼 보존 확정 뒤 턴 전환을 허용하는지 반환합니다.
    /// </summary>
    public bool CanEndTurnAfterPreserve => isWaitingForNextTurn;

    /// <summary>
    /// 현재 카드가 튜토리얼에서 허용된 카드인지 확인합니다.
    /// </summary>
    public bool CanSelectCard(CardData cardData, bool isPreserveMode)
    {
        if (!IsTutorialRunning)
        {
            return true;
        }

        if (cardData == null || cardData.cardID != requiredCardID)
        {
            return false;
        }

        return isWaitingForCardUse ||
               (isWaitingForPreserve &&
                !isWaitingForPreserveModeStart &&
                isPreserveMode);
    }

    private void Update()
    {
        if (isTutorialActive && Input.GetKeyDown(KeyCode.Space))
        {
            ShowNextDialogue();
        }
    }

    /// <summary>
    /// 새 게임 첫 전투의 클래스별 도입 튜토리얼을 시작합니다.
    /// 이어하기 경로에서는 호출하지 않습니다.
    /// </summary>
    public void BeginInitialTutorial()
    {
        if (isTutorialActive ||
            GameManager.Instance == null)
        {
            return;
        }

        PlayerClass playerClass =
            GameManager.Instance.PlayerData.PlayerClass;

        if (!BuildDialogueSteps(playerClass))
        {
            Debug.LogWarning(
                $"[TutorialManager] 지원하지 않는 클래스입니다: {playerClass}"
            );
            return;
        }

        EnsureTutorialUI();
        SetPortrait(playerClass);

        currentDialogueIndex = 0;
        isTutorialActive = true;
        tutorialRoot.SetActive(true);
        ShowCurrentDialogue();

        Debug.Log(
            $"[TutorialManager] {playerClass} 첫 전투 튜토리얼 시작"
        );
    }

    /// <summary>
    /// 다음 대사로 진행하고 현재 1단계 대화가 끝나면 전투 입력을 복구합니다.
    /// </summary>
    public void ShowNextDialogue()
    {
        if (!isTutorialActive ||
            lastDialogueAdvanceFrame == Time.frameCount)
        {
            return;
        }

        lastDialogueAdvanceFrame = Time.frameCount;

        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueSteps.Count)
        {
            BeginCardUseWait();
            return;
        }

        ShowCurrentDialogue();
    }

    private bool BuildDialogueSteps(PlayerClass playerClass)
    {
        dialogueSteps.Clear();

        switch (playerClass)
        {
            case PlayerClass.Captain:
                dialogueSteps.Add(new TutorialDialogueData(
                    "반가워! 너가 이번에 새로 온 견습이구나?\n" +
                    "양 옆에 얘네들? 아, 얘넨 엘리트 선원! 너도 열심히 하면 저렇게 될거야!"
                ));
                dialogueSteps.Add(new TutorialDialogueData(
                    "우선 전투의 기본부터 알려줄게!\n투창 카드를 사용해 적을 공격해봐!"
                    , "ALL_ATK_001"
                ));
                return true;

            case PlayerClass.Technician:
            case PlayerClass.Physique:
                dialogueSteps.Add(new TutorialDialogueData(
                    "자네가 내 배의 견습 선원인가? 우선, 전투의 기본을 가르쳐주마."
                ));
                dialogueSteps.Add(new TutorialDialogueData(
                    "투창 카드를 사용해 적을 공격해봐라."
                    , "ALL_ATK_001"
                ));
                return true;

            default:
                return false;
        }
    }

    private void EnsureTutorialUI()
    {
        if (tutorialRoot != null)
        {
            return;
        }

        tutorialRoot = new GameObject(
            "TutorialPanel",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );
        tutorialRoot.transform.SetParent(transform, false);

        Canvas canvas = tutorialRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = TutorialSortingOrder;

        CanvasScaler scaler = tutorialRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        CreateDimOverlay(tutorialRoot.transform);
        CreateDialogueFrame(tutorialRoot.transform);
        tutorialRoot.SetActive(false);
    }

    private void CreateDimOverlay(Transform parent)
    {
        GameObject dimObject = new GameObject(
            "DimOverlay",
            typeof(RectTransform),
            typeof(Image)
        );
        dimObject.transform.SetParent(parent, false);

        RectTransform rect = dimObject.GetComponent<RectTransform>();
        StretchFullScreen(rect);

        Image dimImage = dimObject.GetComponent<Image>();
        dimImage.color = new Color(0f, 0f, 0f, 0.4f);
        dimImage.raycastTarget = true;
    }

    private void CreateDialogueFrame(Transform parent)
    {
        GameObject frameObject = CreateImageObject(
            "DialogueFrame",
            parent,
            dialogueFrame
        );
        RectTransform frameRect = frameObject.GetComponent<RectTransform>();
        frameRect.anchorMin = new Vector2(0.05f, 0f);
        frameRect.anchorMax = new Vector2(0.95f, 0f);
        frameRect.pivot = new Vector2(0.5f, 0f);
        frameRect.anchoredPosition = new Vector2(0f, 4f);
        frameRect.sizeDelta = new Vector2(0f, 370f);

        GameObject portraitObject = CreateImageObject(
            "Portrait",
            parent,
            null
        );
        portraitImage = portraitObject.GetComponent<Image>();
        portraitImage.preserveAspect = true;
        portraitImage.raycastTarget = false;

        RectTransform portraitRect = portraitObject.GetComponent<RectTransform>();
        portraitRect.anchorMin = new Vector2(0f, 0f);
        portraitRect.anchorMax = new Vector2(0f, 0f);
        portraitRect.pivot = new Vector2(0f, 0f);
        portraitRect.anchoredPosition = new Vector2(0f, 0f);
        portraitRect.sizeDelta = new Vector2(560f, 760f);

        GameObject textObject = new GameObject(
            "DialogueText",
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );
        textObject.transform.SetParent(parent, false);
        dialogueText = textObject.GetComponent<TextMeshProUGUI>();
        dialogueText.font = dialogueFont;
        dialogueText.color = Color.white;
        dialogueText.fontSize = 36f;
        dialogueText.alignment = TextAlignmentOptions.Center;
        dialogueText.textWrappingMode =
            TextWrappingModes.Normal;
        dialogueText.raycastTarget = false;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.25f, 0f);
        textRect.anchorMax = new Vector2(0.88f, 0f);
        textRect.pivot = new Vector2(0.5f, 0f);
        textRect.anchoredPosition = new Vector2(0f, 75f);
        textRect.sizeDelta = new Vector2(0f, 210f);

        CreateNextButton(parent);
    }

    private void CreateNextButton(Transform parent)
    {
        GameObject buttonObject = new GameObject(
            "NextButton",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );
        buttonObject.transform.SetParent(parent, false);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.sprite = nextNormal;
        buttonImage.preserveAspect = true;

        Button button = buttonObject.GetComponent<Button>();
        button.transition = Selectable.Transition.SpriteSwap;
        SpriteState spriteState = button.spriteState;
        spriteState.highlightedSprite = nextHover;
        spriteState.pressedSprite = nextPressed;
        spriteState.selectedSprite = nextHover;
        button.spriteState = spriteState;
        button.onClick.AddListener(ShowNextDialogue);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.88f, 0f);
        buttonRect.anchorMax = new Vector2(0.88f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(0f, 64f);
        buttonRect.sizeDelta = new Vector2(110f, 110f);
    }

    private GameObject CreateImageObject(
        string objectName,
        Transform parent,
        Sprite sprite)
    {
        GameObject imageObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image)
        );
        imageObject.transform.SetParent(parent, false);

        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.raycastTarget = false;

        return imageObject;
    }

    private void SetPortrait(PlayerClass playerClass)
    {
        switch (playerClass)
        {
            case PlayerClass.Physique:
                portraitImage.sprite = physiquePortrait;
                break;

            case PlayerClass.Technician:
                portraitImage.sprite = technicianPortrait;
                break;

            case PlayerClass.Captain:
                portraitImage.sprite = captainPortrait;
                break;
        }
    }

    private void ShowCurrentDialogue()
    {
        if (isFinalDialogueSequence && currentDialogueIndex == 1)
        {
            SetChangedPortrait(
                GameManager.Instance.PlayerData.PlayerClass
            );
        }

        dialogueText.text =
            dialogueSteps[currentDialogueIndex].dialogue;
    }

    /// <summary>
    /// 성공적으로 사용된 카드가 현재 튜토리얼 조건과 일치하면
    /// 다음 설명 단계로 진행합니다.
    /// </summary>
    public void NotifyCardUsed(CardData usedCardData)
    {
        if (!isWaitingForCardUse ||
            usedCardData == null ||
            usedCardData.cardID != requiredCardID)
        {
            return;
        }

        StopCardHighlight();
        ClearTargetMarkers();
        isWaitingForCardUse = false;

        if (requiredCardID == "ALL_ATK_001")
        {
            ShowActionDialogue(
                "잘했네. 이번엔 회피 카드를 사용해 방어도를 올려봐라.",
                "ALL_DEF_001"
            );
            return;
        }

        if (requiredCardID == "ALL_DEF_001")
        {
            string skillCardID = GetFirstSkillCardID();
            ShowActionDialogue(
                GetFirstSkillInstruction(),
                skillCardID
            );
            return;
        }

        ShowPreserveInstructions();
    }

    private void ShowActionDialogue(
        string dialogue,
        string nextRequiredCardID)
    {
        dialogueSteps.Clear();
        dialogueSteps.Add(
            new TutorialDialogueData(
                dialogue,
                nextRequiredCardID
            )
        );
        currentDialogueIndex = 0;
        requiredCardID = null;
        isTutorialActive = true;
        tutorialRoot.SetActive(true);
        ShowCurrentDialogue();
    }

    private void BeginCardUseWait()
    {
        if (isFinalDialogueSequence)
        {
            CompleteTutorial();
            return;
        }

        if (isWaitingForPreserve)
        {
            BeginPreserveWait();
            return;
        }

        TutorialDialogueData completedStep =
            dialogueSteps[dialogueSteps.Count - 1];

        if (string.IsNullOrEmpty(completedStep.requiredCardID))
        {
            CloseDialogue();
            return;
        }

        requiredCardID = completedStep.requiredCardID;
        isTutorialActive = false;
        isWaitingForCardUse = true;
        tutorialRoot.SetActive(false);
        StartCardHighlight();
    }

    private void ShowPreserveInstructions()
    {
        dialogueSteps.Clear();

        PlayerClass playerClass =
            GameManager.Instance.PlayerData.PlayerClass;

        if (playerClass == PlayerClass.Captain)
        {
            dialogueSteps.Add(new TutorialDialogueData(
                "훗! 엘리트 선원의 힘을 너에게 줘봤어!\n" +
                "하지만 그렇게 많이 좋아졌다고 해서 한 번에 무리하면 안되지!\n" +
                "한 번 움직일 땐 적당히 두 번정도만 움직이는게 좋아!"
            ));
            dialogueSteps.Add(new TutorialDialogueData(
                "있잖아, 그렇게 우두커니 서서 멀리봐라 달라지는건 없다?\n" +
                "차라리 다음을 기억하면서 힘을 아끼는게 더 나은 걸?"
            ));
        }
        else
        {
            dialogueSteps.Add(new TutorialDialogueData(
                "이제야 좀 내 배의 선원답군.\n" +
                "하지만 그렇게 몸이 좋아졌다고 해서 한 번에 무리하면 쓰나.\n" +
                "한 번 움직일 땐 적당히 두 번정도만 움직여라."
            ));
            dialogueSteps.Add(new TutorialDialogueData(
                "그렇게 멍뗀다고 나아지는건 아무것도 없다.\n" +
                "다음을 기억하고 숨을 죽이는 것이 진짜 뱃사람이 가져야할 자세지."
            ));
        }

        currentDialogueIndex = 0;
        requiredCardID = GetSecondSkillCardID();
        isWaitingForPreserve = true;
        isTutorialActive = true;
        tutorialRoot.SetActive(true);
        ShowCurrentDialogue();
    }

    private void BeginPreserveWait()
    {
        isTutorialActive = false;
        tutorialRoot.SetActive(false);
        isWaitingForPreserveModeStart = true;
        endTurnHighlightCoroutine = StartCoroutine(
            BlinkEndTurnHighlightCoroutine()
        );
    }

    /// <summary>
    /// 턴 종료/보존 버튼으로 보존 모드에 진입했음을 알립니다.
    /// </summary>
    public void NotifyPreserveModeStarted()
    {
        if (!isWaitingForPreserveModeStart)
        {
            return;
        }

        isWaitingForPreserveModeStart = false;
        HandManager handManager = FindFirstObjectByType<HandManager>();
        StopEndTurnHighlight(handManager);
        StartCardHighlight();
    }

    /// <summary>
    /// 튜토리얼에서 지정한 보존 카드가 선택되었음을 알립니다.
    /// </summary>
    public void NotifyPreserveCardSelected(CardData cardData)
    {
        if (!isWaitingForPreserve ||
            cardData == null ||
            cardData.cardID != requiredCardID)
        {
            return;
        }

        isPreserveCardSelected = true;
        StopCardHighlight();
        preserveConfirmHighlightCoroutine = StartCoroutine(
            BlinkPreserveConfirmHighlightCoroutine()
        );
    }

    /// <summary>
    /// 현재 선택한 카드로 튜토리얼 보존 확정을 진행할 수 있는지 확인합니다.
    /// </summary>
    public bool CanConfirmPreserve(CardData cardData)
    {
        return isWaitingForPreserve &&
               isPreserveCardSelected &&
               cardData != null &&
               cardData.cardID == requiredCardID;
    }

    /// <summary>
    /// 튜토리얼 보존 확정 완료를 기록하고 다음 플레이어 턴을 기다립니다.
    /// </summary>
    public void NotifyPreserveConfirmed(CardData cardData)
    {
        if (!CanConfirmPreserve(cardData))
        {
            return;
        }

        HandManager handManager = FindFirstObjectByType<HandManager>();
        StopPreserveConfirmHighlight(handManager);

        isWaitingForPreserve = false;
        isWaitingForPreserveModeStart = false;
        isPreserveCardSelected = false;
        isWaitingForNextTurn = true;
        requiredCardID = null;
    }

    private IEnumerator BlinkEndTurnHighlightCoroutine()
    {
        bool visible = true;

        while (isWaitingForPreserveModeStart)
        {
            HandManager handManager = FindFirstObjectByType<HandManager>();
            handManager?.SetTutorialEndTurnHighlight(visible);
            visible = !visible;
            yield return new WaitForSeconds(1f);
        }
    }

    private void StopEndTurnHighlight(HandManager handManager)
    {
        if (endTurnHighlightCoroutine != null)
        {
            StopCoroutine(endTurnHighlightCoroutine);
            endTurnHighlightCoroutine = null;
        }

        handManager?.SetTutorialEndTurnHighlight(false);
    }

    /// <summary>
    /// 튜토리얼 카드가 선택되면 실제 적용 대상 머리 위에 표시를 생성합니다.
    /// </summary>
    public void NotifyCardSelected(CardData cardData)
    {
        ClearTargetMarkers();

        if (!isWaitingForCardUse ||
            cardData == null ||
            cardData.cardID != requiredCardID ||
            cardData.effects == null)
        {
            return;
        }

        bool requiresEnemy = false;
        bool requiresSelf = false;

        foreach (CardEffectData effect in cardData.effects)
        {
            requiresEnemy |= effect.target == CardTargetType.Enemy ||
                             effect.target == CardTargetType.AllEnemies;
            requiresSelf |= effect.target == CardTargetType.Self;
        }

        if (requiresEnemy)
        {
            EnemySpawner enemySpawner =
                FindFirstObjectByType<EnemySpawner>();

            if (enemySpawner != null)
            {
                List<Enemy> activeEnemies =
                    enemySpawner.GetActiveEnemies();

                if (activeEnemies.Count > 0)
                {
                    AddTargetMarker(activeEnemies[0].transform);
                }
            }
        }
        else if (requiresSelf)
        {
            PlayerCombat player = FindFirstObjectByType<PlayerCombat>();
            if (player != null)
            {
                AddTargetMarker(player.transform);
            }
        }
    }

    /// <summary>
    /// 카드 선택 해제 시 현재 대상 표시를 제거합니다.
    /// </summary>
    public void ClearTargetMarkers()
    {
        foreach (TutorialTargetMarker marker in targetMarkers)
        {
            if (marker != null)
            {
                Destroy(marker.gameObject);
            }
        }

        targetMarkers.Clear();
    }

    private void AddTargetMarker(Transform target)
    {
        GameObject markerObject = new GameObject("TutorialTargetMarker");
        TutorialTargetMarker marker =
            markerObject.AddComponent<TutorialTargetMarker>();
        marker.SetTarget(target);
        targetMarkers.Add(marker);
    }

    private IEnumerator BlinkPreserveConfirmHighlightCoroutine()
    {
        bool visible = true;

        while (isWaitingForPreserve && isPreserveCardSelected)
        {
            HandManager handManager = FindFirstObjectByType<HandManager>();
            handManager?.SetTutorialPreserveConfirmHighlight(visible);
            visible = !visible;
            yield return new WaitForSeconds(1f);
        }
    }

    private void StopPreserveConfirmHighlight(HandManager handManager)
    {
        if (preserveConfirmHighlightCoroutine != null)
        {
            StopCoroutine(preserveConfirmHighlightCoroutine);
            preserveConfirmHighlightCoroutine = null;
        }

        handManager?.SetTutorialPreserveConfirmHighlight(false);
    }

    /// <summary>
    /// 보존 뒤 다음 플레이어 턴이 시작되면 마무리 대사를 표시합니다.
    /// </summary>
    public void NotifyPlayerTurnStarted()
    {
        if (!isWaitingForNextTurn)
        {
            return;
        }

        isWaitingForNextTurn = false;
        isFinalDialogueSequence = true;
        BuildFinalDialogueSteps();
        currentDialogueIndex = 0;
        isTutorialActive = true;
        tutorialRoot.SetActive(true);
        SetPortrait(GameManager.Instance.PlayerData.PlayerClass);
        ShowCurrentDialogue();
    }

    private void StartCardHighlight()
    {
        HandManager handManager =
            FindFirstObjectByType<HandManager>();

        if (handManager == null)
        {
            Debug.LogError(
                "[TutorialManager] 강조할 손패를 찾지 못했습니다."
            );
            return;
        }

        foreach (CardUI cardUI in handManager.HandCardUIs)
        {
            if (cardUI == null ||
                cardUI.GetCardData() == null ||
                cardUI.GetCardData().cardID != requiredCardID)
            {
                continue;
            }

            highlightedCardUI = cardUI;
            highlightCoroutine = StartCoroutine(
                BlinkCardHighlightCoroutine(cardUI)
            );
            return;
        }

        Debug.LogError(
            $"[TutorialManager] 손패에 지정 카드가 없습니다: {requiredCardID}"
        );
    }

    private IEnumerator BlinkCardHighlightCoroutine(CardUI cardUI)
    {
        bool visible = true;

        while ((isWaitingForCardUse || isWaitingForPreserve) &&
               cardUI != null)
        {
            cardUI.SetTutorialHighlight(visible);
            visible = !visible;
            yield return new WaitForSeconds(1f);
        }
    }

    private void StopCardHighlight()
    {
        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
            highlightCoroutine = null;
        }

        if (highlightedCardUI != null)
        {
            highlightedCardUI.SetTutorialHighlight(false);
            highlightedCardUI = null;
        }
    }

    private string GetFirstSkillCardID()
    {
        PlayerClass playerClass =
            GameManager.Instance.PlayerData.PlayerClass;

        switch (playerClass)
        {
            case PlayerClass.Physique:
                return "PHY_SKL_001";
            case PlayerClass.Technician:
                return "TEC_SKL_001";
            case PlayerClass.Captain:
                return "CAP_SKL_001";
            default:
                return string.Empty;
        }
    }

    private string GetFirstSkillInstruction()
    {
        PlayerClass playerClass =
            GameManager.Instance.PlayerData.PlayerClass;

        switch (playerClass)
        {
            case PlayerClass.Captain:
                return "이제 스킬 카드를 사용해 적을 공격해봐!";
            case PlayerClass.Technician:
                return "이번엔 스킬 카드를 사용해 적을 공격해봐라.";
            case PlayerClass.Physique:
                return "이번엔 스킬 카드를 사용해 힘을 강화해봐라.";
            default:
                return string.Empty;
        }
    }

    private string GetSecondSkillCardID()
    {
        switch (GameManager.Instance.PlayerData.PlayerClass)
        {
            case PlayerClass.Physique:
                return "PHY_SKL_002";
            case PlayerClass.Technician:
                return "TEC_SKL_002";
            case PlayerClass.Captain:
                return "CAP_SKL_002";
            default:
                return string.Empty;
        }
    }

    private void BuildFinalDialogueSteps()
    {
        dialogueSteps.Clear();

        if (GameManager.Instance.PlayerData.PlayerClass == PlayerClass.Captain)
        {
            dialogueSteps.Add(new TutorialDialogueData(
                "이제야 좀 선원답네! 그러면 이제…"
            ));
            dialogueSteps.Add(new TutorialDialogueData(
                "내 배에 탄 걸 환영한다!\n우린, 더 큰놈들을 사냥할 것이다!"
            ));
            return;
        }

        dialogueSteps.Add(new TutorialDialogueData(
            "이제야 좀 뱃사람답군. 이제…"
        ));
        dialogueSteps.Add(new TutorialDialogueData(
            GameManager.Instance.PlayerData.PlayerClass == PlayerClass.Physique
                ? "내 배에 탄걸 환영한다, 꼬맹아.\n우린… 더 큰걸 잡을거다."
                : "내 배에 탄 걸 환영한다, 꼬맹아.\n우린… 더 큰걸 잡을거다."
        ));
    }

    private void SetChangedPortrait(PlayerClass playerClass)
    {
        switch (playerClass)
        {
            case PlayerClass.Physique:
                portraitImage.sprite = physiqueChangedPortrait;
                break;
            case PlayerClass.Technician:
                portraitImage.sprite = technicianChangedPortrait;
                break;
            case PlayerClass.Captain:
                portraitImage.sprite = captainChangedPortrait;
                break;
        }
    }

    private void CompleteTutorial()
    {
        isTutorialActive = false;
        isFinalDialogueSequence = false;
        ClearTargetMarkers();
        requiredCardID = null;
        tutorialRoot.SetActive(false);
        Debug.Log("[TutorialManager] 첫 전투 튜토리얼 완료");
    }

    private void CloseDialogue()
    {
        isTutorialActive = false;
        tutorialRoot.SetActive(false);

        Debug.Log(
            "[TutorialManager] 튜토리얼 1단계 대화 종료"
        );
    }

    private static void StretchFullScreen(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
