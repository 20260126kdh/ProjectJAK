using System.Collections;
using Spine.Unity;
using UnityEngine;

/// <summary>
/// 서로 다른 Spine Skeleton을 사용하는 플레이어 애니메이션을 관리합니다.
/// 평소에는 Idle 오브젝트를 표시하고,
/// 공격할 때 Attack 오브젝트로 잠시 전환합니다.
/// </summary>
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Idle Visual")]
    [SerializeField]
    private GameObject idleVisual;

    [SerializeField]
    private SkeletonAnimation idleSkeletonAnimation;

    [Header("Attack Visual")]
    [SerializeField]
    private GameObject attackVisual;

    [SerializeField]
    private SkeletonAnimation attackSkeletonAnimation;

    [Header("Animation Names")]
    [SerializeField]
    private string idleAnimationName = "animation";

    [SerializeField]
    private string attackAnimationName = "animation";

    [Header("Fallback Settings")]
    [Tooltip("공격 애니메이션 길이를 가져오지 못했을 때 사용할 시간입니다.")]
    [SerializeField]
    private float fallbackAttackDuration = 0.6f;

    private Coroutine attackCoroutine;

    [Header("Hit Visual")]
    [SerializeField]
    private GameObject hitVisual;

    [Header("Hit Impact")]
    [SerializeField]
    private GameObject hitImpactObject;

    [SerializeField]
    private Animator hitImpactAnimator;

    [SerializeField]
    private string hitImpactStateName = "PlayerHitImpact";

    [Header("Hit Settings")]
    [Tooltip("피격 이미지를 유지하는 임시 시간입니다.")]
    [SerializeField]
    private float hitDuration = 1f;

    private Coroutine hitCoroutine;

    /// <summary>
    /// 현재 공격 애니메이션이 재생 중인지 여부입니다.
    /// </summary>
    public bool IsPlayingAttack => attackCoroutine != null;

    public bool IsPlayingHit => hitCoroutine != null;

    private void Awake()
    {
        ShowIdleImmediately();
    }

    /// <summary>
    /// 공격 애니메이션을 처음부터 재생합니다.
    /// 이미 공격 중이라면 기존 재생을 중단하고 다시 시작합니다.
    /// </summary>
    public void PlayAttack()
    {
        if (hitCoroutine != null)
        {
            return;
        }

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }

        attackCoroutine = StartCoroutine(PlayAttackCoroutine());
    }

    /// <summary>
    /// 공격 Spine을 표시하고 공격 애니메이션 종료 후 Idle로 돌아갑니다.
    /// </summary>
    private IEnumerator PlayAttackCoroutine()
    {
        if (idleVisual != null)
        {
            idleVisual.SetActive(false);
        }

        if (attackVisual != null)
        {
            attackVisual.SetActive(true);
        }

        float attackDuration = fallbackAttackDuration;

        if (attackSkeletonAnimation != null)
        {
            attackSkeletonAnimation.Initialize(true);

            Spine.TrackEntry trackEntry =
                attackSkeletonAnimation.AnimationState.SetAnimation(
                    0,
                    attackAnimationName,
                    false
                );

            if (trackEntry != null && trackEntry.Animation != null)
            {
                attackDuration = trackEntry.Animation.Duration;
            }
        }
        else
        {
            Debug.LogWarning(
                "[PlayerAnimationController] Attack SkeletonAnimation이 연결되지 않았습니다."
            );
        }

        yield return new WaitForSeconds(attackDuration);

        ShowIdleImmediately();

        attackCoroutine = null;
    }

    /// <summary>
    /// 공격 모션을 즉시 종료하고 Idle 상태로 돌아갑니다.
    /// </summary>
    public void ShowIdleImmediately()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);
            hitCoroutine = null;
        }

        if (attackVisual != null)
        {
            attackVisual.SetActive(false);
        }

        if (hitVisual != null)
        {
            hitVisual.SetActive(false);
        }

        if (hitImpactObject != null)
        {
            hitImpactObject.SetActive(false);
        }

        if (idleVisual != null)
        {
            idleVisual.SetActive(true);
        }

        if (idleSkeletonAnimation != null)
        {
            idleSkeletonAnimation.Initialize(false);

            if (idleSkeletonAnimation.AnimationState != null)
            {
                idleSkeletonAnimation.AnimationState.SetAnimation(
                    0,
                    idleAnimationName,
                    true
                );
            }
        }
    }

    /// <summary>
    /// 피격 PNG와 피격 임팩트를 동시에 표시하고,
    /// 지정된 시간 후 Idle 상태로 돌아갑니다.
    /// 피격 연출은 공격 연출보다 우선합니다.
    /// </summary>
    public void PlayHit()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);
        }

        hitCoroutine =
            StartCoroutine(
                PlayHitCoroutine()
            );
    }

    /// <summary>
    /// 피격 이미지를 표시하고 피격 임팩트를 재생합니다.
    /// </summary>
    private IEnumerator PlayHitCoroutine()
    {
        if (idleVisual != null)
        {
            idleVisual.SetActive(false);
        }

        if (attackVisual != null)
        {
            attackVisual.SetActive(false);
        }

        if (hitVisual != null)
        {
            hitVisual.SetActive(true);
        }

        if (hitImpactObject != null)
        {
            hitImpactObject.SetActive(true);
        }

        if (hitImpactAnimator != null)
        {
            hitImpactAnimator.Play(
                hitImpactStateName,
                0,
                0f
            );
        }
        else
        {
            Debug.LogWarning(
                "[PlayerAnimationController] " +
                "피격 임팩트 Animator가 연결되지 않았습니다.",
                this
            );
        }

        yield return new WaitForSeconds(
            hitDuration
        );

        if (hitVisual != null)
        {
            hitVisual.SetActive(false);
        }

        if (hitImpactObject != null)
        {
            hitImpactObject.SetActive(false);
        }

        hitCoroutine = null;

        ShowIdleImmediately();
    }
}