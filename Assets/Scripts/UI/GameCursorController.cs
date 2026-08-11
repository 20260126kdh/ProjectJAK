using UnityEngine;

/// <summary>
/// 게임 전체에서 기본 커서와 클릭 커서를 전환합니다.
/// </summary>
public sealed class GameCursorController : MonoBehaviour
{
    private const string NormalCursorResourcePath = "Cursor/Nomal";
    private const string ClickCursorResourcePath = "Cursor/Click";

    private static readonly Vector2 NormalHotspotRatio = new Vector2(
        40f / 256f,
        25f / 256f
    );

    private static readonly Vector2 ClickHotspotRatio = new Vector2(
        36f / 256f,
        24f / 256f
    );

    private static GameCursorController instance;

    private Texture2D normalCursor;
    private Texture2D clickCursor;
    private bool isClickCursorActive;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (instance != null || FindFirstObjectByType<GameCursorController>() != null)
        {
            return;
        }

        GameObject root = new GameObject("[GameCursorController]");
        DontDestroyOnLoad(root);
        root.AddComponent<GameCursorController>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        normalCursor = Resources.Load<Texture2D>(NormalCursorResourcePath);
        clickCursor = Resources.Load<Texture2D>(ClickCursorResourcePath);

        if (normalCursor == null || clickCursor == null)
        {
            Debug.LogError(
                "[GameCursorController] 기본 또는 클릭 커서 이미지를 불러오지 못했습니다."
            );
            enabled = false;
            return;
        }

        ApplyNormalCursor();
    }

    private void Update()
    {
        bool shouldUseClickCursor = Input.GetMouseButton(0);

        if (shouldUseClickCursor == isClickCursorActive)
        {
            return;
        }

        if (shouldUseClickCursor)
        {
            ApplyCursor(clickCursor, ClickHotspotRatio);
            isClickCursorActive = true;
            return;
        }

        ApplyNormalCursor();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && normalCursor != null)
        {
            ApplyNormalCursor();
        }
    }

    private void OnDestroy()
    {
        if (instance != this)
        {
            return;
        }

        instance = null;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void ApplyNormalCursor()
    {
        ApplyCursor(normalCursor, NormalHotspotRatio);
        isClickCursorActive = false;
    }

    private static void ApplyCursor(Texture2D cursorTexture, Vector2 hotspotRatio)
    {
        Vector2 hotspot = new Vector2(
            cursorTexture.width * hotspotRatio.x,
            cursorTexture.height * hotspotRatio.y
        );

        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
    }
}
