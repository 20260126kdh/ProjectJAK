using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 클래스 선택 카드의 프레임 표현과 위치·크기 전환을 관리합니다.
/// </summary>
public sealed class ClassSelectionCardUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private RectTransform rectTransform;
    private Image frameImage;
    private Image bannerImage;
    private AspectRatioFitter bannerAspectFitter;
    private Coroutine transitionCoroutine;
    private Coroutine hoverCoroutine;
    private Vector2 defaultPosition;
    private Vector2 defaultSize;
    private Vector3 defaultScale;
    private int defaultSiblingIndex;
    private Color accentColor;

    /// <summary>
    /// 카드의 기본 위치를 반환합니다.
    /// </summary>
    public Vector2 DefaultPosition => defaultPosition;

    /// <summary>
    /// 카드의 기본 크기를 반환합니다.
    /// </summary>
    public Vector2 DefaultSize => defaultSize;

    /// <summary>
    /// 기존 버튼을 클래스 선택 카드로 초기화합니다.
    /// </summary>
    /// <param name="button">카드 선택에 사용하는 버튼</param>
    /// <param name="frameSprite">카드 안에 표시할 클래스 배너 Sprite</param>
    /// <param name="fallbackColor">Sprite가 없을 때 사용할 임시 색상</param>
    /// <param name="className">하단 명판에 표시할 영문 클래스 이름</param>
    public void Initialize(
        Button button,
        Sprite frameSprite,
        Color fallbackColor,
        string className)
    {
        rectTransform = button.GetComponent<RectTransform>();
        frameImage = button.targetGraphic as Image;
        accentColor = fallbackColor;

        defaultPosition = rectTransform.anchoredPosition;
        defaultSize = rectTransform.sizeDelta;
        defaultScale = rectTransform.localScale;
        defaultSiblingIndex = rectTransform.GetSiblingIndex();

        if (frameImage == null)
        {
            frameImage = button.GetComponent<Image>();
        }

        if (frameImage == null)
        {
            return;
        }

        frameImage.sprite = null;
        frameImage.type = Image.Type.Simple;
        frameImage.color = new Color(1f, 1f, 1f, 0.01f);

        HideLegacyClassLabel(button);
        InitializeBanner(frameSprite, fallbackColor, className);
    }

    private static void HideLegacyClassLabel(Button button)
    {
        TMP_Text[] labels = button.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text label in labels)
        {
            string normalizedText = label.text.Trim().ToLowerInvariant();
            bool isLegacyClassLabel = normalizedText == "physique"
                || normalizedText == "technician"
                || normalizedText == "techanician"
                || normalizedText == "captain";

            if (isLegacyClassLabel)
            {
                label.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 클래스 배너를 카드 영역에 비율을 유지한 채 채우고 넘치는 부분은 자릅니다.
    /// 선택 카드가 전체 화면으로 확대될 때도 원본 이미지가 찌그러지지 않도록 사용합니다.
    /// </summary>
    /// <param name="bannerSprite">표시할 클래스 배너 Sprite</param>
    /// <param name="classColor">클래스 고유 색상</param>
    /// <param name="className">하단 명판에 표시할 영문 클래스 이름</param>
    private void InitializeBanner(
        Sprite bannerSprite,
        Color classColor,
        string className)
    {
        const float nameplateHeight = 72f;
        const float borderThickness = 4f;

        Transform existingViewport = transform.Find("BannerViewport");
        GameObject viewportObject = existingViewport != null
            ? existingViewport.gameObject
            : new GameObject(
                "BannerViewport",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Mask)
            );

        RectTransform viewportRect =
            viewportObject.GetComponent<RectTransform>();
        viewportRect.SetParent(transform, false);
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(0f, nameplateHeight);
        viewportRect.offsetMax = Vector2.zero;
        viewportRect.SetAsFirstSibling();

        Image viewportImage = viewportObject.GetComponent<Image>();
        viewportImage.color = new Color(0.015f, 0.018f, 0.022f, 1f);
        viewportImage.raycastTarget = false;

        Mask viewportMask = viewportObject.GetComponent<Mask>();
        viewportMask.showMaskGraphic = true;

        Transform existingBanner = viewportObject.transform.Find("BannerArtwork");
        GameObject bannerObject = existingBanner != null
            ? existingBanner.gameObject
            : new GameObject(
                "BannerArtwork",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(AspectRatioFitter)
            );

        RectTransform bannerRect =
            bannerObject.GetComponent<RectTransform>();
        bannerRect.SetParent(viewportObject.transform, false);
        bannerRect.anchorMin = new Vector2(0.5f, 0.5f);
        bannerRect.anchorMax = new Vector2(0.5f, 0.5f);
        bannerRect.anchoredPosition = Vector2.zero;
        bannerRect.sizeDelta = Vector2.zero;
        bannerRect.SetAsFirstSibling();

        bannerImage = bannerObject.GetComponent<Image>();
        bannerImage.sprite = bannerSprite;
        bannerImage.color = Color.white;
        bannerImage.raycastTarget = false;

        bannerAspectFitter = bannerObject.GetComponent<AspectRatioFitter>();
        bannerAspectFitter.aspectMode =
            AspectRatioFitter.AspectMode.EnvelopeParent;
        bannerAspectFitter.aspectRatio = bannerSprite != null
            ? bannerSprite.rect.width / bannerSprite.rect.height
            : 1f;

        bannerObject.SetActive(bannerSprite != null);

        Color borderColor = Color.Lerp(
            classColor,
            new Color(0.86f, 0.66f, 0.30f, 1f),
            0.58f
        );
        CreateBorder(viewportObject.transform, borderColor, borderThickness);
        CreateNameplate(classColor, borderColor, className, nameplateHeight);
    }

    private static void CreateBorder(
        Transform parent,
        Color borderColor,
        float thickness)
    {
        CreateBorderEdge("BorderTop", parent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, thickness), borderColor);
        CreateBorderEdge("BorderBottom", parent, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, thickness), borderColor);
        CreateBorderEdge("BorderLeft", parent, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(thickness, 0f), borderColor);
        CreateBorderEdge("BorderRight", parent, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(thickness, 0f), borderColor);
    }

    private static void CreateBorderEdge(
        string objectName,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 sizeDelta,
        Color color)
    {
        Transform existingEdge = parent.Find(objectName);
        GameObject edgeObject = existingEdge != null
            ? existingEdge.gameObject
            : new GameObject(
                objectName,
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
        edgeRect.SetAsLastSibling();

        Image edgeImage = edgeObject.GetComponent<Image>();
        edgeImage.color = color;
        edgeImage.raycastTarget = false;
    }

    private void CreateNameplate(
        Color classColor,
        Color borderColor,
        string className,
        float nameplateHeight)
    {
        Transform existingNameplate = transform.Find("ClassNameplate");
        GameObject nameplateObject = existingNameplate != null
            ? existingNameplate.gameObject
            : new GameObject(
                "ClassNameplate",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

        RectTransform nameplateRect =
            nameplateObject.GetComponent<RectTransform>();
        nameplateRect.SetParent(transform, false);
        nameplateRect.anchorMin = new Vector2(0f, 0f);
        nameplateRect.anchorMax = new Vector2(1f, 0f);
        nameplateRect.pivot = new Vector2(0.5f, 0f);
        nameplateRect.anchoredPosition = Vector2.zero;
        nameplateRect.sizeDelta = new Vector2(0f, nameplateHeight);

        Image nameplateImage = nameplateObject.GetComponent<Image>();
        nameplateImage.color = Color.Lerp(
            new Color(0.025f, 0.03f, 0.035f, 0.98f),
            classColor,
            0.22f
        );
        nameplateImage.raycastTarget = false;

        Outline nameplateOutline = nameplateObject.GetComponent<Outline>();
        if (nameplateOutline == null)
        {
            nameplateOutline = nameplateObject.AddComponent<Outline>();
        }
        nameplateOutline.effectColor = borderColor;
        nameplateOutline.effectDistance = new Vector2(2f, -2f);
        nameplateOutline.useGraphicAlpha = true;

        Transform existingLabel = nameplateObject.transform.Find("Label");
        GameObject labelObject = existingLabel != null
            ? existingLabel.gameObject
            : new GameObject(
                "Label",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI)
            );

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.SetParent(nameplateObject.transform, false);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = className;
        label.fontSize = 31f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(0.94f, 0.90f, 0.80f, 1f);
        label.raycastTarget = false;
    }

    /// <summary>
    /// 상세 화면 전환 중 카드의 표시 여부를 설정합니다.
    /// 선택하지 않은 카드가 전체 화면 배경 위로 노출되지 않도록 사용합니다.
    /// </summary>
    /// <param name="isVisible">카드를 표시할지 여부</param>
    public void SetVisible(bool isVisible)
    {
        if (!isVisible && transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        gameObject.SetActive(isVisible);
    }

    /// <summary>
    /// 카드가 초기화될 당시의 계층 순서로 복원합니다.
    /// </summary>
    public void RestoreSiblingOrder()
    {
        ResetHover();

        if (rectTransform != null)
        {
            rectTransform.SetSiblingIndex(defaultSiblingIndex);
        }
    }

    /// <summary>
    /// 포인터가 카드에 진입하면 카드 전체를 살짝 확대합니다.
    /// </summary>
    /// <param name="eventData">포인터 이벤트 정보</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.SetAsLastSibling();
        AnimateHoverScale(defaultScale * 1.035f);
    }

    /// <summary>
    /// 포인터가 카드에서 벗어나면 원래 크기로 되돌립니다.
    /// </summary>
    /// <param name="eventData">포인터 이벤트 정보</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        rectTransform.SetSiblingIndex(defaultSiblingIndex);
        AnimateHoverScale(defaultScale);
    }

    /// <summary>
    /// 클릭 또는 화면 전환 전에 카드의 호버 확대를 즉시 초기화합니다.
    /// </summary>
    public void ResetHover()
    {
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }

        if (rectTransform != null)
        {
            rectTransform.localScale = defaultScale;
            rectTransform.SetSiblingIndex(defaultSiblingIndex);
        }
    }

    private void AnimateHoverScale(Vector3 targetScale)
    {
        if (rectTransform == null || !gameObject.activeInHierarchy)
        {
            return;
        }

        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
        }

        hoverCoroutine = StartCoroutine(
            AnimateScale(targetScale, 0.15f)
        );
    }

    private IEnumerator AnimateScale(Vector3 targetScale, float duration)
    {
        Vector3 startScale = rectTransform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(elapsed / duration);
            float eased = normalized * normalized * (3f - 2f * normalized);
            rectTransform.localScale = Vector3.Lerp(
                startScale,
                targetScale,
                eased
            );
            yield return null;
        }

        rectTransform.localScale = targetScale;
        hoverCoroutine = null;
    }

    /// <summary>
    /// 카드를 지정한 위치와 크기로 부드럽게 전환합니다.
    /// </summary>
    /// <param name="targetPosition">목표 앵커 위치</param>
    /// <param name="targetSize">목표 크기</param>
    /// <param name="duration">전환 시간</param>
    /// <param name="isSelected">선택 카드 여부</param>
    public void AnimateTo(
        Vector2 targetPosition,
        Vector2 targetSize,
        float duration,
        bool isSelected)
    {
        if (rectTransform == null)
        {
            return;
        }

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine = StartCoroutine(
            AnimateRect(
                targetPosition,
                targetSize,
                duration,
                isSelected
            )
        );
    }

    private IEnumerator AnimateRect(
        Vector2 targetPosition,
        Vector2 targetSize,
        float duration,
        bool isSelected)
    {
        Vector2 startPosition = rectTransform.anchoredPosition;
        Vector2 startSize = rectTransform.sizeDelta;
        float elapsed = 0f;

        if (isSelected)
        {
            rectTransform.SetAsLastSibling();
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(elapsed / duration);
            float eased = normalized * normalized * (3f - 2f * normalized);

            rectTransform.anchoredPosition = Vector2.Lerp(
                startPosition,
                targetPosition,
                eased
            );
            rectTransform.sizeDelta = Vector2.Lerp(
                startSize,
                targetSize,
                eased
            );

            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
        rectTransform.sizeDelta = targetSize;

        if (frameImage != null && frameImage.sprite == null)
        {
            frameImage.color = isSelected
                ? Color.Lerp(accentColor, Color.white, 0.12f)
                : accentColor;
        }

        transitionCoroutine = null;
    }
}
