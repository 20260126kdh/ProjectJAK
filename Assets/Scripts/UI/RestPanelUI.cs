using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 휴식 단계에서 표시되는 Rest 패널을 관리합니다.
///
/// 기능:
/// - 최대 체력의 20% 회복
/// - 카드 강화 패널 진입
/// - 휴식 완료 후 보스 전투 시작
/// </summary>
public class RestPanelUI : MonoBehaviour
{
    private const int RestPanelSortingOrder = 150;

    [Header("Rest Panel")]
    [SerializeField]
    private GameObject restPanel;

    [Header("Battle Manager")]
    [SerializeField]
    private BattleManager battleManager;

    [Header("전투 손패")]
    [SerializeField]
    private GameObject handCardParent;

    [Header("Upgrade Panel UI")]
    [SerializeField]
    private UpgradePanelUI upgradePanelUI;

    [Header("휴식 배경")]
    [SerializeField] private Image restBackgroundImage;
    [SerializeField] private Sprite captainRestSprite;
    [SerializeField] private Sprite captainRestEndSprite;
    [SerializeField] private Sprite physiqueRestSprite;
    [SerializeField] private Sprite physiqueRestEndSprite;
    [SerializeField] private Sprite technicianRestSprite;
    [SerializeField] private Sprite technicianRestEndSprite;

    [Range(0f, 5f)]
    [SerializeField] private float restImageDuration = 1f;

    [Range(0f, 5f)]
    [SerializeField] private float restFadeDuration = 0.3f;

    [Range(0f, 5f)]
    [SerializeField] private float restEndHoldDuration = 0.5f;

    [Header("휴식 버튼")]
    [SerializeField]
    private Button restButton;

    [Header("강화 버튼")]
    [SerializeField]
    private Button upgradeButton;

    [Header("다음 전투 버튼")]
    [SerializeField]
    private Button nextBattleButton;

    [Header("휴식 설정")]
    [Range(0f, 1f)]
    [SerializeField]
    private float healRate = 0.2f;

    [Header("현재 휴식 상태")]
    [SerializeField]
    private bool hasRested;

    [SerializeField]
    private bool hasUpgraded;

    [SerializeField]
    private bool isMovingToNextBattle;

    private Coroutine restImageCoroutine;
    private Image restTransitionImage;

    private void Awake()
    {
        EnsureRestPanelSorting();
        CreateRestTransitionImage();
        HideRestImage();
        HideRestPanel();
    }

    /// <summary>
    /// 휴식 패널을 표시하고
    /// 이번 휴식 단계의 버튼 상태를 초기화합니다.
    /// </summary>
    public void ShowRestPanel()
    {
        if (restPanel == null)
        {
            Debug.LogError(
                "[RestPanelUI] Rest Panel이 연결되지 않았습니다."
            );

            return;
        }

        hasRested = false;
        hasUpgraded = false;
        isMovingToNextBattle = false;
        SetHandVisible(false);
        StopRestImageSequence();
        SetRestButtonsVisible(true);

        if (restButton != null)
        {
            restButton.interactable = true;
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable = true;
        }

        if (nextBattleButton != null)
        {
            nextBattleButton.interactable = true;
        }

        restPanel.SetActive(true);
        RefreshRestBackground(false);

        Debug.Log(
            $"[RestPanelUI] 휴식 패널 표시 / " +
            $"ActiveSelf: {restPanel.activeSelf} / " +
            $"ActiveInHierarchy: {restPanel.activeInHierarchy}"
        );
    }

    /// <summary>
    /// 휴식 패널을 숨깁니다.
    /// </summary>
    public void HideRestPanel()
    {
        if (restPanel == null)
        {
            return;
        }

        restPanel.SetActive(false);
    }

    /// <summary>
    /// 강화 패널에서 휴식 패널로 돌아옵니다.
    /// 기존 회복 및 강화 사용 상태는 초기화하지 않습니다.
    /// </summary>
    public void ReturnToRestPanel()
    {
        if (restPanel == null)
        {
            Debug.LogError(
                "[RestPanelUI] Rest Panel이 연결되지 않았습니다."
            );

            return;
        }

        restPanel.SetActive(true);
        RefreshRestBackground(hasRested);
        SetRestButtonsVisible(true);

        if (restButton != null)
        {
            restButton.interactable = !hasRested;
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable = !hasUpgraded;
        }

        if (nextBattleButton != null)
        {
            nextBattleButton.interactable =
                !isMovingToNextBattle;
        }

        Debug.Log("[RestPanelUI] 휴식 패널 복귀");
    }

    /// <summary>
    /// 이번 휴식 단계에서 카드 강화를 완료 처리합니다.
    /// </summary>
    public void CompleteUpgrade()
    {
        hasUpgraded = true;

        if (upgradeButton != null)
        {
            upgradeButton.interactable = false;
        }

        ReturnToRestPanel();

        Debug.Log("[RestPanelUI] 카드 강화 완료");
    }

    /// <summary>
    /// 휴식 버튼에서 호출합니다.
    /// 플레이어 최대 체력의 20%를 회복하고
    /// 휴식 버튼을 비활성화합니다.
    /// </summary>
    public void OnClickRest()
    {
        if (hasRested)
        {
            Debug.LogWarning(
                "[RestPanelUI] 이번 휴식 단계에서 이미 회복했습니다."
            );

            return;
        }

        if (GameManager.Instance == null ||
            GameManager.Instance.PlayerData == null)
        {
            Debug.LogError(
                "[RestPanelUI] PlayerData를 찾지 못했습니다."
            );

            return;
        }

        PlayerData playerData =
            GameManager.Instance.PlayerData;

        int healAmount =
            Mathf.FloorToInt(
                playerData.MaxHP * healRate
            );

        playerData.Heal(healAmount);

        hasRested = true;
        SetRestButtonsVisible(false);
        RefreshRestBackground(false);
        restImageCoroutine = StartCoroutine(
            PlayRestImageSequence()
        );

        if (restButton != null)
        {
            restButton.interactable = false;
        }

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayRestHeal();
        }

        Debug.Log(
            $"[RestPanelUI] 휴식 완료 / " +
            $"최대 체력의 {healRate * 100f}% 회복 / " +
            $"회복 시도량: {healAmount} / " +
            $"현재 체력: {playerData.CurrentHP}/{playerData.MaxHP}"
        );
    }

    /// <summary>
    /// 강화 패널을 표시한 뒤 휴식 패널을 숨깁니다.
    /// 두 패널은 Hierarchy에서 형제 오브젝트여야 합니다.
    /// </summary>
    public void OnClickUpgrade()
    {
        if (upgradePanelUI == null)
        {
            Debug.LogError(
                "[RestPanelUI] UpgradePanelUI가 연결되지 않았습니다."
            );

            return;
        }

        /*
         * 강화 패널을 먼저 활성화한 뒤
         * 휴식 패널을 숨깁니다.
         */
        upgradePanelUI.ShowPanel();

        HideRestPanel();

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayCardUpgrade();
        }

        Debug.Log(
            "[RestPanelUI] 카드 강화 패널 열기"
        );
    }

    /// <summary>
    /// 다음 전투 버튼에서 호출합니다.
    /// 휴식 단계를 완료하고 보스 전투를 시작합니다.
    /// </summary>
    public void OnClickNextBattle()
    {
        if (isMovingToNextBattle)
        {
            return;
        }

        if (battleManager == null)
        {
            Debug.LogError(
                "[RestPanelUI] BattleManager가 연결되지 않았습니다."
            );

            return;
        }

        if (StageManager.Instance == null)
        {
            Debug.LogError(
                "[RestPanelUI] StageManager.Instance가 없습니다."
            );

            return;
        }

        if (StageManager.Instance.CurrentPhase !=
            StagePhase.Rest)
        {
            Debug.LogWarning(
                $"[RestPanelUI] 현재 휴식 단계가 아닙니다. " +
                $"현재 단계: {StageManager.Instance.CurrentPhase}"
            );

            return;
        }

        isMovingToNextBattle = true;

        if (restButton != null)
        {
            restButton.interactable = false;
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable = false;
        }

        if (nextBattleButton != null)
        {
            nextBattleButton.interactable = false;
        }

        /*
        * 먼저 StageManager를 보스 전투 단계로 변경해야
        * BattleManager가 보스 전투 데이터를 가져올 수 있습니다.
        */
        StageManager.Instance.RestComplete();

        HideRestPanel();

        SetHandVisible(true);
        battleManager.StartNextBattle();

        Debug.Log(
            "[RestPanelUI] 휴식 완료 - 보스 전투 시작"
        );
    }

    /// <summary>
    /// 현재 직업과 휴식 완료 여부에 맞는 배경 이미지를 표시합니다.
    /// </summary>
    private void RefreshRestBackground(bool showCompleted)
    {
        if (restBackgroundImage == null ||
            GameManager.Instance == null ||
            GameManager.Instance.PlayerData == null)
        {
            Debug.LogWarning(
                "[RestPanelUI] 휴식 배경 또는 PlayerData가 연결되지 않았습니다."
            );
            return;
        }

        PlayerClass playerClass =
            GameManager.Instance.PlayerData.PlayerClass;
        Sprite targetSprite = GetRestSprite(
            playerClass,
            showCompleted
        );

        if (targetSprite == null)
        {
            Debug.LogWarning(
                $"[RestPanelUI] {playerClass} 직업의 휴식 배경이 없습니다."
            );
            return;
        }

        restBackgroundImage.sprite = targetSprite;
        restBackgroundImage.color = Color.white;
        restBackgroundImage.enabled = true;
    }

    /// <summary>
    /// 기본 휴식 이미지를 보여준 뒤 완료 이미지와 버튼을 표시합니다.
    /// </summary>
    private IEnumerator PlayRestImageSequence()
    {
        yield return new WaitForSecondsRealtime(restImageDuration);

        yield return FadeToRestEndImage();
        yield return new WaitForSecondsRealtime(restEndHoldDuration);

        SetRestButtonsVisible(true);
        restImageCoroutine = null;
    }

    private void SetRestButtonsVisible(bool isVisible)
    {
        if (restButton != null)
        {
            restButton.gameObject.SetActive(isVisible);
        }

        if (upgradeButton != null)
        {
            upgradeButton.gameObject.SetActive(isVisible);
        }

        if (nextBattleButton != null)
        {
            nextBattleButton.gameObject.SetActive(isVisible);
        }
    }

    private void HideRestImage()
    {
        if (restBackgroundImage != null)
        {
            restBackgroundImage.enabled = false;
        }

        if (restTransitionImage != null)
        {
            restTransitionImage.enabled = false;
        }
    }

    private Sprite GetRestSprite(
        PlayerClass playerClass,
        bool showCompleted
    )
    {
        switch (playerClass)
        {
            case PlayerClass.Captain:
                return showCompleted
                    ? captainRestEndSprite
                    : captainRestSprite;
            case PlayerClass.Physique:
                return showCompleted
                    ? physiqueRestEndSprite
                    : physiqueRestSprite;
            case PlayerClass.Technician:
                return showCompleted
                    ? technicianRestEndSprite
                    : technicianRestSprite;
            default:
                return null;
        }
    }

    private IEnumerator FadeToRestEndImage()
    {
        if (restBackgroundImage == null ||
            restTransitionImage == null ||
            GameManager.Instance == null ||
            GameManager.Instance.PlayerData == null)
        {
            RefreshRestBackground(true);
            yield break;
        }

        Sprite endSprite = GetRestSprite(
            GameManager.Instance.PlayerData.PlayerClass,
            true
        );
        if (endSprite == null)
        {
            yield break;
        }

        restTransitionImage.sprite = null;
        restTransitionImage.color = new Color(0f, 0f, 0f, 0f);
        restTransitionImage.enabled = true;

        float elapsed = 0f;
        while (elapsed < restFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = restFadeDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / restFadeDuration);

            restTransitionImage.color = new Color(
                0f,
                0f,
                0f,
                progress
            );
            yield return null;
        }

        restBackgroundImage.sprite = endSprite;
        restBackgroundImage.color = Color.white;

        elapsed = 0f;
        while (elapsed < restFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = restFadeDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / restFadeDuration);

            restTransitionImage.color = new Color(
                0f,
                0f,
                0f,
                1f - progress
            );
            yield return null;
        }

        restTransitionImage.enabled = false;
    }

    private void CreateRestTransitionImage()
    {
        if (restPanel == null || restTransitionImage != null)
        {
            return;
        }

        GameObject transitionObject = new GameObject(
            "RestTransitionImage",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        RectTransform transitionRect =
            transitionObject.GetComponent<RectTransform>();
        transitionRect.SetParent(restPanel.transform, false);
        transitionRect.anchorMin = Vector2.zero;
        transitionRect.anchorMax = Vector2.one;
        transitionRect.anchoredPosition = Vector2.zero;
        transitionRect.sizeDelta = Vector2.zero;
        transitionRect.SetAsFirstSibling();

        restTransitionImage = transitionObject.GetComponent<Image>();
        restTransitionImage.raycastTarget = false;
        restTransitionImage.enabled = false;
    }

    private void StopRestImageSequence()
    {
        if (restImageCoroutine == null)
        {
            return;
        }

        StopCoroutine(restImageCoroutine);
        restImageCoroutine = null;

        if (restTransitionImage != null)
        {
            restTransitionImage.enabled = false;
        }

        if (restBackgroundImage != null)
        {
            restBackgroundImage.color = Color.white;
        }
    }

    private void EnsureRestPanelSorting()
    {
        if (restPanel == null)
        {
            return;
        }

        Canvas restCanvas = restPanel.GetComponent<Canvas>();
        if (restCanvas == null)
        {
            restCanvas = restPanel.AddComponent<Canvas>();
        }

        restCanvas.overrideSorting = true;
        restCanvas.sortingOrder = RestPanelSortingOrder;

        if (restPanel.GetComponent<GraphicRaycaster>() == null)
        {
            restPanel.AddComponent<GraphicRaycaster>();
        }
    }

    private void SetHandVisible(bool isVisible)
    {
        if (handCardParent != null)
        {
            handCardParent.SetActive(isVisible);
        }
    }
}
