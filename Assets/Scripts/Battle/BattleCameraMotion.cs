using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어가 실제 체력 피해를 받을 때 월드 카메라에 짧은 반동을 적용합니다.
/// 화면 고정 UI는 움직이지 않고 배경과 전투 캐릭터만 함께 움직입니다.
/// </summary>
public sealed class BattleCameraMotion : MonoBehaviour
{
    private const float AttackOffsetX = 0.12f;
    private const float AttackShakeY = 0.018f;
    private const float AttackPushDuration = 0.045f;
    private const float AttackReturnDuration = 0.16f;

    private const float HitOffsetX = -0.16f;
    private const float HitShakeY = 0.045f;
    private const float HitPushDuration = 0.05f;
    private const float HitReturnDuration = 0.21f;

    private Camera targetCamera;
    private Vector3 baseLocalPosition;
    private Quaternion baseLocalRotation;
    private float baseOrthographicSize;
    private Coroutine motionCoroutine;
    private int lastAttackFrame = -1;
    private int lastHitFrame = -1;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
        CacheBaseState();
    }

    private void OnDisable()
    {
        RestoreBaseState();
    }

    /// <summary>
    /// 플레이어 공격 시 창 방향으로 짧게 전진하는 카메라 연출을 재생합니다.
    /// 같은 프레임의 다단 공격은 한 번으로 합칩니다.
    /// </summary>
    public static void PlayPlayerAttack()
    {
        BattleCameraMotion cameraMotion = GetOrCreate();
        if (cameraMotion == null ||
            cameraMotion.lastAttackFrame == Time.frameCount)
        {
            return;
        }

        cameraMotion.lastAttackFrame = Time.frameCount;
        cameraMotion.StartMotion(cameraMotion.PlayAttackMotion());
    }

    /// <summary>
    /// 플레이어가 실제 체력 피해를 받았을 때 반동 카메라 연출을 재생합니다.
    /// 같은 프레임의 중복 피해는 한 번으로 합칩니다.
    /// </summary>
    public static void PlayPlayerHit()
    {
        BattleCameraMotion cameraMotion = GetOrCreate();
        if (cameraMotion == null ||
            cameraMotion.lastHitFrame == Time.frameCount)
        {
            return;
        }

        cameraMotion.lastHitFrame = Time.frameCount;
        cameraMotion.StartMotion(cameraMotion.PlayHitMotion());
    }

    private static BattleCameraMotion GetOrCreate()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null || !mainCamera.orthographic)
        {
            return null;
        }

        BattleCameraMotion cameraMotion =
            mainCamera.GetComponent<BattleCameraMotion>();

        if (cameraMotion == null)
        {
            cameraMotion = mainCamera.gameObject.AddComponent<BattleCameraMotion>();
        }

        return cameraMotion;
    }

    private void StartMotion(IEnumerator motion)
    {
        if (motionCoroutine != null)
        {
            StopCoroutine(motionCoroutine);
        }

        RestoreBaseState();
        motionCoroutine = StartCoroutine(motion);
    }

    private IEnumerator PlayHitMotion()
    {
        Vector3 hitPosition =
            baseLocalPosition + new Vector3(HitOffsetX, 0f, 0f);

        yield return MoveCamera(
            baseLocalPosition,
            hitPosition,
            HitPushDuration,
            HitShakeY,
            1.5f
        );

        yield return MoveCamera(
            hitPosition,
            baseLocalPosition,
            HitReturnDuration,
            HitShakeY,
            1.5f
        );

        CompleteMotion();
    }

    private IEnumerator PlayAttackMotion()
    {
        Vector3 attackPosition =
            baseLocalPosition + new Vector3(AttackOffsetX, 0f, 0f);

        yield return MoveCamera(
            baseLocalPosition,
            attackPosition,
            AttackPushDuration,
            AttackShakeY,
            1f
        );

        yield return MoveCamera(
            attackPosition,
            baseLocalPosition,
            AttackReturnDuration,
            AttackShakeY,
            1f
        );

        CompleteMotion();
    }

    private IEnumerator MoveCamera(
        Vector3 startPosition,
        Vector3 targetPosition,
        float duration,
        float verticalShake,
        float shakeCycles)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(elapsed / duration);
            float eased = normalized * normalized * (3f - 2f * normalized);
            Vector3 position = Vector3.Lerp(
                startPosition,
                targetPosition,
                eased
            );
            float shakeEnvelope = 1f - normalized;
            position.y += Mathf.Sin(
                    normalized * Mathf.PI * 2f * shakeCycles
                )
                * verticalShake
                * shakeEnvelope;

            transform.localPosition = position;
            yield return null;
        }

        transform.localPosition = targetPosition;
    }

    private void CacheBaseState()
    {
        baseLocalPosition = transform.localPosition;
        baseLocalRotation = transform.localRotation;
        baseOrthographicSize = targetCamera != null
            ? targetCamera.orthographicSize
            : 5f;
    }

    private void CompleteMotion()
    {
        RestoreBaseState();
        motionCoroutine = null;
    }

    private void RestoreBaseState()
    {
        transform.localPosition = baseLocalPosition;
        transform.localRotation = baseLocalRotation;

        if (targetCamera != null)
        {
            targetCamera.orthographicSize = baseOrthographicSize;
        }
    }
}
