using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어의 HP와 방어도 UI를 표시하는 클래스입니다.
/// PlayerCombat과 PlayerData 값을 읽어서 화면에 출력합니다.
/// </summary>
public class PlayerCombatUI : MonoBehaviour
{
    [Header("Player Combat")]
    [SerializeField]
    private PlayerCombat playerCombat;

    [Header("HP Text")]
    [SerializeField]
    private TextMeshProUGUI hpText;

    [Header("Block Text")]
    [SerializeField]
    private TextMeshProUGUI blockText;

    [Header("Player HUD")]
    [SerializeField] private Image hpFillImage;
    [SerializeField] private TMP_Text hpValueText;
    [SerializeField] private TMP_Text blockValueText;

    private void Update()
    {
        if (playerCombat == null)
        {
            playerCombat = FindFirstObjectByType<PlayerCombat>();

            if (playerCombat == null)
            {
                return;
            }
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        PlayerData playerData = GameManager.Instance.PlayerData;

        if (playerData == null)
        {
            return;
        }

        if (hpText != null)
        {
            hpText.text = $"HP : {playerData.CurrentHP} / {playerData.MaxHP}";
        }

        if (blockText != null)
        {
            blockText.text = $"Block : {playerCombat.CurrentBlock}";
        }

        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = playerData.MaxHP > 0
                ? Mathf.Clamp01((float)playerData.CurrentHP / playerData.MaxHP)
                : 0f;
        }

        if (hpValueText != null)
        {
            hpValueText.text =
                $"{Mathf.Max(0, playerData.CurrentHP)} / {Mathf.Max(0, playerData.MaxHP)}";
        }

        if (blockValueText != null)
        {
            blockValueText.text = Mathf.Max(0, playerCombat.CurrentBlock).ToString();
        }
    }

    /// <summary>
    /// 전투 화면에서 사용할 플레이어 체력 및 방어도 UI를 연결합니다.
    /// </summary>
    public void ConfigureBattleHud(
        Image healthFill,
        TMP_Text healthValue,
        TMP_Text blockValue)
    {
        hpFillImage = healthFill;
        hpValueText = healthValue;
        blockValueText = blockValue;
    }
}
