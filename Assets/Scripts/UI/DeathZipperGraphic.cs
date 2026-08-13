using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 작살이 통과한 위치부터 화면 중앙이 위아래로 벌어지는
/// 사망 전환의 붉은 영역을 UI 메시로 그립니다.
/// </summary>
public sealed class DeathZipperGraphic : MaskableGraphic
{
    private const int HorizontalSegments = 96;

    private float harpoonProgress = -0.1f;
    private float openingDuration = 0.28f;
    private bool forceOpen;

    /// <summary>
    /// 작살 진행률과 각 지점의 최대 개방 시간을 갱신합니다.
    /// </summary>
    public void SetOpening(
        float progress,
        float duration,
        bool shouldForceOpen)
    {
        harpoonProgress = progress;
        openingDuration = Mathf.Max(0.01f, duration);
        forceOpen = shouldForceOpen;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();

        Rect rect = rectTransform.rect;
        float halfHeight = rect.height * 0.5f;
        float segmentWidth = rect.width / HorizontalSegments;

        for (int index = 0; index < HorizontalSegments; index++)
        {
            float normalizedLeft =
                (float)index / HorizontalSegments;
            float normalizedRight =
                (float)(index + 1) / HorizontalSegments;

            float leftOpening = GetOpening(normalizedLeft);
            float rightOpening = GetOpening(normalizedRight);

            if (leftOpening <= 0f && rightOpening <= 0f)
            {
                continue;
            }

            float leftX = rect.xMin + segmentWidth * index;
            float rightX = leftX + segmentWidth;
            float leftExtent = halfHeight * leftOpening;
            float rightExtent = halfHeight * rightOpening;

            AddQuad(
                vertexHelper,
                new Vector2(leftX, 0f),
                new Vector2(rightX, 0f),
                new Vector2(rightX, rightExtent),
                new Vector2(leftX, leftExtent)
            );
            AddQuad(
                vertexHelper,
                new Vector2(leftX, -leftExtent),
                new Vector2(rightX, -rightExtent),
                new Vector2(rightX, 0f),
                new Vector2(leftX, 0f)
            );
        }
    }

    private float GetOpening(float horizontalPosition)
    {
        if (forceOpen)
        {
            return 1f;
        }

        if (horizontalPosition > harpoonProgress)
        {
            return 0f;
        }

        /*
         * 작살이 특정 가로 지점을 지난 이후의 시간을 환산합니다.
         * 오래 전에 지나간 왼쪽일수록 더 크게 벌어져
         * 작살 바로 뒤에 V자형 지퍼 끝점이 남습니다.
         */
        float passedDistance = harpoonProgress - horizontalPosition;
        float estimatedPassedTime =
            passedDistance * PlayerDeathTransitionController.ActiveTravelDuration;
        float opening = estimatedPassedTime / openingDuration;

        return Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(opening));
    }

    private void AddQuad(
        VertexHelper vertexHelper,
        Vector2 bottomLeft,
        Vector2 bottomRight,
        Vector2 topRight,
        Vector2 topLeft)
    {
        int startIndex = vertexHelper.currentVertCount;
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        vertex.position = bottomLeft;
        vertexHelper.AddVert(vertex);
        vertex.position = bottomRight;
        vertexHelper.AddVert(vertex);
        vertex.position = topRight;
        vertexHelper.AddVert(vertex);
        vertex.position = topLeft;
        vertexHelper.AddVert(vertex);

        vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
    }
}
