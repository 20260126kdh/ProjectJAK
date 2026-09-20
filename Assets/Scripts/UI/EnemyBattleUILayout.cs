using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 기존 적 UI의 위치와 배율을 유지하면서 상단 아이콘의 겹침과 배경 높이를 관리합니다.
/// 체력바·방어도·Canvas·적 이미지의 배치는 변경하지 않습니다.
/// </summary>
public class EnemyBattleUILayout : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxIconsPerRow = 4;
    [SerializeField, Min(0f)] private float iconSpacing = 8f;
    [SerializeField] private Vector2 backgroundPadding = new Vector2(10f, 5f);
    private RectTransform topBar;
    private RectTransform statusContainer;
    private RectTransform intentContainer;
    private RectTransform block;
    private RectTransform background;
    private bool configured;
    private readonly Vector3[] corners = new Vector3[4];
    private readonly List<RectTransform> icons = new List<RectTransform>();
    private readonly List<float> widths = new List<float>();
    private readonly List<float> heights = new List<float>();
    private readonly List<bool> intentFlags = new List<bool>();
    private readonly List<EnemyIconPlacement> placements = new List<EnemyIconPlacement>();

    /// <summary>기존 상단바 안의 상태·작살·행동 아이콘만 줄바꿈 대상으로 연결합니다.</summary>
    public void Configure(Enemy enemy, Transform existingTopBar)
    {
        if (configured || enemy == null || existingTopBar == null) return;
        topBar = existingTopBar as RectTransform;
        statusContainer = existingTopBar.Find("StatusRoot") as RectTransform;
        Transform intentRoot = existingTopBar.Find("IntentRoot");
        intentContainer = intentRoot != null ? intentRoot.Find("IconContainer") as RectTransform : null;
        block = existingTopBar.Find("BlockRoot") as RectTransform;
        background = existingTopBar.Find("TopBarBackground") as RectTransform;
        if (topBar == null || statusContainer == null || intentContainer == null || background == null)
        {
            Debug.LogWarning("[EnemyBattleUILayout] 상단 UI 연결이 부족하여 기존 배치를 유지합니다.", this);
            return;
        }
        // 자동 레이아웃과 수동 줄바꿈이 같은 아이콘 위치를 덮어쓰지 않게 합니다.
        DisableLayout(statusContainer);
        DisableLayout(intentContainer);
        DisableLayout(intentRoot as RectTransform);
        background.gameObject.SetActive(true);
        Image image = background.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(1f, 1f, 1f, 0.65f);
            image.raycastTarget = false;
        }
        // 방어도는 어두운 방패 위에 표시하므로 프리팹의 흰색 숫자 스타일을 유지합니다.
        configured = true;
        RefreshLayout();
    }

    private void LateUpdate()
    {
        if (configured) RefreshLayout();
    }

    private void RefreshLayout()
    {
        if (topBar == null || background == null) return;
        float left = topBar.rect.xMin + 4f;
        float right = topBar.rect.xMax - 4f;
        Vector2 min = topBar.rect.min;
        Vector2 max = topBar.rect.max;
        if (block != null && block.gameObject.activeInHierarchy)
        {
            GetLocalBounds(block, out Vector2 blockMin, out Vector2 blockMax);
            right = Mathf.Min(right, blockMin.x - iconSpacing);
            min = Vector2.Min(min, blockMin);
            max = Vector2.Max(max, blockMax);
        }
        float availableWidth = Mathf.Max(48f, right - left);
        icons.Clear();
        widths.Clear();
        heights.Clear();
        intentFlags.Clear();
        CollectIcons(statusContainer, false, availableWidth);
        CollectIcons(intentContainer, true, availableWidth);
        EnemyIconFlow.Arrange(widths, heights, availableWidth, maxIconsPerRow, iconSpacing, placements, intentFlags);

        // 첫 줄은 원래 위치에 두고 추가 줄은 위로 쌓아 체력바를 가리지 않습니다.
        float bottom = topBar.rect.center.y - 24f;
        for (int i = 0; i < icons.Count; i++)
        {
            EnemyIconPlacement item = placements[i];
            float x = item.Left;
            RectTransform rect = icons[i];
            Vector2 size = new Vector2(item.Width, item.Height);
            if (rect.pivot != new Vector2(0.5f, 0.5f)) rect.pivot = new Vector2(0.5f, 0.5f);
            if (rect.sizeDelta != size) rect.sizeDelta = size;
            if (rect.localScale != Vector3.one) rect.localScale = Vector3.one;
            Vector3 position = topBar.TransformPoint(new Vector3(left + x + item.Width * 0.5f,
                bottom + item.Bottom + item.Height * 0.5f, 0f));
            if (rect.position != position) rect.position = position;
            GetLocalBounds(rect, out Vector2 itemMin, out Vector2 itemMax);
            min = Vector2.Min(min, itemMin);
            max = Vector2.Max(max, itemMax);
        }
        background.anchorMin = background.anchorMax = new Vector2(0.5f, 0.5f);
        background.pivot = new Vector2(0.5f, 0.5f);
        Vector2 backgroundSize = max - min + backgroundPadding * 2f;
        if (background.sizeDelta != backgroundSize) background.sizeDelta = backgroundSize;
        Vector3 backgroundPosition = topBar.TransformPoint((min + max) * 0.5f);
        if (background.position != backgroundPosition) background.position = backgroundPosition;
    }

    private void CollectIcons(RectTransform container, bool isIntent, float availableWidth)
    {
        if (container == null || !container.gameObject.activeInHierarchy) return;
        foreach (Transform child in container)
        {
            if (!child.gameObject.activeSelf || !(child is RectTransform rect)) continue;
            Vector2 size = new Vector2(48f, 48f);
            if (isIntent && child.TryGetComponent(out EnemyIntentIconUI intentIcon))
                size = intentIcon.GetLayoutSize(availableWidth);
            icons.Add(rect);
            widths.Add(size.x);
            heights.Add(size.y);
            intentFlags.Add(isIntent);
        }
    }

    private void GetLocalBounds(RectTransform rect, out Vector2 min, out Vector2 max)
    {
        rect.GetWorldCorners(corners);
        min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        foreach (Vector3 corner in corners)
        {
            Vector2 local = topBar.InverseTransformPoint(corner);
            min = Vector2.Min(min, local);
            max = Vector2.Max(max, local);
        }
    }

    private static void DisableLayout(RectTransform rect)
    {
        if (rect == null) return;
        foreach (LayoutGroup group in rect.GetComponents<LayoutGroup>()) group.enabled = false;
        ContentSizeFitter fitter = rect.GetComponent<ContentSizeFitter>();
        if (fitter != null) fitter.enabled = false;
    }

    /// <summary>흰색 반투명 배경에서 읽을 수 있는 숫자 스타일을 적용합니다.</summary>
    public static void StyleNumber(TMP_Text text, float size)
    {
        if (text == null) return;
        text.fontSize = size;
        text.enableAutoSizing = false;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color(0.08f, 0.1f, 0.14f, 1f);
        // TMP 초기화 전 머티리얼 외곽선 setter는 호출하지 않습니다.
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
    }
}

// 엔진 실행 없이 밀집·다단 공격·줄 감소 조건을 검증하는 순수 배치 계산입니다.
internal struct EnemyIconPlacement
{
    internal float Left;
    internal float Bottom;
    internal float Width;
    internal float Height;
}

internal static class EnemyIconFlow
{
    internal static void Arrange(IReadOnlyList<float> widths, IReadOnlyList<float> heights,
        float availableWidth, int maxPerRow, float spacing, List<EnemyIconPlacement> output,
        IReadOnlyList<bool> rightAligned = null)
    {
        output.Clear();
        availableWidth = System.Math.Max(1f, availableWidth);
        maxPerRow = System.Math.Max(1, maxPerRow);
        spacing = System.Math.Max(0f, spacing);
        float bottom = 0f;
        int start = 0;
        while (start < widths.Count)
        {
            int end = start;
            float rowWidth = 0f;
            float rowHeight = 0f;
            while (end < widths.Count && end - start < maxPerRow)
            {
                float width = System.Math.Min(availableWidth, System.Math.Max(1f, widths[end]));
                float nextWidth = rowWidth + (end > start ? spacing : 0f) + width;
                if (end > start && nextWidth > availableWidth) break;
                rowWidth = nextWidth;
                rowHeight = System.Math.Max(rowHeight, heights[end]);
                end++;
            }
            float left = 0f;
            for (int i = start; i < end; i++)
            {
                float width = System.Math.Min(availableWidth, System.Math.Max(1f, widths[i]));
                output.Add(new EnemyIconPlacement { Left = left, Bottom = bottom,
                    Width = width, Height = System.Math.Max(1f, heights[i]) });
                left += width + spacing;
            }
            // 상태 아이콘 뒤에 오는 의도 그룹은 남은 공간의 오른쪽에 붙입니다.
            if (rightAligned != null)
            {
                for (int i = start; i < end; i++)
                {
                    if (!rightAligned[i]) continue;
                    EnemyIconPlacement item = output[i];
                    item.Left += availableWidth - rowWidth;
                    output[i] = item;
                }
            }
            bottom += System.Math.Max(1f, rowHeight) + spacing;
            start = end;
        }
    }
}
