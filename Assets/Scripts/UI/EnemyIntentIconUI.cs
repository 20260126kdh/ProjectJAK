using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 적 Intent 아이콘 하나를 관리합니다.
///
/// 피해, 방어도, 버프, 디버프 아이콘을 표시하며,
/// 필요한 경우 아이콘 우측 상단에 수치 또는
/// 다단히트 정보를 표시합니다.
/// </summary>
public class EnemyIntentIconUI : MonoBehaviour
{
    [Header("Intent 아이콘")]
    [SerializeField]
    private Image iconImage;

    [Header("Intent 표시 텍스트")]
    [SerializeField]
    private TMP_Text valueText;

    /// <summary>
    /// 아이콘과 표시 문자열을 설정합니다.
    ///
    /// 표시 예:
    /// - 일반 공격: 17
    /// - 다단히트 공격: 9 × 3
    /// - 방어도: 24
    /// </summary>
    public void Initialize(
        Sprite iconSprite,
        string displayText)
    {
        ConfigureReadableLayout(displayText);
        RefreshIcon(iconSprite);
        RefreshValue(displayText);
    }

    private string layoutDisplayText;

    /// <summary>기존 아이콘·우측 숫자 구도를 유지하면서 숫자가 차지하는 폭을 확보합니다.</summary>
    private void ConfigureReadableLayout(string displayText)
    {
        layoutDisplayText = displayText;
        if (iconImage != null)
        {
            RectTransform iconRect = iconImage.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.sizeDelta = new Vector2(48f, 48f);
            iconRect.anchoredPosition = Vector2.zero;
            iconImage.raycastTarget = false;
        }
        if (valueText != null)
        {
            EnemyBattleUILayout.StyleNumber(valueText, 18f);
            RectTransform textRect = valueText.rectTransform;
            textRect.anchorMin = textRect.anchorMax = Vector2.one;
            textRect.pivot = Vector2.one;
            textRect.anchoredPosition = Vector2.zero;
            valueText.alignment = TextAlignmentOptions.TopRight;
            valueText.textWrappingMode = TextWrappingModes.Normal;
        }
    }

    /// <summary>긴 다단 공격 표기를 포함한 아이콘의 안전한 배치 크기를 반환합니다.</summary>
    public Vector2 GetLayoutSize(float availableWidth)
    {
        if (string.IsNullOrWhiteSpace(layoutDisplayText)) return new Vector2(48f, 48f);
        // 비활성 TMP의 머티리얼 초기화를 강제하지 않고 숫자 폭을 여유 있게 산정합니다.
        float textWidth = Mathf.Clamp(layoutDisplayText.Length * 11f, 24f, Mathf.Max(24f, availableWidth - 52f));
        float lines = Mathf.Max(1f, Mathf.Ceil(layoutDisplayText.Length * 11f / textWidth));
        float height = Mathf.Max(48f, lines * 23f);
        if (valueText != null)
        {
            Vector2 textSize = new Vector2(textWidth, height);
            if (valueText.rectTransform.sizeDelta != textSize) valueText.rectTransform.sizeDelta = textSize;
        }
        return new Vector2(52f + textWidth, height);
    }

    /// <summary>
    /// Intent 아이콘 이미지를 갱신합니다.
    /// </summary>
    private void RefreshIcon(
        Sprite iconSprite)
    {
        if (iconImage == null)
        {
            Debug.LogWarning(
                "[EnemyIntentIconUI] " +
                "Icon Image가 연결되지 않았습니다.",
                this
            );

            return;
        }

        bool hasIcon =
            iconSprite != null;

        iconImage.gameObject.SetActive(
            hasIcon
        );

        if (!hasIcon)
        {
            iconImage.sprite = null;
            return;
        }

        iconImage.sprite =
            iconSprite;

        iconImage.color =
            Color.white;

        iconImage.preserveAspect =
            true;
    }

    /// <summary>
    /// Intent 수치 또는 다단히트 정보를 갱신합니다.
    ///
    /// 빈 문자열이면 ValueText를 숨깁니다.
    /// </summary>
    private void RefreshValue(
        string displayText)
    {
        if (valueText == null)
        {
            Debug.LogWarning(
                "[EnemyIntentIconUI] " +
                "Value Text가 연결되지 않았습니다.",
                this
            );

            return;
        }

        bool shouldShowValue =
            !string.IsNullOrWhiteSpace(
                displayText
            );

        valueText.gameObject.SetActive(
            shouldShowValue
        );

        if (!shouldShowValue)
        {
            valueText.text =
                string.Empty;

            return;
        }

        valueText.text =
            displayText;
    }

    /// <summary>
    /// Inspector에서 다단히트 표기를 테스트합니다.
    /// </summary>
    [ContextMenu("Intent 아이콘 테스트")]
    private void TestIntentIcon()
    {
        RefreshValue("9×3");
    }
}