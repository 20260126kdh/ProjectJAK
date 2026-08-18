using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 전투 턴 변경 문구를 두루마리 형태로 표시합니다.
/// </summary>
public class TurnBannerUI : MonoBehaviour
{
    private const float BannerWidth = 520f;
    private const float BannerHeight = 130f;
    private const int SortingOrder = 160;

    [Header("턴 배너 시간")]
    [SerializeField] private float unfoldDuration = 0.4f;
    [SerializeField] private float holdDuration = 0.5f;
    [SerializeField] private float foldDuration = 0.4f;

    private GameObject canvasObject;
    private RectTransform bannerRect;
    private TextMeshProUGUI bannerText;

    /// <summary>
    /// 현재 턴 배너가 입력을 차단하며 재생 중인지 반환합니다.
    /// </summary>
    public bool IsPlaying { get; private set; }

    /// <summary>
    /// 런타임 턴 배너 UI를 구성합니다.
    /// </summary>
    /// <param name="font">기존 전투 UI에서 사용할 TMP 폰트입니다.</param>
    public void Initialize(TMP_FontAsset font)
    {
        if (canvasObject != null)
        {
            return;
        }

        canvasObject = new GameObject(
            "TurnBannerCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = SortingOrder;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        CreateInputBlocker();
        CreateBanner(font);
        canvasObject.SetActive(false);
    }

    /// <summary>
    /// 지정한 턴 문구를 좌측에서 우측으로 펼쳤다가 접습니다.
    /// </summary>
    /// <param name="message">표시할 턴 문구입니다.</param>
    public IEnumerator PlayBanner(string message)
    {
        if (canvasObject == null || bannerRect == null)
        {
            yield break;
        }

        IsPlaying = true;
        canvasObject.SetActive(true);
        bannerText.text = message;
        bannerText.enabled = false;

        yield return AnimateWidth(0f, BannerWidth, unfoldDuration);

        bannerText.enabled = true;
        yield return new WaitForSecondsRealtime(holdDuration);

        bannerText.enabled = false;
        yield return AnimateWidth(BannerWidth, 0f, foldDuration);

        canvasObject.SetActive(false);
        IsPlaying = false;
    }

    private void CreateInputBlocker()
    {
        GameObject blockerObject = new GameObject(
            "TurnBannerInputBlocker",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        RectTransform blockerRect =
            blockerObject.GetComponent<RectTransform>();
        blockerRect.SetParent(canvasObject.transform, false);
        blockerRect.anchorMin = Vector2.zero;
        blockerRect.anchorMax = Vector2.one;
        blockerRect.offsetMin = Vector2.zero;
        blockerRect.offsetMax = Vector2.zero;

        Image blockerImage = blockerObject.GetComponent<Image>();
        blockerImage.color = new Color(0f, 0f, 0f, 0f);
        blockerImage.raycastTarget = true;
    }

    private void CreateBanner(TMP_FontAsset font)
    {
        GameObject bannerObject = new GameObject(
            "TurnBanner",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Outline)
        );
        bannerRect = bannerObject.GetComponent<RectTransform>();
        bannerRect.SetParent(canvasObject.transform, false);
        bannerRect.anchorMin = new Vector2(0.5f, 0.5f);
        bannerRect.anchorMax = new Vector2(0.5f, 0.5f);
        bannerRect.pivot = new Vector2(0f, 0.5f);
        bannerRect.anchoredPosition = new Vector2(
            -BannerWidth * 0.5f,
            0f
        );
        bannerRect.sizeDelta = new Vector2(0f, BannerHeight);

        Image background = bannerObject.GetComponent<Image>();
        background.color = new Color(0.88f, 0.64f, 0.43f, 1f);
        background.raycastTarget = false;

        Outline outline = bannerObject.GetComponent<Outline>();
        outline.effectColor = new Color(0.15f, 0.08f, 0.04f, 1f);
        outline.effectDistance = new Vector2(3f, -3f);

        CreateRoll("LeftRoll", bannerRect, false);
        CreateRoll("RightRoll", bannerRect, true);
        CreateText(font);
    }

    private void CreateRoll(
        string objectName,
        RectTransform parent,
        bool isRight)
    {
        GameObject rollObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Outline)
        );
        RectTransform rollRect =
            rollObject.GetComponent<RectTransform>();
        rollRect.SetParent(parent, false);
        float anchorX = isRight ? 1f : 0f;
        rollRect.anchorMin = new Vector2(anchorX, 0.5f);
        rollRect.anchorMax = new Vector2(anchorX, 0.5f);
        rollRect.pivot = new Vector2(anchorX, 0.5f);
        rollRect.anchoredPosition = Vector2.zero;
        rollRect.sizeDelta = new Vector2(24f, BannerHeight + 20f);

        Image rollImage = rollObject.GetComponent<Image>();
        rollImage.color = new Color(0.95f, 0.72f, 0.51f, 1f);
        rollImage.raycastTarget = false;

        Outline rollOutline = rollObject.GetComponent<Outline>();
        rollOutline.effectColor = new Color(0.15f, 0.08f, 0.04f, 1f);
        rollOutline.effectDistance = new Vector2(2f, -2f);
    }

    private void CreateText(TMP_FontAsset font)
    {
        GameObject textObject = new GameObject(
            "TurnBannerText",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        RectTransform textRect =
            textObject.GetComponent<RectTransform>();
        textRect.SetParent(bannerRect, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(40f, 10f);
        textRect.offsetMax = new Vector2(-40f, -10f);

        bannerText = textObject.GetComponent<TextMeshProUGUI>();
        if (font != null)
        {
            bannerText.font = font;
        }

        bannerText.fontSize = 64f;
        bannerText.fontWeight = FontWeight.Bold;
        bannerText.color = new Color(0.1f, 0.06f, 0.03f, 1f);
        bannerText.alignment = TextAlignmentOptions.Center;
        bannerText.raycastTarget = false;
    }

    private IEnumerator AnimateWidth(
        float startWidth,
        float endWidth,
        float duration)
    {
        if (duration <= 0f)
        {
            bannerRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                endWidth
            );
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float easedProgress = progress * progress *
                (3f - 2f * progress);
            bannerRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                Mathf.Lerp(startWidth, endWidth, easedProgress)
            );
            yield return null;
        }

        bannerRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            endWidth
        );
    }
}
