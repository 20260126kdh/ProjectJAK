using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 클래스 선택 카드의 프레임 표현과 위치·크기 전환을 관리합니다.
/// </summary>
public sealed class ClassSelectionCardUI : MonoBehaviour
{
    private RectTransform rectTransform;
    private Image frameImage;
    private Coroutine transitionCoroutine;
    private Vector2 defaultPosition;
    private Vector2 defaultSize;
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
    /// <param name="frameSprite">추후 교체할 9-slice 프레임 Sprite</param>
    /// <param name="fallbackColor">Sprite가 없을 때 사용할 임시 색상</param>
    public void Initialize(
        Button button,
        Sprite frameSprite,
        Color fallbackColor)
    {
        rectTransform = button.GetComponent<RectTransform>();
        frameImage = button.targetGraphic as Image;
        accentColor = fallbackColor;

        defaultPosition = rectTransform.anchoredPosition;
        defaultSize = rectTransform.sizeDelta;

        if (frameImage == null)
        {
            frameImage = button.GetComponent<Image>();
        }

        if (frameImage == null)
        {
            return;
        }

        frameImage.sprite = frameSprite;
        frameImage.type = frameSprite == null
            ? Image.Type.Simple
            : Image.Type.Sliced;
        frameImage.color = frameSprite == null
            ? fallbackColor
            : Color.white;
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
