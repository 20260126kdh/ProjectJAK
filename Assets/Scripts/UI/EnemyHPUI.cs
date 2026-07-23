using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 적의 현재 체력과 최대 체력을
/// 체력바와 텍스트에 표시합니다.
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

    private int lastCurrentHP = -1;
    private int lastMaxHP = -1;

    private void Awake()
    {
        TryFindReferences();
        RefreshUI(true);
    }

    private void Update()
    {
        RefreshUI(false);
    }

    /// <summary>
    /// 필요한 참조가 비어 있다면
    /// 같은 적 프리팹 내부에서 자동으로 찾습니다.
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
    /// 적의 체력이 변경됐을 때만
    /// 체력 UI를 갱신합니다.
    /// </summary>
    private void RefreshUI(
        bool forceRefresh)
    {
        if (targetEnemy == null)
        {
            return;
        }

        int currentHP =
            targetEnemy.CurrentHP;

        int maxHP =
            targetEnemy.MaxHP;

        if (!forceRefresh &&
            currentHP == lastCurrentHP &&
            maxHP == lastMaxHP)
        {
            return;
        }

        lastCurrentHP = currentHP;
        lastMaxHP = maxHP;

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
    /// 현재 체력 비율에 따라
    /// 체력바의 Fill Amount를 갱신합니다.
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
    /// 현재 체력과 최대 체력을
    /// 텍스트로 표시합니다.
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
    /// 외부에서 표시할 적을 지정합니다.
    /// </summary>
    public void SetTargetEnemy(
        Enemy enemy)
    {
        targetEnemy = enemy;

        lastCurrentHP = -1;
        lastMaxHP = -1;

        RefreshUI(true);
    }
}