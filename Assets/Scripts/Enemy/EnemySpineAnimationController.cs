using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

/// <summary>
/// 서로 다른 Spine SkeletonData를 사용하는 적의 대기·공격 연출을 전환합니다.
/// 공격 재생이 끝나면 기존 대기 SkeletonData와 배치로 자동 복귀합니다.
/// </summary>
public class EnemySpineAnimationController : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private SkeletonDataAsset idleSkeletonData;
    [SerializeField] private SkeletonDataAsset attackSkeletonData;
    [SerializeField] private Vector3 idleLocalPosition;
    [SerializeField] private Vector3 idleLocalScale = Vector3.one;
    [SerializeField] private Vector3 attackLocalPosition;
    [SerializeField] private Vector3 attackLocalScale = Vector3.one;

    private Coroutine attackRoutine;

    /// <summary>
    /// 대기·공격 Spine과 각 SkeletonData에 맞춘 로컬 배치를 설정합니다.
    /// </summary>
    public void Configure(
        SkeletonAnimation targetAnimation,
        SkeletonDataAsset idleData,
        SkeletonDataAsset attackData,
        Vector3 idlePosition,
        Vector3 idleScale,
        Vector3 attackPosition,
        Vector3 attackScale)
    {
        skeletonAnimation = targetAnimation;
        idleSkeletonData = idleData;
        attackSkeletonData = attackData;
        idleLocalPosition = idlePosition;
        idleLocalScale = idleScale;
        attackLocalPosition = attackPosition;
        attackLocalScale = attackScale;
    }

    /// <summary>
    /// 공격 Spine을 한 번 재생하고 완료 후 대기 Spine으로 복귀합니다.
    /// 공격 Spine 참조가 없으면 기존 표현을 유지합니다.
    /// </summary>
    public void PlayAttack()
    {
        if (skeletonAnimation == null ||
            idleSkeletonData == null ||
            attackSkeletonData == null)
        {
            return;
        }

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
        }

        attackRoutine = StartCoroutine(PlayAttackRoutine());
    }

    private IEnumerator PlayAttackRoutine()
    {
        Transform visualTransform = skeletonAnimation.transform;
        visualTransform.localPosition = attackLocalPosition;
        visualTransform.localScale = attackLocalScale;

        skeletonAnimation.skeletonDataAsset = attackSkeletonData;
        skeletonAnimation.Initialize(true);

        string attackAnimationName = FindAnimationName(
            attackSkeletonData,
            "attack");
        TrackEntry attackEntry = skeletonAnimation.AnimationState.SetAnimation(
            0,
            attackAnimationName,
            false);

        float duration = attackEntry != null && attackEntry.Animation != null
            ? attackEntry.Animation.Duration
            : 0f;
        if (duration > 0f)
        {
            yield return new WaitForSeconds(duration);
        }
        else
        {
            yield return null;
        }

        skeletonAnimation.skeletonDataAsset = idleSkeletonData;
        skeletonAnimation.Initialize(true);
        visualTransform.localPosition = idleLocalPosition;
        visualTransform.localScale = idleLocalScale;

        string idleAnimationName = FindAnimationName(
            idleSkeletonData,
            "idle");
        skeletonAnimation.AnimationState.SetAnimation(
            0,
            idleAnimationName,
            true);
        attackRoutine = null;
    }

    private static string FindAnimationName(
        SkeletonDataAsset skeletonDataAsset,
        string preferredName)
    {
        SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(true);
        if (skeletonData == null || skeletonData.Animations.Count == 0)
        {
            return string.Empty;
        }

        for (int i = 0; i < skeletonData.Animations.Count; i++)
        {
            string animationName = skeletonData.Animations.Items[i].Name;
            if (animationName.IndexOf(
                    preferredName,
                    System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return animationName;
            }
        }

        return skeletonData.Animations.Items[0].Name;
    }
}
