using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 적의 체력 변경 이벤트를 받아
/// 체력바와 체력 텍스트를 갱신합니다.
/// </summary>
public class EnemyHPUI : MonoBehaviour
{
    [Header("대상 적")]
    [SerializeField]
    private Enemy targetEnemy;

    [Header("체력바 Fill")]
    [SerializeField]
    private Image hpFillImage;

    [Header("체력 텍스트")]
    [SerializeField]
    private TMP_Text hpText;

    [Header("체력 숫자 표시 여부")]
    [SerializeField]
    private bool showHPText = true;

    private void Awake()
    {
        TryFindReferences();
    }

    private void OnEnable()
    {
        TryFindReferences();
        SubscribeHealthEvent();
        RefreshCurrentHealth();
    }

    private void OnDisable()
    {
        UnsubscribeHealthEvent();
    }

    /// <summary>
    /// 대상 적이 연결되지 않았다면
    /// 부모 오브젝트에서 자동으로 찾습니다.
    /// </summary>
    private void TryFindReferences()
    {
        if (targetEnemy == null)
        {
            targetEnemy =
                GetComponentInParent<Enemy>();
        }
    }

    /// <summary>
    /// 적의 체력 변경 이벤트를 구독합니다.
    /// </summary>
    private void SubscribeHealthEvent()
    {
        if (targetEnemy == null)
        {
            Debug.LogWarning(
                "[EnemyHPUI] 대상 Enemy를 찾지 못했습니다.",
                this
            );

            return;
        }

        /*
         * 중복 구독을 방지합니다.
         */
        targetEnemy.HealthChanged -= HandleHealthChanged;
        targetEnemy.HealthChanged += HandleHealthChanged;
    }

    /// <summary>
    /// 적의 체력 변경 이벤트 구독을 해제합니다.
    /// </summary>
    private void UnsubscribeHealthEvent()
    {
        if (targetEnemy == null)
        {
            return;
        }

        targetEnemy.HealthChanged -= HandleHealthChanged;
    }

    /// <summary>
    /// UI가 활성화될 때 현재 체력으로 즉시 갱신합니다.
    /// 이벤트 구독 이전에 초기화된 체력도 정상 표시됩니다.
    /// </summary>
    private void RefreshCurrentHealth()
    {
        if (targetEnemy == null)
        {
            return;
        }

        HandleHealthChanged(
            targetEnemy.CurrentHP,
            targetEnemy.MaxHP
        );
    }

    /// <summary>
    /// 체력 변경 이벤트가 발생했을 때
    /// 체력바와 텍스트를 갱신합니다.
    /// </summary>
    private void HandleHealthChanged(
        int currentHP,
        int maxHP)
    {
        UpdateFillImage(
            currentHP,
            maxHP
        );

        UpdateHPText(
            currentHP,
            maxHP
        );
    }

    /// <summary>
    /// 현재 체력 비율에 맞게
    /// 체력바의 Fill Amount를 변경합니다.
    /// </summary>
    private void UpdateFillImage(
        int currentHP,
        int maxHP)
    {
        if (hpFillImage == null)
        {
            return;
        }

        float hpRatio = 0f;

        if (maxHP > 0)
        {
            hpRatio =
                Mathf.Clamp01(
                    (float)currentHP / maxHP
                );
        }

        hpFillImage.fillAmount =
            hpRatio;
    }

    /// <summary>
    /// 현재 체력과 최대 체력을 표시합니다.
    /// </summary>
    private void UpdateHPText(
        int currentHP,
        int maxHP)
    {
        if (hpText == null)
        {
            return;
        }

        hpText.gameObject.SetActive(
            showHPText
        );

        if (!showHPText)
        {
            return;
        }

        hpText.text =
            $"{currentHP} / {maxHP}";
    }

    /// <summary>
    /// 외부에서 표시할 적을 변경합니다.
    /// </summary>
    public void SetTargetEnemy(
        Enemy enemy)
    {
        UnsubscribeHealthEvent();

        targetEnemy = enemy;

        SubscribeHealthEvent();
        RefreshCurrentHealth();
    }
}