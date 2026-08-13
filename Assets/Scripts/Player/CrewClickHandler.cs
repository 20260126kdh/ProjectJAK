using UnityEngine;

/// <summary>
/// 선원 오브젝트 클릭을 감지하여 선택된 카드를 해당 선원에게 사용하도록 요청합니다.
/// </summary>
public class CrewClickHandler : MonoBehaviour
{
    private Crew crew;
    private BattleManager battleManager;

    private void Awake()
    {
        crew = GetComponentInParent<Crew>();
        EnsureClickCollider();
    }

    private void Start()
    {
        FindBattleManager();
    }

    private void OnMouseDown()
    {
        if (battleManager == null)
        {
            FindBattleManager();
        }

        if (battleManager == null || crew == null)
        {
            return;
        }

        if (!IsClosestCrewToPointer())
        {
            return;
        }

        battleManager.UseSelectedCardOnCrew(crew);
    }

    /// <summary>
    /// 선원 Collider가 겹친 경우 실제 PNG 중심이 클릭 지점에 가장 가까운
    /// 살아 있는 선원 하나만 입력을 처리하도록 합니다.
    /// </summary>
    private bool IsClosestCrewToPointer()
    {
        CrewManager crewManager =
            FindFirstObjectByType<CrewManager>();

        Camera mainCamera = Camera.main;

        if (crewManager == null || mainCamera == null)
        {
            return true;
        }

        Vector3 pointerScreenPosition = Input.mousePosition;
        Crew closestCrew = null;
        float closestSqrDistance = float.MaxValue;

        foreach (Crew candidate in crewManager.Crews)
        {
            if (candidate == null || !candidate.IsAlive)
            {
                continue;
            }

            SpriteRenderer candidateRenderer =
                candidate.transform.Find("Visual")?
                    .GetComponent<SpriteRenderer>();

            Vector3 candidateWorldPosition =
                candidateRenderer != null
                    ? candidateRenderer.bounds.center
                    : candidate.transform.position;
            Vector3 candidateScreenPosition =
                mainCamera.WorldToScreenPoint(candidateWorldPosition);
            Vector2 screenDifference =
                new Vector2(
                    pointerScreenPosition.x - candidateScreenPosition.x,
                    pointerScreenPosition.y - candidateScreenPosition.y
                );
            float sqrDistance = screenDifference.sqrMagnitude;

            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                closestCrew = candidate;
            }
        }

        return closestCrew == null || closestCrew == crew;
    }

    /// <summary>
    /// 선원 Sprite 영역에 맞는 클릭 Collider를 런타임에 준비합니다.
    /// 기존 프리팹의 렌더링 설정은 변경하지 않습니다.
    /// </summary>
    private void EnsureClickCollider()
    {
        if (GetComponent<Collider2D>() != null)
        {
            return;
        }

        SpriteRenderer spriteRenderer =
            GetComponent<SpriteRenderer>();

        if (spriteRenderer == null ||
            spriteRenderer.sprite == null)
        {
            Debug.LogWarning(
                "[CrewClickHandler] 클릭 영역을 만들 " +
                "SpriteRenderer가 없습니다."
            );

            return;
        }

        BoxCollider2D clickCollider =
            gameObject.AddComponent<BoxCollider2D>();

        clickCollider.size =
            spriteRenderer.sprite.bounds.size;
        clickCollider.offset =
            spriteRenderer.sprite.bounds.center;
    }

    private void FindBattleManager()
    {
        battleManager =
            FindFirstObjectByType<BattleManager>();

        if (battleManager == null)
        {
            Debug.LogError(
                "[CrewClickHandler] BattleManager를 찾지 못했습니다."
            );
        }
    }
}
