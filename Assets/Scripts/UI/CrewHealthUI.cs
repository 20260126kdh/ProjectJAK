using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 캡틴 선원의 체력 비율만 작은 월드 체력바로 표시합니다.
/// </summary>
public class CrewHealthUI : MonoBehaviour
{
    [SerializeField] private Crew targetCrew;
    [SerializeField] private Image hpFillImage;
    [SerializeField] private TMP_Text hpText;

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
    /// 표시할 선원과 체력 Fill 이미지를 설정합니다.
    /// </summary>
    public void Configure(Crew crew, Image fillImage, TMP_Text healthText)
    {
        UnsubscribeHealthEvent();
        targetCrew = crew;
        hpFillImage = fillImage;
        hpText = healthText;
        SubscribeHealthEvent();
        RefreshCurrentHealth();
    }

    private void TryFindReferences()
    {
        if (targetCrew == null)
        {
            targetCrew = GetComponentInParent<Crew>();
        }
    }

    private void SubscribeHealthEvent()
    {
        if (targetCrew == null)
        {
            return;
        }

        targetCrew.HealthChanged -= HandleHealthChanged;
        targetCrew.HealthChanged += HandleHealthChanged;
    }

    private void UnsubscribeHealthEvent()
    {
        if (targetCrew != null)
        {
            targetCrew.HealthChanged -= HandleHealthChanged;
        }
    }

    private void RefreshCurrentHealth()
    {
        if (targetCrew != null)
        {
            HandleHealthChanged(targetCrew.CurrentHP, targetCrew.MaxHP);
        }
    }

    private void HandleHealthChanged(int currentHP, int maxHP)
    {
        if (hpFillImage != null)
        {
            float healthRatio = maxHP > 0
                ? Mathf.Clamp01((float)currentHP / maxHP)
                : 0f;
            hpFillImage.fillAmount = healthRatio;
        }

        if (hpText != null)
        {
            hpText.text = $"{Mathf.Max(0, currentHP)} / {Mathf.Max(0, maxHP)}";
        }
    }
}
