#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 전투 씬에 플레이어 체력과 방어도 HUD를 구성하고 기존 UI를 재배치합니다.
/// </summary>
public static class PlayerBattleHUDSetup
{
    private const string MenuPath =
        "Tools/ProjectJAK/Apply Player Battle HUD";
    private const string BattleScenePath =
        "Assets/Scenes/PlayScene/BattleScene.unity";
    private const string HudRootName = "PlayerBattleHUD";
    private const string HpCasePath = "Assets/Art/Enemy/UI/HP_Case.png";
    private const string HpBarPath = "Assets/Art/Enemy/UI/HP_Bar.png";
    private const string BlockPath = "Assets/Art/Enemy/UI/Block.png";
    private const string FontPath =
        "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    [MenuItem(MenuPath)]
    public static void Apply()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != BattleScenePath)
        {
            Debug.LogError(
                $"[PlayerBattleHUDSetup] BattleScene을 연 뒤 실행해 주세요: {BattleScenePath}");
            return;
        }

        Canvas canvas = FindRootCanvas();
        PlayerCombatUI combatUI = Object.FindFirstObjectByType<PlayerCombatUI>();
        if (canvas == null || combatUI == null)
        {
            Debug.LogError(
                "[PlayerBattleHUDSetup] Canvas 또는 PlayerCombatUI를 찾지 못했습니다.");
            return;
        }

        Sprite hpCase = LoadSprite(HpCasePath, "HP_Case_0");
        Sprite hpBar = LoadSprite(HpBarPath, "HP_Bar_0");
        Sprite block = LoadSprite(BlockPath, "Block_0");
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (hpCase == null || hpBar == null || block == null || font == null)
        {
            Debug.LogError(
                "[PlayerBattleHUDSetup] 플레이어 HUD에 필요한 UI 리소스를 찾지 못했습니다.");
            return;
        }

        RemovePreviousHud(canvas.transform);
        DisableLegacyTexts(combatUI);
        ApplyExistingBattleUiLayout();

        GameObject hudRoot = CreateRect(HudRootName, canvas.transform);
        RectTransform hudRect = hudRoot.GetComponent<RectTransform>();
        SetTopLeft(hudRect, new Vector2(730f, 112f), new Vector2(365f, -60f));

        Image hpFill;
        TMP_Text hpText;
        CreateHealthBar(hudRect, hpCase, hpBar, font, out hpFill, out hpText);
        TMP_Text blockText = CreateBlockUi(hudRect, block, font);

        combatUI.ConfigureBattleHud(hpFill, hpText, blockText);
        EditorUtility.SetDirty(combatUI);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[PlayerBattleHUDSetup] 플레이어 전투 HUD 적용 완료");
    }

    private static Canvas FindRootCanvas()
    {
        GameObject namedCanvas = GameObject.Find("Canvas");
        if (namedCanvas != null &&
            namedCanvas.TryGetComponent(out Canvas rootCanvas) &&
            rootCanvas.renderMode != RenderMode.WorldSpace)
        {
            return rootCanvas;
        }

        Canvas[] canvases = Object.FindObjectsByType<Canvas>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode != RenderMode.WorldSpace &&
                canvas.transform.parent == null)
            {
                return canvas;
            }
        }

        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode != RenderMode.WorldSpace)
            {
                return canvas;
            }
        }

        return null;
    }

    private static void RemovePreviousHud(Transform canvasTransform)
    {
        Transform previous = canvasTransform.Find(HudRootName);
        if (previous != null)
        {
            Object.DestroyImmediate(previous.gameObject);
        }
    }

    private static void DisableLegacyTexts(PlayerCombatUI combatUI)
    {
        SerializedObject serialized = new SerializedObject(combatUI);
        DisableObject(serialized.FindProperty("hpText"));
        DisableObject(serialized.FindProperty("blockText"));
    }

    private static void DisableObject(SerializedProperty property)
    {
        if (property?.objectReferenceValue is Component component)
        {
            component.gameObject.SetActive(false);
            EditorUtility.SetDirty(component.gameObject);
        }
    }

    private static void ApplyExistingBattleUiLayout()
    {
        SetRectPosition("HandCardParent", new Vector2(0f, 160f));
        SetRectPosition(
            "AttackDefenseUseCountPanel",
            new Vector2(-46f, -85f));
        SetTransformPosition(
            "PlayerStatusRoot",
            new Vector3(-107.6f, -194f, -0.6605325f));
    }

    private static void SetRectPosition(string objectName, Vector2 position)
    {
        GameObject target = GameObject.Find(objectName);
        if (target != null && target.transform is RectTransform rect)
        {
            rect.anchoredPosition = position;
            EditorUtility.SetDirty(rect);
        }
    }

    private static void SetTransformPosition(string objectName, Vector3 position)
    {
        GameObject target = GameObject.Find(objectName);
        if (target != null)
        {
            target.transform.localPosition = position;
            EditorUtility.SetDirty(target.transform);
        }
    }

    private static void CreateHealthBar(
        RectTransform parent,
        Sprite hpCase,
        Sprite hpBar,
        TMP_FontAsset font,
        out Image hpFill,
        out TMP_Text hpText)
    {
        GameObject group = CreateRect("HealthGroup", parent);
        RectTransform groupRect = group.GetComponent<RectTransform>();
        SetCentered(groupRect, new Vector2(600f, 82f), new Vector2(-54f, 0f));

        GameObject mask = CreateImage("HealthFillMask", groupRect, null);
        RectTransform maskRect = mask.GetComponent<RectTransform>();
        SetCentered(maskRect, new Vector2(500f, 52f), Vector2.zero);
        mask.GetComponent<Image>().color = Color.clear;
        mask.AddComponent<RectMask2D>();

        GameObject fill = CreateImage("HealthFill", maskRect, hpBar);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        Stretch(fillRect);
        hpFill = fill.GetComponent<Image>();
        hpFill.type = Image.Type.Filled;
        hpFill.fillMethod = Image.FillMethod.Horizontal;
        hpFill.fillOrigin = 0;
        hpFill.fillAmount = 1f;

        GameObject frame = CreateImage("HealthCase", groupRect, hpCase);
        RectTransform frameRect = frame.GetComponent<RectTransform>();
        Stretch(frameRect);

        hpText = CreateText("HealthText", groupRect, font, 28f);
        SetCentered(hpText.rectTransform, new Vector2(400f, 36f), Vector2.zero);
    }

    private static TMP_Text CreateBlockUi(
        RectTransform parent,
        Sprite blockSprite,
        TMP_FontAsset font)
    {
        GameObject block = CreateImage("BlockCase", parent, blockSprite);
        RectTransform blockRect = block.GetComponent<RectTransform>();
        SetCentered(blockRect, new Vector2(96f, 96f), new Vector2(270f, 0f));

        TMP_Text blockText = CreateText("BlockText", blockRect, font, 30f);
        SetCentered(blockText.rectTransform, new Vector2(54f, 42f), Vector2.zero);
        return blockText;
    }

    private static TMP_Text CreateText(
        string objectName,
        Transform parent,
        TMP_FontAsset font,
        float maxFontSize)
    {
        GameObject textObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = maxFontSize;
        text.enableAutoSizing = true;
        text.fontSizeMin = 10f;
        text.fontSizeMax = maxFontSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.95f, 0.85f, 1f);
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Truncate;
        text.outlineWidth = 0.18f;
        text.outlineColor = new Color32(18, 8, 6, 255);
        text.raycastTarget = false;
        return text;
    }

    private static GameObject CreateRect(string objectName, Transform parent)
    {
        GameObject result = new GameObject(objectName, typeof(RectTransform));
        result.layer = LayerMask.NameToLayer("UI");
        result.transform.SetParent(parent, false);
        return result;
    }

    private static Sprite LoadSprite(string assetPath, string preferredName)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        foreach (Object asset in assets)
        {
            if (asset is Sprite sprite && sprite.name == preferredName)
            {
                return sprite;
            }
        }

        foreach (Object asset in assets)
        {
            if (asset is Sprite sprite)
            {
                return sprite;
            }
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static GameObject CreateImage(
        string objectName,
        Transform parent,
        Sprite sprite)
    {
        GameObject result = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        result.layer = LayerMask.NameToLayer("UI");
        result.transform.SetParent(parent, false);
        Image image = result.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.raycastTarget = false;
        return result;
    }

    private static void SetTopLeft(
        RectTransform rect,
        Vector2 size,
        Vector2 position)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    private static void SetCentered(
        RectTransform rect,
        Vector2 size,
        Vector2 position)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
#endif
