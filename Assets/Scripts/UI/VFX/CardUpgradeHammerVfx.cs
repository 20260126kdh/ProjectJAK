using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 선택 카드 위로 황금 모루가 수직 낙하하는 강화 연출을 재생합니다.
/// 실제 카드 강화 처리는 호출자가 충돌 콜백에서 수행합니다.
/// </summary>
public class CardUpgradeHammerVfx : MonoBehaviour
{
    [Header("황금 모루")]
    [SerializeField] private Sprite anvilSprite;
    [SerializeField] private Color anvilColor = Color.white;

    [Header("연출 시간")]
    [SerializeField] private float anticipationDuration = 0.2f;
    [SerializeField] private float hoverDuration = 0.18f;
    [SerializeField] private float slamDuration = 0.14f;
    [SerializeField] private float hitStopDuration = 0.1f;
    [SerializeField] private float burstDuration = 0.25f;
    [SerializeField] private float recoverDuration = 0.3f;

    [Header("색상")]
    [SerializeField] private Color anticipationColor = new Color(0.82f, 0.58f, 0.18f, 0.9f);
    [SerializeField] private Color impactColor = new Color(1f, 0.78f, 0.3f, 0.95f);
    [SerializeField] private Color metalFragmentColor = new Color(0.56f, 0.39f, 0.16f, 1f);

    private readonly List<RectTransform> cardFrameParts = new List<RectTransform>();
    private readonly List<RectTransform> burstRays = new List<RectTransform>();
    private readonly List<RectTransform> fragments = new List<RectTransform>();
    private RectTransform rootRect;
    private RectTransform anvilRect;
    private Image anvilImage;
    private RectTransform impactFlash;
    private Image impactFlashImage;
    private bool isPlaying;

    /// <summary>
    /// 지정한 카드를 향해 황금 모루 낙하 연출을 재생합니다.
    /// </summary>
    public void Play(RectTransform targetCard, Action onImpact = null, Action onCompleted = null)
    {
        if (isPlaying || targetCard == null)
        {
            return;
        }

        StartCoroutine(PlayCoroutine(targetCard, onImpact, onCompleted));
    }

    private IEnumerator PlayCoroutine(RectTransform targetCard, Action onImpact, Action onCompleted)
    {
        isPlaying = true;
        rootRect = transform as RectTransform;

        if (rootRect == null || anvilSprite == null)
        {
            Debug.LogError("[CardUpgradeHammerVfx] RectTransform 또는 황금 모루 Sprite가 없습니다.");
            onCompleted?.Invoke();
            Destroy(gameObject);
            yield break;
        }

        Vector3[] worldCorners = new Vector3[4];
        targetCard.GetWorldCorners(worldCorners);
        Vector2 bottomLeft = rootRect.InverseTransformPoint(worldCorners[0]);
        Vector2 topRight = rootRect.InverseTransformPoint(worldCorners[2]);
        Vector2 cardCenter = (bottomLeft + topRight) * 0.5f;
        Vector2 cardSize = new Vector2(Mathf.Abs(topRight.x - bottomLeft.x), Mathf.Abs(topRight.y - bottomLeft.y));
        Vector3 originalScale = targetCard.localScale;
        Vector3 originalPosition = targetCard.localPosition;

        CreateVisuals(cardCenter, cardSize);
        yield return AnimateAnticipation(targetCard, originalScale);

        Vector2 hoverPosition = cardCenter + Vector2.up * cardSize.y * 1.7f;
        Vector2 startPosition = hoverPosition + Vector2.up * cardSize.y * 0.45f;
        SetAnvilPose(startPosition, 0.82f);
        anvilImage.color = WithAlpha(anvilColor, 0f);

        yield return AnimateHover(startPosition, hoverPosition);
        yield return AnimateSlam(hoverPosition, cardCenter);

        onImpact?.Invoke();
        yield return AnimateHitStop(targetCard, originalScale, originalPosition);
        yield return AnimateBurst(targetCard, originalScale, originalPosition, cardCenter, cardSize);
        yield return AnimateRecover(targetCard, originalScale, originalPosition, cardCenter, cardSize);

        if (targetCard != null)
        {
            targetCard.localScale = originalScale;
            targetCard.localPosition = originalPosition;
        }

        isPlaying = false;
        onCompleted?.Invoke();
        Destroy(gameObject);
    }

    private void CreateVisuals(Vector2 center, Vector2 cardSize)
    {
        CreateCardFrame(center, cardSize);

        anvilRect = CreateImage("GoldenAnvil", anvilSprite, Color.clear, transform);
        anvilRect.pivot = new Vector2(0.5f, 0.08f);
        anvilImage = anvilRect.GetComponent<Image>();
        anvilImage.preserveAspect = true;
        float width = Mathf.Max(cardSize.x * 1.9f, 310f);
        float aspect = anvilSprite.rect.width / anvilSprite.rect.height;
        anvilRect.sizeDelta = new Vector2(width, width / Mathf.Max(0.01f, aspect));
        anvilRect.localRotation = Quaternion.identity;

        impactFlash = CreateImage("ImpactFlash", null, Color.clear, transform);
        impactFlash.anchoredPosition = center;
        impactFlash.sizeDelta = cardSize * 0.55f;
        impactFlash.localRotation = Quaternion.Euler(0f, 0f, 45f);
        impactFlashImage = impactFlash.GetComponent<Image>();

        for (int i = 0; i < 12; i++)
        {
            RectTransform ray = CreateImage($"ImpactRay_{i + 1}", null, Color.clear, transform);
            ray.anchoredPosition = center;
            ray.pivot = new Vector2(0.5f, 0f);
            ray.sizeDelta = new Vector2(7f + i % 3 * 2f, cardSize.y * 0.3f);
            ray.localRotation = Quaternion.Euler(0f, 0f, i * 30f);
            burstRays.Add(ray);
        }

        for (int i = 0; i < 10; i++)
        {
            RectTransform fragment = CreateImage($"MetalFragment_{i + 1}", null, Color.clear, transform);
            fragment.anchoredPosition = center;
            fragment.sizeDelta = new Vector2(5f + i % 3 * 2f, 12f + i % 4 * 3f);
            fragment.localRotation = Quaternion.Euler(0f, 0f, i * 61f);
            fragments.Add(fragment);
        }
    }

    private void CreateCardFrame(Vector2 center, Vector2 cardSize)
    {
        float thickness = Mathf.Max(5f, cardSize.x * 0.025f);
        Vector2 expanded = cardSize * 1.08f;
        Vector2[] sizes = { new Vector2(expanded.x, thickness), new Vector2(expanded.x, thickness), new Vector2(thickness, expanded.y), new Vector2(thickness, expanded.y) };
        Vector2[] offsets = { new Vector2(0f, expanded.y * 0.5f), new Vector2(0f, -expanded.y * 0.5f), new Vector2(-expanded.x * 0.5f, 0f), new Vector2(expanded.x * 0.5f, 0f) };

        for (int i = 0; i < 4; i++)
        {
            RectTransform framePart = CreateImage($"CardGlowFrame_{i + 1}", null, Color.clear, transform);
            framePart.anchoredPosition = center + offsets[i];
            framePart.sizeDelta = sizes[i];
            cardFrameParts.Add(framePart);
        }
    }

    private IEnumerator AnimateAnticipation(RectTransform targetCard, Vector3 originalScale)
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, anticipationDuration);
        while (elapsed < duration)
        {
            float progress = Mathf.Clamp01(elapsed / duration);
            targetCard.localScale = originalScale * Mathf.Lerp(1f, 1.05f, Smooth(progress));
            foreach (RectTransform framePart in cardFrameParts)
            {
                framePart.GetComponent<Image>().color = WithAlpha(anticipationColor, Mathf.Lerp(0.2f, 0.9f, progress));
            }
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private IEnumerator AnimateHover(Vector2 start, Vector2 end)
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, hoverDuration);
        while (elapsed < duration)
        {
            float progress = Mathf.Clamp01(elapsed / duration);
            SetAnvilPose(Vector2.Lerp(start, end, Smooth(progress)), Mathf.Lerp(0.82f, 1f, Smooth(progress)));
            anvilImage.color = WithAlpha(anvilColor, Mathf.Clamp01(progress * 4f));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private IEnumerator AnimateSlam(Vector2 start, Vector2 end)
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, slamDuration);
        while (elapsed < duration)
        {
            float progress = Mathf.Clamp01(elapsed / duration);
            float fall = progress * progress * progress;
            SetAnvilPose(Vector2.Lerp(start, end, fall), Mathf.Lerp(1f, 1.08f, fall));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        SetAnvilPose(end, 1.08f);
    }

    private IEnumerator AnimateHitStop(RectTransform targetCard, Vector3 originalScale, Vector3 originalPosition)
    {
        targetCard.localScale = Vector3.Scale(originalScale, new Vector3(1.1f, 0.76f, 1f));
        targetCard.localPosition = originalPosition + Vector3.down * 9f;
        impactFlashImage.color = impactColor;
        foreach (RectTransform framePart in cardFrameParts)
        {
            framePart.GetComponent<Image>().color = Color.clear;
        }

        float elapsed = 0f;
        while (elapsed < Mathf.Max(0.01f, hitStopDuration))
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private IEnumerator AnimateBurst(RectTransform targetCard, Vector3 originalScale, Vector3 originalPosition, Vector2 center, Vector2 cardSize)
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, burstDuration);
        while (elapsed < duration)
        {
            float progress = Mathf.Clamp01(elapsed / duration);
            float eased = Smooth(progress);
            float strength = 1f - progress;
            impactFlash.sizeDelta = Vector2.Lerp(cardSize * 0.7f, cardSize * 2f, eased);
            impactFlashImage.color = WithAlpha(impactColor, strength * 0.9f);

            for (int i = 0; i < burstRays.Count; i++)
            {
                RectTransform ray = burstRays[i];
                ray.sizeDelta = new Vector2(ray.sizeDelta.x, cardSize.y * Mathf.Lerp(0.2f, 1.25f + i % 3 * 0.1f, eased));
                ray.GetComponent<Image>().color = WithAlpha(impactColor, strength * 0.85f);
            }

            for (int i = 0; i < fragments.Count; i++)
            {
                float angle = (i * 137.5f + 20f) * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Abs(Mathf.Sin(angle)) * 0.7f + 0.15f);
                float distance = cardSize.x * (0.35f + i % 4 * 0.12f) * eased;
                fragments[i].anchoredPosition = center + direction * distance + Vector2.down * cardSize.y * 0.25f * progress * progress;
                fragments[i].GetComponent<Image>().color = WithAlpha(metalFragmentColor, strength);
            }

            float shake = strength * 10f;
            targetCard.localPosition = originalPosition + new Vector3(Mathf.Sin(progress * Mathf.PI * 12f) * shake, -strength * 8f, 0f);
            targetCard.localScale = Vector3.Scale(originalScale, new Vector3(1f + strength * 0.1f, 1f - strength * 0.24f, 1f));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private IEnumerator AnimateRecover(RectTransform targetCard, Vector3 originalScale, Vector3 originalPosition, Vector2 center, Vector2 cardSize)
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, recoverDuration);
        Vector2 anvilStart = anvilRect.anchoredPosition;
        Vector2 anvilEnd = center + Vector2.up * cardSize.y * 0.28f;
        while (elapsed < duration)
        {
            float progress = Mathf.Clamp01(elapsed / duration);
            float eased = Smooth(progress);
            SetAnvilPose(Vector2.Lerp(anvilStart, anvilEnd, eased), Mathf.Lerp(1.08f, 1f, eased));
            anvilImage.color = WithAlpha(anvilColor, 1f - eased);
            targetCard.localScale = Vector3.Lerp(targetCard.localScale, originalScale, eased);
            targetCard.localPosition = Vector3.Lerp(targetCard.localPosition, originalPosition, eased);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private void SetAnvilPose(Vector2 position, float scale)
    {
        anvilRect.anchoredPosition = position;
        anvilRect.localRotation = Quaternion.identity;
        anvilRect.localScale = Vector3.one * scale;
    }

    private RectTransform CreateImage(string objectName, Sprite sprite, Color color, Transform parent)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    private float Smooth(float value)
    {
        return value * value * (3f - 2f * value);
    }

    private Color WithAlpha(Color color, float alpha)
    {
        color.a = Mathf.Clamp01(alpha);
        return color;
    }
}
