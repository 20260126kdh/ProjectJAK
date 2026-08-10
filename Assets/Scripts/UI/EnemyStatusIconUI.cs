using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 적에게 적용된 상태 효과 아이콘 하나를 표시합니다.
///
/// 상태 효과 아이콘과 상태 수치,
/// 남은 지속 턴을 화면에 표시합니다.
/// </summary>
public class EnemyStatusIconUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("상태 효과 아이콘")]
    [SerializeField]
    private Image iconImage;

    [Header("상태 효과 수치")]
    [SerializeField]
    private TMP_Text valueText;

    [Header("남은 지속 턴")]
    [SerializeField]
    private TMP_Text turnText;

    private StatusEffectType statusEffectType;
    private StatusEffectData currentStatusEffect;
    private GameObject tooltipObject;
    private RectTransform tooltipRectTransform;
    private TMP_Text tooltipText;

    private const float TooltipWidth = 300f;
    private const float TooltipHeight = 120f;
    private static readonly Vector2 TooltipOffset = new Vector2(18f, -18f);

    /// <summary>
    /// 현재 이 아이콘이 표시하는 상태 효과 종류입니다.
    /// </summary>
    public StatusEffectType StatusEffectType =>
        statusEffectType;

    /// <summary>
    /// 상태 효과 아이콘을 처음 생성할 때 호출합니다.
    /// </summary>
    public void Initialize(
        StatusEffectData statusEffect,
        Sprite iconSprite)
    {
        if (statusEffect == null)
        {
            Debug.LogWarning(
                "[EnemyStatusIconUI] 상태 효과 데이터가 없습니다.",
                this
            );

            return;
        }

        statusEffectType =
            statusEffect.statusEffectType;

        currentStatusEffect = statusEffect;

        if (iconImage != null)
        {
            iconImage.sprite =
                iconSprite;

            iconImage.enabled =
                iconSprite != null;
        }

        Refresh(
            statusEffect
        );
    }

    /// <summary>
    /// 상태 효과의 현재 수치와 남은 턴을 갱신합니다.
    /// </summary>
    public void Refresh(
        StatusEffectData statusEffect)
    {
        if (statusEffect == null)
        {
            return;
        }

        currentStatusEffect = statusEffect;

        RefreshValue(
        statusEffect.value,
        statusEffect.statusEffectType
        );

        RefreshTurn(
            statusEffect.remainingTurn,
            statusEffect.isPermanent,
            statusEffect.statusEffectType
        );

        RefreshTooltipText();
    }

    /// <summary>
    /// 상태 효과 아이콘에 마우스를 올리면 효과 설명을 표시합니다.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentStatusEffect == null)
        {
            return;
        }

        EnsureTooltip();

        if (tooltipObject == null)
        {
            return;
        }

        RefreshTooltipText();
        PositionTooltip(eventData.position);
        tooltipObject.SetActive(true);
    }

    /// <summary>
    /// 상태 효과 아이콘에서 마우스가 벗어나면 설명을 숨깁니다.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void OnDisable()
    {
        HideTooltip();
    }

    private void OnDestroy()
    {
        if (tooltipObject != null)
        {
            Destroy(tooltipObject);
        }
    }

    /// <summary>
    /// 공용 Canvas 위에 입력을 가로막지 않는 툴팁 UI를 생성합니다.
    /// </summary>
    private void EnsureTooltip()
    {
        if (tooltipObject != null)
        {
            return;
        }

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            return;
        }

        Canvas rootCanvas = parentCanvas.rootCanvas;
        tooltipObject = new GameObject(
            "StatusEffectTooltip",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        tooltipObject.transform.SetParent(rootCanvas.transform, false);

        tooltipRectTransform = tooltipObject.GetComponent<RectTransform>();
        tooltipRectTransform.sizeDelta = new Vector2(TooltipWidth, TooltipHeight);
        tooltipRectTransform.pivot = new Vector2(0f, 1f);

        Image backgroundImage = tooltipObject.GetComponent<Image>();
        backgroundImage.color = new Color(0f, 0f, 0f, 0.9f);
        backgroundImage.raycastTarget = false;

        GameObject textObject = new GameObject(
            "TooltipText",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        textObject.transform.SetParent(tooltipObject.transform, false);

        RectTransform textRectTransform = textObject.GetComponent<RectTransform>();
        textRectTransform.anchorMin = Vector2.zero;
        textRectTransform.anchorMax = Vector2.one;
        textRectTransform.offsetMin = new Vector2(14f, 10f);
        textRectTransform.offsetMax = new Vector2(-14f, -10f);

        tooltipText = textObject.GetComponent<TMP_Text>();
        if (valueText != null)
        {
            tooltipText.font = valueText.font;
            tooltipText.fontSharedMaterial = valueText.fontSharedMaterial;
        }

        tooltipText.fontSize = 18f;
        tooltipText.color = Color.white;
        tooltipText.alignment = TextAlignmentOptions.TopLeft;
        tooltipText.textWrappingMode = TextWrappingModes.Normal;
        tooltipText.raycastTarget = false;

        tooltipObject.transform.SetAsLastSibling();
        tooltipObject.SetActive(false);
    }

    /// <summary>
    /// 툴팁이 화면을 벗어나지 않도록 마우스 위치를 Canvas 좌표로 보정합니다.
    /// </summary>
    private void PositionTooltip(Vector2 screenPosition)
    {
        if (tooltipRectTransform == null)
        {
            return;
        }

        Canvas rootCanvas = tooltipRectTransform.GetComponentInParent<Canvas>();
        RectTransform canvasRectTransform =
            rootCanvas != null ? rootCanvas.transform as RectTransform : null;

        if (canvasRectTransform == null)
        {
            return;
        }

        Camera eventCamera =
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : rootCanvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                screenPosition,
                eventCamera,
                out Vector2 localPosition))
        {
            return;
        }

        localPosition += TooltipOffset;

        Rect canvasRect = canvasRectTransform.rect;
        localPosition.x = Mathf.Clamp(
            localPosition.x,
            canvasRect.xMin,
            canvasRect.xMax - TooltipWidth
        );
        localPosition.y = Mathf.Clamp(
            localPosition.y,
            canvasRect.yMin + TooltipHeight,
            canvasRect.yMax
        );

        tooltipRectTransform.anchoredPosition = localPosition;
        tooltipObject.transform.SetAsLastSibling();
    }

    private void RefreshTooltipText()
    {
        if (tooltipText == null || currentStatusEffect == null)
        {
            return;
        }

        string durationText = currentStatusEffect.isPermanent
            ? "전투 종료까지 유지"
            : currentStatusEffect.remainingTurn > 0
                ? $"남은 턴: {currentStatusEffect.remainingTurn}"
                : "조건 충족 시 제거";

        tooltipText.text =
            $"<b>{GetStatusEffectName(statusEffectType)}</b>\n" +
            $"{GetStatusEffectDescription(statusEffectType)}\n" +
            $"현재 수치: {currentStatusEffect.value} · {durationText}";
    }

    private void HideTooltip()
    {
        if (tooltipObject != null)
        {
            tooltipObject.SetActive(false);
        }
    }

    private string GetStatusEffectName(StatusEffectType effectType)
    {
        switch (effectType)
        {
            case StatusEffectType.Might: return "힘";
            case StatusEffectType.Guard: return "속도";
            case StatusEffectType.Resist: return "무감각";
            case StatusEffectType.Lifesteal: return "피 맛";
            case StatusEffectType.Echo: return "잔상";
            case StatusEffectType.Immortal: return "불사의 존재";
            case StatusEffectType.Undead: return "이계의 존재";
            case StatusEffectType.KShellguard: return "등껍질 방패";
            case StatusEffectType.DToxinSwitch: return "독오름";
            case StatusEffectType.FFesteredSkin: return "불어터진 피부";
            case StatusEffectType.HRevelation: return "타락한 계시";
            case StatusEffectType.UnderGround: return "가라앉는 섬";
            case StatusEffectType.UnderWater: return "침몰";
            case StatusEffectType.ProfanedHalo: return "장송의 가호";
            case StatusEffectType.DevilPower: return "악마의 힘";
            case StatusEffectType.Weaken: return "약화";
            case StatusEffectType.Vulnerable: return "취약";
            case StatusEffectType.Cripple: return "손상";
            case StatusEffectType.NoBlock: return "미끄러짐";
            case StatusEffectType.Broken: return "부러짐";
            case StatusEffectType.Jinx: return "침체";
            case StatusEffectType.Paralyze: return "마비";
            case StatusEffectType.Toxic: return "중독";
            case StatusEffectType.Exit: return "소멸";
            case StatusEffectType.MightReduction: return "힘 감소";
            default: return "알 수 없는 상태 효과";
        }
    }

    private string GetStatusEffectDescription(StatusEffectType effectType)
    {
        switch (effectType)
        {
            case StatusEffectType.Might:
                return "공격력이 수치만큼 증가합니다.";
            case StatusEffectType.Guard:
                return "카드로 얻는 방어도가 수치만큼 증가합니다.";
            case StatusEffectType.Resist:
                return "이번 턴 동안 받는 피해가 30% 감소합니다.";
            case StatusEffectType.Lifesteal:
                return "이번 턴 동안 상대에게 준 피해만큼 회복합니다.";
            case StatusEffectType.Echo:
                return "이번 턴에 다음 공격 카드 1장을 2번 사용합니다.";
            case StatusEffectType.Immortal:
                return "체력을 모두 잃으면 체력 1로 한 번 부활합니다.";
            case StatusEffectType.Undead:
                return "방어도 다음으로 선원이 모든 공격 피해를 대신 받습니다.";
            case StatusEffectType.KShellguard:
                return "방어도가 완전히 소진되면 다음 행동 1회를 건너뜁니다.";
            case StatusEffectType.DToxinSwitch:
                return "피해 행동마다 마비와 중독을 번갈아 부여합니다.";
            case StatusEffectType.FFesteredSkin:
                return "피해 행동마다 미끄러짐과 부러짐을 번갈아 부여합니다.";
            case StatusEffectType.HRevelation:
                return "패턴 사이에 타락한 계시를 선택하며 효과가 누적됩니다.";
            case StatusEffectType.UnderGround:
                return "적 행동마다 침몰이 3 증가하며, 사망 시 침몰 상태에 돌입합니다.";
            case StatusEffectType.UnderWater:
                return "다음 턴에 침몰 수치만큼 피해를 준 뒤 사망합니다.";
            case StatusEffectType.ProfanedHalo:
                return "원혼이 남아 있는 동안 받는 피해가 25% 감소합니다.";
            case StatusEffectType.DevilPower:
                return "다음에 사용하는 공격 카드의 피해가 2배가 됩니다.";
            case StatusEffectType.Weaken:
                return "주는 피해가 40% 감소합니다.";
            case StatusEffectType.Vulnerable:
                return "받는 피해가 40% 증가합니다.";
            case StatusEffectType.Cripple:
                return "얻는 방어도가 30% 감소합니다.";
            case StatusEffectType.NoBlock:
                return "방어도를 얻을 수 없습니다.";
            case StatusEffectType.Broken:
                return "공격 카드를 사용할 수 없습니다.";
            case StatusEffectType.Jinx:
                return "다음 턴 손패의 무작위 카드 1장을 사용 불가로 만듭니다.";
            case StatusEffectType.Paralyze:
                return "속도가 수치만큼 감소합니다.";
            case StatusEffectType.Toxic:
                return "턴 종료 시 수치만큼 피해를 받고 수치가 1 감소합니다.";
            case StatusEffectType.Exit:
                return "사용 시 해당 카드를 이번 전투에서 제외합니다.";
            case StatusEffectType.MightReduction:
                return "이번 턴 동안 완력이 수치만큼 감소합니다.";
            default:
                return "현재 적용 중인 특수 상태 효과입니다.";
        }
    }

    /// <summary>
    /// 상태 효과 종류에 따라 수치가 필요한 경우에만 표시합니다.
    /// 힘, 속도, 무감각, 불운, 마비, 중독 등
    /// 실제 수치가 게임 계산에 사용되는 상태만 표시합니다.
    /// </summary>
    private void RefreshValue(
        int value,
        StatusEffectType effectType)
    {
        if (valueText == null)
        {
            return;
        }

        bool shouldShow =
            ShouldShowValue(
                effectType
            ) &&
            value != 0;

        valueText.gameObject.SetActive(
            shouldShow
        );

        if (!shouldShow)
        {
            valueText.text = string.Empty;
            return;
        }

        valueText.text =
            value.ToString();
    }

    /// <summary>
    /// 아이콘에 상태 효과 수치를 표시해야 하는지 반환합니다.
    /// 고정 비율 효과는 수치를 표시하지 않습니다.
    /// </summary>
    private bool ShouldShowValue(
        StatusEffectType effectType)
    {
        switch (effectType)
        {
            case StatusEffectType.Might:
            case StatusEffectType.Guard:
            case StatusEffectType.Resist:
            case StatusEffectType.Jinx:
            case StatusEffectType.Paralyze:
            case StatusEffectType.Toxic:
            case StatusEffectType.MightReduction:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// 턴 감소형 상태 효과에만 남은 지속 턴을 표시합니다.
    /// 영구 효과와 수치형 효과에는 지속 턴을 표시하지 않습니다.
    /// </summary>
    private void RefreshTurn(
        int remainingTurn,
        bool isPermanent,
        StatusEffectType effectType)
    {
        if (turnText == null)
        {
            return;
        }

        bool shouldShow =
            !isPermanent &&
            ShouldShowRemainingTurn(
                effectType
            ) &&
            remainingTurn > 0;

        turnText.gameObject.SetActive(
            shouldShow
        );

        if (!shouldShow)
        {
            turnText.text = string.Empty;
            return;
        }

        turnText.text =
            remainingTurn.ToString();
    }

    /// <summary>
    /// 아이콘에 남은 지속 턴을 표시해야 하는지 반환합니다.
    /// 효과 강도가 고정되어 있고 턴이 감소하는 상태와
    /// 현재 턴에만 유지되는 일시 효과를 분류합니다.
    /// </summary>
    private bool ShouldShowRemainingTurn(
        StatusEffectType effectType)
    {
        switch (effectType)
        {
            /*
             * 현재 턴에만 유지되는 일시형 버프
             */
            case StatusEffectType.Lifesteal:
            case StatusEffectType.Echo:

            /*
             * 남은 턴이 감소하는 디버프
             */
            case StatusEffectType.Weaken:
            case StatusEffectType.Vulnerable:
            case StatusEffectType.Cripple:
            case StatusEffectType.NoBlock:
            case StatusEffectType.Broken:
            case StatusEffectType.MightReduction:
                return true;

            default:
                return false;
        }
    }
}
