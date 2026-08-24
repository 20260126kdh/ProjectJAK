using System.Collections;
using UnityEngine;

/// <summary>
/// 적의 대기 표시를 보존하면서 공격·피격 전용 이미지를 잠시 표시합니다.
/// 피격 표현은 진행 중인 공격 표현보다 우선합니다.
/// </summary>
public class EnemyCombatVisualController : MonoBehaviour
{
    [SerializeField] private GameObject idleVisual;
    [SerializeField] private SpriteRenderer stateRenderer;
    [SerializeField] private Sprite attackSprite;
    [SerializeField] private Vector3 attackLocalPosition;
    [SerializeField] private Vector3 attackLocalScale = Vector3.one;
    [SerializeField] private Sprite hitSprite;
    [SerializeField] private Vector3 hitLocalPosition;
    [SerializeField] private Vector3 hitLocalScale = Vector3.one;
    [SerializeField, Min(0.05f)] private float attackDuration = 0.35f;
    [SerializeField, Min(0.05f)] private float hitDuration = 0.18f;

    private Coroutine stateRoutine;
    private int activePriority;
    private int stateVersion;

    /// <summary>
    /// 공격·피격 이미지와 현재 대기 표시를 기준으로 전환 정보를 설정합니다.
    /// </summary>
    public void Configure(
        GameObject targetIdleVisual,
        SpriteRenderer targetStateRenderer,
        Sprite targetAttackSprite,
        Vector3 targetAttackPosition,
        Vector3 targetAttackScale,
        Sprite targetHitSprite,
        Vector3 targetHitPosition,
        Vector3 targetHitScale)
    {
        idleVisual = targetIdleVisual;
        stateRenderer = targetStateRenderer;
        attackSprite = targetAttackSprite;
        attackLocalPosition = targetAttackPosition;
        attackLocalScale = targetAttackScale;
        hitSprite = targetHitSprite;
        hitLocalPosition = targetHitPosition;
        hitLocalScale = targetHitScale;
        RestoreIdleVisual();
    }

    /// <summary>
    /// 공격 이미지를 짧게 표시합니다. 피격 표현 중에는 실행하지 않습니다.
    /// </summary>
    public void PlayAttack()
    {
        PlayState(
            attackSprite,
            attackLocalPosition,
            attackLocalScale,
            attackDuration,
            1);
    }

    /// <summary>
    /// 실제 체력 피해를 받았을 때 피격 이미지를 최우선으로 표시합니다.
    /// </summary>
    public void PlayHit()
    {
        PlayState(
            hitSprite,
            hitLocalPosition,
            hitLocalScale,
            hitDuration,
            2);
    }

    private void OnDisable()
    {
        RestoreIdleVisual();
    }

    private void PlayState(
        Sprite sprite,
        Vector3 localPosition,
        Vector3 localScale,
        float duration,
        int priority)
    {
        if (sprite == null || stateRenderer == null || idleVisual == null)
        {
            return;
        }

        if (stateRoutine != null && priority < activePriority)
        {
            return;
        }

        if (stateRoutine != null)
        {
            StopCoroutine(stateRoutine);
        }

        stateVersion++;
        activePriority = priority;
        stateRoutine = StartCoroutine(
            PlayStateRoutine(
                sprite,
                localPosition,
                localScale,
                duration,
                stateVersion));
    }

    private IEnumerator PlayStateRoutine(
        Sprite sprite,
        Vector3 localPosition,
        Vector3 localScale,
        float duration,
        int version)
    {
        idleVisual.SetActive(false);
        stateRenderer.sprite = sprite;
        stateRenderer.transform.localPosition = localPosition;
        stateRenderer.transform.localScale = localScale;
        stateRenderer.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        if (version == stateVersion)
        {
            RestoreIdleVisual();
        }
    }

    private void RestoreIdleVisual()
    {
        activePriority = 0;
        stateRoutine = null;

        if (stateRenderer != null)
        {
            stateRenderer.gameObject.SetActive(false);
        }

        if (idleVisual != null)
        {
            idleVisual.SetActive(true);
        }
    }
}
