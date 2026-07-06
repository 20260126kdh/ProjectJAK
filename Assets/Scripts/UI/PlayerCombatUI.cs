using TMPro;
using UnityEngine;

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
    }
}