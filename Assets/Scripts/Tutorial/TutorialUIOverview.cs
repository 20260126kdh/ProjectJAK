using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>전투 UI의 밝기를 유지하며 모든 설명을 한 화면에 표시합니다.</summary>
public sealed class TutorialUIOverview : MonoBehaviour
{
    private sealed class CanvasState
    {
        internal Canvas Canvas;
        internal int Layer, Order;
        internal bool Override;
    }

    private sealed class Note
    {
        internal RectTransform Target, Panel, Line;
    }

    private readonly List<CanvasState> canvasStates = new List<CanvasState>();
    private readonly List<Note> notes = new List<Note>();
    private readonly List<Rect> obstacles = new List<Rect>();
    private readonly Vector3[] corners = new Vector3[4];
    private RectTransform overlay;
    private TMP_FontAsset font;
    private Action onClose;
    private Vector2 lastSize;
    private bool closed;

    /// <summary>현재 전투 UI를 찾아 설명을 표시하고 닫기 콜백을 연결합니다.</summary>
    public void Show(TMP_FontAsset textFont, Action close)
    {
        font = textFont;
        onClose = close;
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Camera camera = Camera.main;
        // 월드 공간의 적/선원 UI도 암전보다 나중에 그립니다. Overlay UI는 원래부터 위에 그려집니다.
        int topLayer = 0;
        int topValue = int.MinValue;
        foreach (SortingLayer layer in SortingLayer.layers)
            if (layer.value > topValue) { topValue = layer.value; topLayer = layer.id; }

        Canvas dim = CreateCanvas("WorldDim", camera != null ? RenderMode.ScreenSpaceCamera : RenderMode.ScreenSpaceOverlay, 30000);
        dim.worldCamera = camera;
        dim.sortingLayerID = topLayer;
        if (camera != null) dim.planeDistance = camera.nearClipPlane + 0.01f;
        Image shade = CreateImage("Black60Percent", dim.transform, new Color(0, 0, 0, 0.6f));
        Stretch(shade.rectTransform);
        shade.raycastTarget = false;

        Array.Sort(canvases, (a, b) =>
        {
            int layer = SortingLayer.GetLayerValueFromID(a.sortingLayerID).CompareTo(SortingLayer.GetLayerValueFromID(b.sortingLayerID));
            return layer != 0 ? layer : a.sortingOrder.CompareTo(b.sortingOrder);
        });
        int order = 30001;
        foreach (Canvas canvas in canvases)
        {
            if (!canvas.isActiveAndEnabled || (!canvas.isRootCanvas && !canvas.overrideSorting)) continue;
            if (camera != null && canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) continue;
            canvasStates.Add(new CanvasState { Canvas = canvas, Layer = canvas.sortingLayerID,
                Order = canvas.sortingOrder, Override = canvas.overrideSorting });
            canvas.overrideSorting = true;
            canvas.sortingLayerID = topLayer;
            canvas.sortingOrder = order++;
        }

        Canvas labels = CreateCanvas("UIExplanations", RenderMode.ScreenSpaceOverlay, 32000);
        labels.gameObject.SetActive(false);
        overlay = (RectTransform)labels.transform;
        labels.gameObject.AddComponent<GraphicRaycaster>();
        CanvasScaler scaler = labels.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        Image blocker = CreateImage("InputBlocker", overlay, Color.clear);
        Stretch(blocker.rectTransform);
        blocker.raycastTarget = true;

        TMP_Text heading = CreateText("전투 화면 안내", overlay, 32);
        SetRect(heading.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(650, 60));
        heading.alignment = TextAlignmentOptions.Center;
        Image closeImage = CreateImage("Close", overlay, new Color(0.91f, 0.76f, 0.43f, 1));
        SetRect(closeImage.rectTransform, new Vector2(1, 1), new Vector2(-110, -42), new Vector2(180, 52));
        Button button = closeImage.gameObject.AddComponent<Button>();
        button.targetGraphic = closeImage;
        button.onClick.AddListener(Close);
        TMP_Text closeText = CreateText("닫기", closeImage.transform, 25);
        closeText.color = new Color(0.08f, 0.1f, 0.14f, 1);
        closeText.alignment = TextAlignmentOptions.Center;
        Stretch(closeText.rectTransform);

        AddNote("PlayerBattleHUD", "체력 · 방어도", "현재 / 최대 체력과 방어도입니다.\n방어도는 받는 피해를 막아 줍니다.");
        TurnManager turn = FindFirstObjectByType<TurnManager>();
        string maximum = turn != null ? turn.MaxAttackDefenseCardUseCount.ToString() : "2";
        AddNote("AttackDefenseUseCountPanel", "공격 · 수비 카드 횟수", "이번 턴 사용 횟수 / 최대 횟수입니다.\n공격·수비 카드는 합쳐서 턴당 " + maximum + "장입니다.");
        AddNote("PlayerStatusRoot", "버프 · 디버프", "현재 적용된 효과와 수치입니다.\n아이콘에 마우스를 올려 자세히 확인하세요.");
        AddNote("HandCardParent", "손패 · 카드 선택", "마우스를 올리면 카드가 확대됩니다.\n클릭해 선택한 뒤 사용할 대상을 고르세요.");
        AddNote("DeckOpenButton", "전체 덱 · 뽑을 패 · 버린 패", "각 더미를 눌러 포함된 카드를 확인합니다.\n뽑을 패와 버린 패의 남은 장수도 확인하세요.");
        AddNote("EndTurnButton", "턴 종료 · 보존", "남은 카드에서 보존할 카드를 선택하고\n확정하면 적의 턴이 시작됩니다.");
        AddNote("EnemyUI", "적의 상태 · 다음 행동", "체력·방어도·작살과 상태 효과를 확인하세요.\n의도 아이콘은 적의 다음 행동을 예고합니다.");
        AddNote("SettingButton", "설정", "게임 설정과 일시정지 메뉴를 엽니다.\n지금 안내를 닫은 뒤 이용할 수 있습니다.");
        CrewHealthUI crew = FindFirstObjectByType<CrewHealthUI>();
        if (crew != null) AddNote(crew.transform as RectTransform, "선원 체력", "소환된 선원의 현재 / 최대 체력입니다.\n선원 대상 카드를 사용할 때 확인하세요.");
        labels.gameObject.SetActive(true);
        Canvas.ForceUpdateCanvases();
        LayoutNotes();
    }

    private Canvas CreateCanvas(string objectName, RenderMode mode, int order)
    {
        GameObject root = new GameObject(objectName, typeof(RectTransform), typeof(Canvas));
        root.transform.SetParent(transform, false);
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = mode;
        canvas.overrideSorting = true;
        canvas.sortingOrder = order;
        return canvas;
    }

    private void AddNote(string targetName, string title, string body)
    {
        GameObject target = GameObject.Find(targetName);
        if (target != null) AddNote(target.transform as RectTransform, title, body);
    }

    private void AddNote(RectTransform target, string title, string body)
    {
        if (target == null) return;
        Image line = CreateImage("GuideLine", overlay, new Color(0.93f, 0.8f, 0.5f, 0.7f));
        line.raycastTarget = false;
        line.transform.SetSiblingIndex(1);
        Image panel = CreateImage("Note_" + target.name, overlay, new Color(0.08f, 0.1f, 0.14f, 0.97f));
        panel.raycastTarget = false;
        TMP_Text text = CreateText("<color=#F0CE89><b>" + title + "</b></color>\n" + body, panel.transform, 20);
        Stretch(text.rectTransform);
        text.margin = new Vector4(14, 12, 14, 12);
        notes.Add(new Note { Target = target, Panel = panel.rectTransform, Line = line.rectTransform });
    }

    private void LateUpdate()
    {
        if (overlay != null && (Vector2)overlay.rect.size != lastSize) LayoutNotes();
    }

    private void LayoutNotes()
    {
        lastSize = overlay.rect.size;
        obstacles.Clear();
        foreach (Graphic graphic in FindObjectsByType<Graphic>(FindObjectsSortMode.None))
        {
            if (!graphic.isActiveAndEnabled || graphic.color.a < 0.01f || graphic.transform.IsChildOf(transform)) continue;
            Rect rect = ScreenRect(graphic.rectTransform);
            if (rect.width < 2 || rect.height < 2 || (rect.width > lastSize.x * 0.8f && rect.height > lastSize.y * 0.8f)) continue;
            obstacles.Add(rect);
        }
        var placed = new List<Rect>();
        float width = Mathf.Min(420, (lastSize.x - 64) / 3);
        Vector2 size = new Vector2(width, 146);
        foreach (Note note in notes)
        {
            Rect target = ScreenRect(note.Target);
            Vector2 best = new Vector2(24, 100);
            float bestCost = float.PositiveInfinity;
            // 설명끼리 겹치지 않게 빈 영역을 찾고 실제 UI 영역을 최대한 피합니다.
            for (float y = 24; y <= lastSize.y - size.y - 90; y += 24)
                for (float x = 24; x <= lastSize.x - size.x - 24; x += 24)
                {
                    Rect candidate = new Rect(new Vector2(x, y), size);
                    bool overlaps = false;
                    foreach (Rect previous in placed) if (candidate.Overlaps(previous)) { overlaps = true; break; }
                    if (overlaps) continue;
                    float cost = Vector2.Distance(candidate.center, target.center);
                    foreach (Rect obstacle in obstacles) cost += IntersectionArea(candidate, obstacle) * 10;
                    if (cost < bestCost) { bestCost = cost; best = candidate.position; }
                }
            SetRect(note.Panel, Vector2.zero, best + size * 0.5f, size);
            placed.Add(new Rect(best - Vector2.one * 8, size + Vector2.one * 16));
            Vector2 start = new Vector2(Mathf.Clamp(target.center.x, best.x, best.x + size.x),
                Mathf.Clamp(target.center.y, best.y, best.y + size.y));
            Vector2 end = new Vector2(Mathf.Clamp(target.center.x, 4, lastSize.x - 4), Mathf.Clamp(target.center.y, 4, lastSize.y - 4));
            Vector2 delta = end - start;
            SetRect(note.Line, Vector2.zero, (start + end) * 0.5f, new Vector2(delta.magnitude, 2));
            note.Line.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }
    }

    private Rect ScreenRect(RectTransform target)
    {
        Canvas canvas = target.GetComponentInParent<Canvas>();
        Camera camera = canvas != null && canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera != null ? canvas.worldCamera : Camera.main : null;
        target.GetWorldCorners(corners);
        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        foreach (Vector3 corner in corners)
        {
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(camera, corner);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(overlay, screen, null, out Vector2 local);
            local -= overlay.rect.min;
            min = Vector2.Min(min, local);
            max = Vector2.Max(max, local);
        }
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private static float IntersectionArea(Rect a, Rect b) =>
        Mathf.Max(0, Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin)) *
        Mathf.Max(0, Mathf.Min(a.yMax, b.yMax) - Mathf.Max(a.yMin, b.yMin));

    private TMP_Text CreateText(string value, Transform parent, float size)
    {
        GameObject root = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        root.transform.SetParent(parent, false);
        TMP_Text text = root.GetComponent<TMP_Text>();
        if (font != null) text.font = font;
        text.text = value;
        text.fontSize = size;
        text.color = Color.white;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        return text;
    }

    private static Image CreateImage(string name, Transform parent, Color color)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(Image));
        root.transform.SetParent(parent, false);
        Image image = root.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    /// <summary>UI의 크기를 따라가는 빨간 테두리를 추가합니다. 입력을 가로채지 않습니다.</summary>
    public static GameObject CreateRedBorder(RectTransform target)
    {
        GameObject root = new GameObject("TutorialCountBorder", typeof(RectTransform), typeof(LayoutElement));
        root.transform.SetParent(target, false);
        root.GetComponent<LayoutElement>().ignoreLayout = true;
        Stretch((RectTransform)root.transform);
        for (int i = 0; i < 4; i++)
        {
            Image edge = CreateImage("Edge", root.transform, new Color(1, 0.05f, 0.05f, 1));
            edge.raycastTarget = false;
            RectTransform rect = edge.rectTransform;
            bool horizontal = i < 2;
            rect.anchorMin = horizontal ? new Vector2(0, i) : new Vector2(i - 2, 0);
            rect.anchorMax = horizontal ? new Vector2(1, i) : new Vector2(i - 2, 1);
            rect.sizeDelta = new Vector2(6, 6);
            rect.anchoredPosition = Vector2.zero;
        }
        return root;
    }

    private void Close()
    {
        if (closed) return;
        closed = true;
        RestoreCanvases();
        gameObject.SetActive(false);
        onClose?.Invoke();
        Destroy(gameObject);
    }

    private void OnDisable() => RestoreCanvases();

    private void RestoreCanvases()
    {
        foreach (CanvasState state in canvasStates)
        {
            if (state.Canvas == null) continue;
            state.Canvas.sortingLayerID = state.Layer;
            state.Canvas.sortingOrder = state.Order;
            state.Canvas.overrideSorting = state.Override;
        }
        canvasStates.Clear();
    }
}
