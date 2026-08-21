using System;
using UnityEngine;

/// <summary>
/// 스테이지 맵에서 선택한 클래스 외형을 표시하고 WASD 이동을 처리합니다.
/// 전투 프리팹에서는 Spine 표시만 유지하고 전투용 동작과 충돌은 비활성화합니다.
/// </summary>
public class StageMapPlayerController : MonoBehaviour
{
    [Header("클래스별 전투 프리팹")]
    [SerializeField] private GameObject captainPrefab;
    [SerializeField] private GameObject physiquePrefab;
    [SerializeField] private GameObject technicianPrefab;

    [Header("맵 이동")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float avatarScale = 0.18f;

    private Rigidbody2D body;
    private Transform avatarTransform;
    private Vector2 moveInput;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        CreateClassAvatar();
    }

    private void Update()
    {
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical"));

        moveInput = Vector2.ClampMagnitude(moveInput, 1f);
        RefreshFacingDirection();
    }

    private void FixedUpdate()
    {
        if (body == null)
        {
            return;
        }

        body.MovePosition(
            body.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    private void CreateClassAvatar()
    {
        GameObject prefab = GetSelectedClassPrefab();
        if (prefab == null)
        {
            Debug.LogError(
                "[StageMapPlayerController] 선택 클래스에 맞는 프리팹이 없습니다.",
                this);
            return;
        }

        GameObject avatar = Instantiate(prefab, transform);
        avatar.name = "MapAvatar";
        avatar.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        avatar.transform.localScale = Vector3.one * avatarScale;
        avatarTransform = avatar.transform;

        DisableBattleComponents(avatar);
    }

    private GameObject GetSelectedClassPrefab()
    {
        PlayerClass playerClass = PlayerClass.Captain;
        if (GameManager.Instance != null &&
            GameManager.Instance.PlayerData != null)
        {
            playerClass = GameManager.Instance.PlayerData.PlayerClass;
        }

        switch (playerClass)
        {
            case PlayerClass.Physique:
                return physiquePrefab;
            case PlayerClass.Technician:
                return technicianPrefab;
            case PlayerClass.Captain:
                return captainPrefab;
            default:
                Debug.LogWarning(
                    $"[StageMapPlayerController] 선택 클래스가 없어 Captain 외형을 사용합니다: {playerClass}",
                    this);
                return captainPrefab;
        }
    }

    private static void DisableBattleComponents(GameObject avatar)
    {
        foreach (Collider2D collider in avatar.GetComponentsInChildren<Collider2D>(true))
        {
            collider.enabled = false;
        }

        foreach (Rigidbody2D avatarBody in avatar.GetComponentsInChildren<Rigidbody2D>(true))
        {
            avatarBody.simulated = false;
        }

        foreach (Behaviour behaviour in avatar.GetComponentsInChildren<Behaviour>(true))
        {
            if (behaviour == null)
            {
                continue;
            }

            Type behaviourType = behaviour.GetType();
            string typeNamespace = behaviourType.Namespace ?? string.Empty;
            if (typeNamespace.StartsWith("Spine", StringComparison.Ordinal))
            {
                continue;
            }

            behaviour.enabled = false;
        }
    }

    private void RefreshFacingDirection()
    {
        if (avatarTransform == null || Mathf.Approximately(moveInput.x, 0f))
        {
            return;
        }

        Vector3 scale = avatarTransform.localScale;
        scale.x = Mathf.Abs(scale.x) * (moveInput.x < 0f ? -1f : 1f);
        avatarTransform.localScale = scale;
    }
}
