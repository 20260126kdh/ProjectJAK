using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 버림 더미의 카드가 물살에 섞여 뽑을 더미로 재구성되는
/// 셔플 전용 연출을 생성하고 재생합니다.
/// </summary>
public class ShuffleTransferVfx : MonoBehaviour
{
    [Header("연출 시간")]
    [SerializeField]
    private float gatherDuration = 0.35f;

    [SerializeField]
    private float shuffleDuration = 0.4f;

    [SerializeField]
    private float transferDuration = 0.55f;

    [SerializeField]
    private float rebuildDuration = 0.3f;

    [Header("Water Flow")]
    [SerializeField]
    private Material waterTrailMaterial;

    [SerializeField]
    [Min(0.5f)]
    private float riseHeightMultiplier = 6f;

    [SerializeField]
    [Range(0.2f, 0.6f)]
    private float riseDurationPortion = 0.4f;

    [SerializeField]
    [Min(0.5f)]
    private float visualScale = 1.35f;

    [Header("색상")]
    [SerializeField]
    private Color deepWaterColor = new Color(0.02f, 0.34f, 0.48f, 0.9f);

    [SerializeField]
    private Color waterColor = new Color(0.05f, 0.78f, 0.92f, 1f);

    [SerializeField]
    private Color highlightColor = new Color(0.75f, 0.98f, 1f, 1f);

    private readonly List<LineRenderer> createdLines =
        new List<LineRenderer>();
    private Material accentMaterial;
    private bool isPlaying;

    /// <summary>
    /// 전체 셔플 연출 시간을 반환합니다.
    /// </summary>
    public float TotalDuration =>
        Mathf.Max(0f, gatherDuration) +
        Mathf.Max(0f, shuffleDuration) +
        Mathf.Max(0f, transferDuration) +
        Mathf.Max(0f, rebuildDuration);

    /// <summary>
    /// 지정한 시작점에서 도착점까지 셔플 연출을 재생합니다.
    /// </summary>
    public void Play(
        Vector3 startPosition,
        Vector3 endPosition,
        Action onCompleted = null)
    {
        if (isPlaying)
        {
            return;
        }

        StartCoroutine(
            PlayCoroutine(startPosition, endPosition, onCompleted)
        );
    }

    private IEnumerator PlayCoroutine(
        Vector3 startPosition,
        Vector3 endPosition,
        Action onCompleted)
    {
        isPlaying = true;
        CreateRuntimeMaterial();

        float distance = Vector3.Distance(startPosition, endPosition);
        float unit = Mathf.Clamp(distance * 0.055f, 0.12f, 0.32f);
        float visualUnit = unit * Mathf.Max(0.5f, visualScale);

        LineRenderer outerRing = CreateLine("GatherOuterRing", 0.08f, deepWaterColor, 20);
        LineRenderer innerRing = CreateLine("GatherInnerRing", 0.035f, highlightColor, 21);
        List<LineRenderer> cardGhosts = CreateCardGhosts(5, visualUnit);

        yield return AnimateGather(
            startPosition,
            visualUnit,
            outerRing,
            innerRing,
            cardGhosts
        );

        yield return AnimateShuffle(startPosition, visualUnit, cardGhosts);

        outerRing.gameObject.SetActive(false);
        innerRing.gameObject.SetActive(false);

        LineRenderer ribbonGlow = CreateLine("RibbonGlow", visualUnit * 0.82f, WithAlpha(deepWaterColor, 0.58f), 18, true);
        LineRenderer ribbonCore = CreateLine("RibbonCore", visualUnit * 0.42f, waterColor, 20, true);
        LineRenderer ribbonHighlight = CreateLine("RibbonHighlight", visualUnit * 0.08f, highlightColor, 22);
        LineRenderer upperRipple = CreateLine("UpperRipple", visualUnit * 0.11f, WithAlpha(waterColor, 0.72f), 21, true);
        LineRenderer lowerRipple = CreateLine("LowerRipple", visualUnit * 0.07f, WithAlpha(highlightColor, 0.55f), 21, true);
        LineRenderer headRing = CreateLine("TransferHead", visualUnit * 0.11f, highlightColor, 24);
        List<LineRenderer> waterDrops = CreateWaterDrops(5, visualUnit);

        yield return AnimateTransfer(
            startPosition,
            endPosition,
            unit,
            visualUnit,
            ribbonGlow,
            ribbonCore,
            ribbonHighlight,
            upperRipple,
            lowerRipple,
            headRing,
            waterDrops,
            cardGhosts
        );

        ribbonGlow.gameObject.SetActive(false);
        ribbonCore.gameObject.SetActive(false);
        ribbonHighlight.gameObject.SetActive(false);
        upperRipple.gameObject.SetActive(false);
        lowerRipple.gameObject.SetActive(false);
        headRing.gameObject.SetActive(false);

        foreach (LineRenderer waterDrop in waterDrops)
        {
            waterDrop.gameObject.SetActive(false);
        }

        yield return AnimateRebuild(
            endPosition,
            visualUnit,
            outerRing,
            innerRing,
            cardGhosts
        );

        isPlaying = false;
        onCompleted?.Invoke();
        Destroy(gameObject);
    }

    private IEnumerator AnimateGather(
        Vector3 center,
        float unit,
        LineRenderer outerRing,
        LineRenderer innerRing,
        List<LineRenderer> cards)
    {
        float duration = Mathf.Max(0f, gatherDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = duration > 0f
                ? Mathf.Clamp01(elapsed / duration)
                : 1f;
            float eased = Smooth(progress);

            SetCircle(outerRing, center, Mathf.Lerp(unit * 2.4f, unit * 0.65f, eased), 40);
            SetCircle(innerRing, center, Mathf.Lerp(unit * 1.65f, unit * 0.35f, eased), 40);

            for (int i = 0; i < cards.Count; i++)
            {
                float angle =
                    i * (360f / cards.Count) + progress * 210f;
                Vector3 position = center + Direction(angle) *
                    Mathf.Lerp(unit * 2.2f, unit * 0.5f, eased);
                SetCardOutline(cards[i], position, unit, angle + 18f, 1f - eased * 0.35f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator AnimateShuffle(
        Vector3 center,
        float unit,
        List<LineRenderer> cards)
    {
        float duration = Mathf.Max(0f, shuffleDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = duration > 0f
                ? Mathf.Clamp01(elapsed / duration)
                : 1f;
            float pulse = 0.75f + Mathf.Sin(progress * Mathf.PI * 4f) * 0.16f;

            for (int i = 0; i < cards.Count; i++)
            {
                float lane = (i - (cards.Count - 1) * 0.5f) * unit * 0.34f;
                float crossing = Mathf.Sin(progress * Mathf.PI * 2f + i * 1.3f);
                Vector3 position = center +
                    new Vector3(crossing * unit * 0.85f, lane, 0f);
                SetCardOutline(cards[i], position, unit, progress * 540f + i * 24f, pulse);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator AnimateTransfer(
        Vector3 start,
        Vector3 end,
        float unit,
        float visualUnit,
        LineRenderer glow,
        LineRenderer core,
        LineRenderer highlight,
        LineRenderer upperRipple,
        LineRenderer lowerRipple,
        LineRenderer head,
        List<LineRenderer> waterDrops,
        List<LineRenderer> cards)
    {
        float duration = Mathf.Max(0f, transferDuration);
        float elapsed = 0f;
        Vector3 raisedStart = start + Vector3.up * unit * riseHeightMultiplier;
        Vector3 horizontalDirection = end - raisedStart;
        Vector3 horizontalNormal = new Vector3(
            -horizontalDirection.y,
            horizontalDirection.x,
            0f
        ).normalized;
        Vector3 controlA = new Vector3(
            Mathf.Lerp(raisedStart.x, end.x, 0.38f),
            raisedStart.y + unit * 0.8f,
            raisedStart.z
        );
        Vector3 controlB = new Vector3(
            Mathf.Lerp(raisedStart.x, end.x, 0.82f),
            raisedStart.y + unit * 0.15f,
            raisedStart.z
        ) + horizontalNormal * unit * 0.2f;

        while (elapsed < duration)
        {
            float progress = duration > 0f
                ? Mathf.Clamp01(elapsed / duration)
                : 1f;
            float headProgress = Smooth(progress);
            float tailProgress = Mathf.Max(0f, headProgress - 0.3f);

            SetWaterRoute(glow, start, raisedStart, controlA, controlB, end, tailProgress, headProgress, 34);
            SetWaterRoute(core, start, raisedStart, controlA, controlB, end, tailProgress, headProgress, 34);
            SetWaterRoute(highlight, start, raisedStart, controlA, controlB, end, Mathf.Lerp(tailProgress, headProgress, 0.35f), headProgress, 22);
            SetOffsetWaterRoute(upperRipple, start, raisedStart, controlA, controlB, end, tailProgress, headProgress, visualUnit * 0.16f, progress * 11f, 28);
            SetOffsetWaterRoute(lowerRipple, start, raisedStart, controlA, controlB, end, tailProgress, headProgress, -visualUnit * 0.13f, progress * 13f + 1.7f, 28);

            Vector3 headPosition = WaterRoute(start, raisedStart, controlA, controlB, end, headProgress);
            SetCircle(head, headPosition, visualUnit * (0.34f + Mathf.Sin(progress * Mathf.PI * 6f) * 0.05f), 24);
            AnimateWaterDrops(
                waterDrops,
                start,
                raisedStart,
                controlA,
                controlB,
                end,
                headProgress,
                progress,
                visualUnit
            );

            for (int i = 0; i < cards.Count; i++)
            {
                float cardProgress = Mathf.Clamp01(headProgress - 0.04f * i);
                Vector3 cardPosition = WaterRoute(start, raisedStart, controlA, controlB, end, cardProgress);
                SetCardOutline(cards[i], cardPosition, visualUnit * 0.72f, progress * 420f + i * 17f, 0.72f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator AnimateRebuild(
        Vector3 center,
        float unit,
        LineRenderer outerRing,
        LineRenderer innerRing,
        List<LineRenderer> cards)
    {
        outerRing.gameObject.SetActive(true);
        innerRing.gameObject.SetActive(true);

        float duration = Mathf.Max(0f, rebuildDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = duration > 0f
                ? Mathf.Clamp01(elapsed / duration)
                : 1f;
            float eased = Smooth(progress);
            float flash = Mathf.Sin(progress * Mathf.PI);

            SetCircle(outerRing, center, Mathf.Lerp(unit * 0.15f, unit * 1.45f, eased), 40);
            SetCircle(innerRing, center, Mathf.Lerp(unit * 0.08f, unit * 0.85f, eased), 40);
            outerRing.startColor = outerRing.endColor = WithAlpha(waterColor, 1f - eased);
            innerRing.startColor = innerRing.endColor = WithAlpha(highlightColor, flash);

            for (int i = 0; i < cards.Count; i++)
            {
                Vector3 spread = new Vector3(
                    (i - 2f) * unit * 0.2f,
                    Mathf.Abs(i - 2f) * unit * 0.09f,
                    0f
                );
                Vector3 position = Vector3.Lerp(center + spread, center, eased);
                SetCardOutline(cards[i], position, unit * 0.68f, Mathf.Lerp(i * 18f, 0f, eased), 1f - eased * 0.45f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private List<LineRenderer> CreateCardGhosts(int count, float unit)
    {
        List<LineRenderer> cards = new List<LineRenderer>();

        for (int i = 0; i < count; i++)
        {
            Color color = Color.Lerp(waterColor, highlightColor, i / (float)Mathf.Max(1, count - 1));
            cards.Add(CreateLine($"CardGhost_{i + 1}", unit * 0.055f, WithAlpha(color, 0.72f), 23));
        }

        return cards;
    }

    private List<LineRenderer> CreateWaterDrops(int count, float unit)
    {
        List<LineRenderer> drops = new List<LineRenderer>();

        for (int i = 0; i < count; i++)
        {
            float width = unit * Mathf.Lerp(0.065f, 0.11f, i / (float)count);
            drops.Add(
                CreateLine(
                    $"WaterDrop_{i + 1}",
                    width,
                    WithAlpha(waterColor, 0.8f),
                    23,
                    true
                )
            );
        }

        return drops;
    }

    private void AnimateWaterDrops(
        List<LineRenderer> drops,
        Vector3 start,
        Vector3 raisedStart,
        Vector3 controlA,
        Vector3 controlB,
        Vector3 end,
        float headProgress,
        float animationProgress,
        float unit)
    {
        for (int i = 0; i < drops.Count; i++)
        {
            float delay = 0.055f + i * 0.038f;
            float dropProgress = Mathf.Clamp01(headProgress - delay);
            Vector3 position = WaterRoute(
                start,
                raisedStart,
                controlA,
                controlB,
                end,
                dropProgress
            );
            float side = i % 2 == 0 ? 1f : -1f;
            float flutter = Mathf.Sin(animationProgress * 18f + i * 1.9f);
            position += new Vector3(
                side * unit * (0.2f + i * 0.035f),
                -unit * (0.1f + Mathf.Abs(flutter) * 0.22f),
                0f
            );
            SetCircle(
                drops[i],
                position,
                unit * (0.07f + i * 0.008f),
                10
            );
        }
    }

    private LineRenderer CreateLine(
        string objectName,
        float width,
        Color color,
        int sortingOrder,
        bool useWaterMaterial = false)
    {
        GameObject lineObject = new GameObject(objectName);
        lineObject.transform.SetParent(transform, false);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.loop = false;
        line.numCapVertices = 5;
        line.numCornerVertices = 5;
        line.widthMultiplier = Mathf.Max(0.001f, width);
        line.sharedMaterial = useWaterMaterial && waterTrailMaterial != null
            ? waterTrailMaterial
            : accentMaterial;
        line.startColor = color;
        line.endColor = color;
        line.sortingOrder = sortingOrder;
        createdLines.Add(line);
        return line;
    }

    private void SetCircle(
        LineRenderer line,
        Vector3 center,
        float radius,
        int segments)
    {
        line.loop = true;
        line.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            line.SetPosition(
                i,
                center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius
            );
        }
    }

    private void SetCardOutline(
        LineRenderer line,
        Vector3 center,
        float unit,
        float angle,
        float scale)
    {
        float halfWidth = unit * 0.42f * scale;
        float halfHeight = unit * 0.62f * scale;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        Vector3[] corners =
        {
            new Vector3(-halfWidth, -halfHeight, 0f),
            new Vector3(-halfWidth, halfHeight, 0f),
            new Vector3(halfWidth, halfHeight, 0f),
            new Vector3(halfWidth, -halfHeight, 0f),
            new Vector3(-halfWidth, -halfHeight, 0f)
        };

        line.loop = false;
        line.positionCount = corners.Length;

        for (int i = 0; i < corners.Length; i++)
        {
            line.SetPosition(i, center + rotation * corners[i]);
        }
    }

    private void SetRibbon(
        LineRenderer line,
        Vector3 start,
        Vector3 controlA,
        Vector3 controlB,
        Vector3 end,
        float from,
        float to,
        int segments)
    {
        line.loop = false;
        line.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float normalized = i / (float)(segments - 1);
            float progress = Mathf.Lerp(from, to, normalized);
            line.SetPosition(i, Bezier(start, controlA, controlB, end, progress));
        }
    }

    private void SetWaterRoute(
        LineRenderer line,
        Vector3 start,
        Vector3 raisedStart,
        Vector3 controlA,
        Vector3 controlB,
        Vector3 end,
        float from,
        float to,
        int segments)
    {
        line.loop = false;
        line.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float normalized = i / (float)(segments - 1);
            float progress = Mathf.Lerp(from, to, normalized);
            line.SetPosition(
                i,
                WaterRoute(start, raisedStart, controlA, controlB, end, progress)
            );
        }
    }

    private void SetOffsetWaterRoute(
        LineRenderer line,
        Vector3 start,
        Vector3 raisedStart,
        Vector3 controlA,
        Vector3 controlB,
        Vector3 end,
        float from,
        float to,
        float offset,
        float wavePhase,
        int segments)
    {
        line.loop = false;
        line.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float normalized = i / (float)(segments - 1);
            float progress = Mathf.Lerp(from, to, normalized);
            Vector3 position = WaterRoute(
                start,
                raisedStart,
                controlA,
                controlB,
                end,
                progress
            );
            float fade = Mathf.Sin(normalized * Mathf.PI);
            float wave = Mathf.Sin(progress * Mathf.PI * 7f + wavePhase);
            position += Vector3.up * (offset + wave * Mathf.Abs(offset) * 0.35f) * fade;
            line.SetPosition(i, position);
        }
    }

    private Vector3 WaterRoute(
        Vector3 start,
        Vector3 raisedStart,
        Vector3 controlA,
        Vector3 controlB,
        Vector3 end,
        float progress)
    {
        float risePortion = Mathf.Clamp(riseDurationPortion, 0.2f, 0.6f);

        if (progress <= risePortion)
        {
            float riseProgress = Smooth(progress / risePortion);
            Vector3 bowedPosition = Vector3.Lerp(start, raisedStart, riseProgress);
            bowedPosition.x += Mathf.Sin(riseProgress * Mathf.PI) *
                Vector3.Distance(start, raisedStart) * 0.08f;
            return bowedPosition;
        }

        float travelProgress = (progress - risePortion) / (1f - risePortion);
        return Bezier(
            raisedStart,
            controlA,
            controlB,
            end,
            Smooth(travelProgress)
        );
    }

    private Vector3 Bezier(
        Vector3 start,
        Vector3 controlA,
        Vector3 controlB,
        Vector3 end,
        float progress)
    {
        float inverse = 1f - progress;
        return
            inverse * inverse * inverse * start +
            3f * inverse * inverse * progress * controlA +
            3f * inverse * progress * progress * controlB +
            progress * progress * progress * end;
    }

    private Vector3 Direction(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f);
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

    private void CreateRuntimeMaterial()
    {
        Shader shader = Shader.Find("Sprites/Default");

        if (shader == null)
        {
            shader = Shader.Find("UI/Default");
        }

        accentMaterial = new Material(shader)
        {
            name = "ShuffleTransferVfx_RuntimeMaterial"
        };
    }

    private void OnDestroy()
    {
        if (accentMaterial != null)
        {
            Destroy(accentMaterial);
        }
    }
}
