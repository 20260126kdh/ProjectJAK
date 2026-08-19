using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 튜토리얼에서 카드 적용 대상의 머리 위를 따라가는 빨간 역삼각형입니다.
/// </summary>
public sealed class TutorialTargetMarker : MonoBehaviour
{
    private const float MarkerHeightOffset = 0.25f;
    private const string EnemySpineVisualName = "SpineVisual";

    private Transform target;
    private Renderer targetSpineRenderer;
    private Graphic[] targetUiGraphics;
    private Renderer[] targetRenderers;
    private SpriteRenderer targetSpriteRenderer;
    private Vector2 positionOffset;

    /// <summary>
    /// 표시가 따라갈 전투 대상을 지정합니다.
    /// </summary>
    public void SetTarget(
        Transform targetTransform,
        SpriteRenderer spriteRenderer = null,
        Vector2 markerOffset = default)
    {
        target = targetTransform;
        Transform spineVisual = target != null
            ? target.Find(EnemySpineVisualName)
            : null;
        targetSpineRenderer = spineVisual != null
            ? spineVisual.GetComponent<Renderer>()
            : null;
        Transform enemyUiRoot = target != null
            ? target.Find("EnemyUIRoot")
            : null;
        targetUiGraphics = enemyUiRoot != null
            ? enemyUiRoot.GetComponentsInChildren<Graphic>(true)
            : null;
        targetSpriteRenderer = spriteRenderer;
        positionOffset = markerOffset;
        targetRenderers = target != null
            ? target.GetComponentsInChildren<Renderer>(true)
            : null;
        CreateTriangle();
        UpdatePosition();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        UpdatePosition();
    }

    private void CreateTriangle()
    {
        Mesh mesh = new Mesh { name = "TutorialTargetTriangle" };
        mesh.vertices = new[]
        {
            new Vector3(-0.28f, 0.24f, 0f),
            new Vector3(0.28f, 0.24f, 0f),
            new Vector3(0f, -0.24f, 0f)
        };
        mesh.triangles = new[] { 0, 1, 2 };

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        Shader shader = Shader.Find("Sprites/Default");
        meshRenderer.material = new Material(shader);
        meshRenderer.material.color = new Color(1f, 0.05f, 0.05f, 1f);
        meshRenderer.sortingOrder = 200;
    }

    private void UpdatePosition()
    {
        Vector3 position = target.position;

        if (TryGetUiBounds(out Bounds uiBounds))
        {
            position.x = uiBounds.center.x;
            position.y = uiBounds.max.y + MarkerHeightOffset;
        }
        else if (targetSpineRenderer != null &&
            targetSpineRenderer.enabled &&
            targetSpineRenderer.gameObject.activeInHierarchy)
        {
            Bounds spineBounds = targetSpineRenderer.bounds;
            position.x = spineBounds.center.x;
            position.y = spineBounds.max.y + MarkerHeightOffset;
        }
        else if (targetSpriteRenderer != null &&
            targetSpriteRenderer.enabled &&
            targetSpriteRenderer.gameObject.activeInHierarchy)
        {
            Bounds spriteBounds = targetSpriteRenderer.bounds;
            position.x = spriteBounds.center.x;
            position.y = spriteBounds.max.y + MarkerHeightOffset;
        }
        else if (TryGetCombinedRendererBounds(out Bounds combinedBounds))
        {
            position.y = combinedBounds.max.y + MarkerHeightOffset;
        }
        else
        {
            position.y += 2f;
        }

        position.x += positionOffset.x;
        position.y += positionOffset.y;
        position.z = -1f;
        transform.position = position;
    }

    /// <summary>
    /// 적 UI에 실제로 표시 중인 Graphic만 합쳐 화살표 기준 영역을 구합니다.
    /// Canvas 전체 크기나 비활성 상태 아이콘은 포함하지 않습니다.
    /// </summary>
    private bool TryGetUiBounds(out Bounds uiBounds)
    {
        uiBounds = default;
        bool hasBounds = false;

        if (targetUiGraphics == null)
        {
            return false;
        }

        Vector3[] corners = new Vector3[4];

        foreach (Graphic graphic in targetUiGraphics)
        {
            if (graphic == null ||
                !graphic.enabled ||
                !graphic.gameObject.activeInHierarchy ||
                graphic.rectTransform == null)
            {
                continue;
            }

            graphic.rectTransform.GetWorldCorners(corners);

            foreach (Vector3 corner in corners)
            {
                if (!hasBounds)
                {
                    uiBounds = new Bounds(corner, Vector3.zero);
                    hasBounds = true;
                }
                else
                {
                    uiBounds.Encapsulate(corner);
                }
            }
        }

        return hasBounds;
    }

    private bool TryGetCombinedRendererBounds(out Bounds combinedBounds)
    {
        combinedBounds = default;
        bool hasBounds = false;

        if (targetRenderers == null)
        {
            return false;
        }

        foreach (Renderer renderer in targetRenderers)
        {
            if (renderer == null ||
                !renderer.enabled ||
                !renderer.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (!hasBounds)
            {
                combinedBounds = renderer.bounds;
                hasBounds = true;
                continue;
            }

            combinedBounds.Encapsulate(renderer.bounds);
        }

        return hasBounds;
    }
}
