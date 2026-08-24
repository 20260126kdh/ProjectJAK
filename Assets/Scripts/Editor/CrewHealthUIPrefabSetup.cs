#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 캡틴 선원 프리팹 머리 위에 작은 체력바를 구성합니다.
/// </summary>
[InitializeOnLoad]
public static class CrewHealthUIPrefabSetup
{
    private const string MenuPath =
        "Tools/ProjectJAK/Apply Crew Health UI";
    private const string PrefabPath =
        "Assets/Prefabs/Crew/CrewPrefab.prefab";
    private const string HealthRootName = "CrewHealthUIRoot";
    private const string HealthCasePath =
        "Assets/Art/Enemy/UI/HP_Case.png";
    private const string HealthCaseSpriteName = "HP_Case_0";
    private const string FontPath =
        "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
    private const float HeadCenterOffsetX = -0.18f;

    static CrewHealthUIPrefabSetup()
    {
        EditorApplication.delayCall += ApplyMissingSetup;
    }

    [MenuItem(MenuPath)]
    public static void ApplyAll()
    {
        ApplySetup(onlyMissing: false);
    }

    private static void ApplyMissingSetup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            EditorApplication.isCompiling)
        {
            return;
        }

        ApplySetup(onlyMissing: true);
    }

    private static void ApplySetup(bool onlyMissing)
    {
        GameObject prefabAsset =
            AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefabAsset == null)
        {
            Debug.LogError(
                $"[CrewHealthUIPrefabSetup] 선원 프리팹을 찾지 못했습니다: " +
                PrefabPath);
            return;
        }

        if (onlyMissing &&
            prefabAsset.transform.Find(HealthRootName) != null &&
            prefabAsset.GetComponentInChildren<CrewHealthUI>(true) != null)
        {
            return;
        }

        GameObject prefabRoot =
            PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            ConfigurePrefab(prefabRoot);
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
            AssetDatabase.SaveAssets();
            Debug.Log("[CrewHealthUIPrefabSetup] 선원 체력 UI 적용 완료");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    private static void ConfigurePrefab(GameObject prefabRoot)
    {
        Crew crew = prefabRoot.GetComponent<Crew>();
        SpriteRenderer crewRenderer =
            prefabRoot.GetComponentInChildren<SpriteRenderer>(true);
        if (crew == null || crewRenderer == null || crewRenderer.sprite == null)
        {
            Debug.LogError(
                "[CrewHealthUIPrefabSetup] Crew 또는 선원 SpriteRenderer를 찾지 못했습니다.");
            return;
        }

        Transform previousRoot = prefabRoot.transform.Find(HealthRootName);
        if (previousRoot != null)
        {
            Object.DestroyImmediate(previousRoot.gameObject);
        }

        GameObject healthRoot = new GameObject(
            HealthRootName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(CrewHealthUI));
        healthRoot.transform.SetParent(prefabRoot.transform, false);

        RectTransform rootRect = healthRoot.GetComponent<RectTransform>();
        /*
         * 몬스터 체력바(400 x 70, Canvas 보정 포함)의
         * 화면상 약 2/3 크기가 되도록 고정합니다.
         */
        rootRect.sizeDelta = new Vector2(240f, 38f);
        rootRect.localScale = new Vector3(0.0045f, 0.0045f, 1f);
        rootRect.localPosition = CalculateHeadPosition(
            prefabRoot.transform,
            crewRenderer);

        Canvas canvas = healthRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = crewRenderer.sortingOrder + 20;

        CanvasScaler canvasScaler = healthRoot.GetComponent<CanvasScaler>();
        canvasScaler.dynamicPixelsPerUnit = 10f;

        Sprite healthCaseSprite = LoadSprite(
            HealthCasePath,
            HealthCaseSpriteName);
        if (healthCaseSprite == null)
        {
            Object.DestroyImmediate(healthRoot);
            throw new InvalidDataException(
                $"HP Case 스프라이트를 불러오지 못했습니다: {HealthCasePath}");
        }

        GameObject frameObject = CreateImageObject("Frame", rootRect);
        RectTransform frameRect = frameObject.GetComponent<RectTransform>();
        StretchToParent(frameRect, Vector2.zero, Vector2.zero);
        Image frameImage = frameObject.GetComponent<Image>();
        frameImage.sprite = healthCaseSprite;
        frameImage.color = Color.white;
        frameImage.preserveAspect = false;
        frameImage.raycastTarget = false;

        GameObject maskObject = CreateImageObject("FillMask", rootRect);
        RectTransform maskRect = maskObject.GetComponent<RectTransform>();
        SetCenteredSize(maskRect, new Vector2(172f, 8f));
        Image maskImage = maskObject.GetComponent<Image>();
        maskImage.color = Color.clear;
        maskImage.raycastTarget = false;
        RectMask2D rectMask = maskObject.AddComponent<RectMask2D>();
        rectMask.padding = Vector4.zero;

        GameObject fillObject = CreateImageObject("Fill", maskRect);
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        StretchToParent(fillRect, Vector2.zero, Vector2.zero);
        Image fillImage = fillObject.GetComponent<Image>();
        fillImage.color = new Color(0.72f, 0.06f, 0.055f, 1f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;
        fillImage.fillAmount = 1f;
        fillImage.raycastTarget = false;

        frameRect.SetAsLastSibling();

        GameObject textObject = new GameObject(
            "HPText",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        textObject.transform.SetParent(rootRect, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        StretchToParent(textRect, new Vector2(5f, 1f), new Vector2(-5f, -1f));
        TextMeshProUGUI hpText = textObject.GetComponent<TextMeshProUGUI>();
        hpText.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        hpText.fontSize = 14f;
        hpText.fontStyle = FontStyles.Bold;
        hpText.alignment = TextAlignmentOptions.Center;
        hpText.color = new Color(1f, 0.94f, 0.82f, 1f);
        hpText.textWrappingMode = TextWrappingModes.NoWrap;
        hpText.raycastTarget = false;
        hpText.outlineWidth = 0.18f;
        hpText.outlineColor = new Color32(20, 10, 8, 255);

        CrewHealthUI healthUI = healthRoot.GetComponent<CrewHealthUI>();
        healthUI.Configure(crew, fillImage, hpText);
    }

    private static Vector3 CalculateHeadPosition(
        Transform prefabRoot,
        SpriteRenderer crewRenderer)
    {
        GetVisibleSpriteBounds(
            crewRenderer.sprite,
            out float spriteMinX,
            out float spriteMaxX,
            out float spriteMinY,
            out float spriteMaxY);
        Transform visualTransform = crewRenderer.transform;
        float firstX =
            visualTransform.localPosition.x +
            spriteMinX * visualTransform.localScale.x;
        float secondX =
            visualTransform.localPosition.x +
            spriteMaxX * visualTransform.localScale.x;
        float firstY =
            visualTransform.localPosition.y +
            spriteMinY * visualTransform.localScale.y;
        float secondY =
            visualTransform.localPosition.y +
            spriteMaxY * visualTransform.localScale.y;

        return new Vector3(
            (firstX + secondX) * 0.5f + HeadCenterOffsetX,
            Mathf.Max(firstY, secondY) + 0.1f,
            0f);
    }

    private static Sprite LoadSprite(string assetPath, string spriteName)
    {
        Sprite mainSprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (mainSprite != null)
        {
            return mainSprite;
        }

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        foreach (Object asset in assets)
        {
            if (asset is Sprite sprite && sprite.name == spriteName)
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

        Debug.LogError(
            $"[CrewHealthUIPrefabSetup] HP Case 스프라이트를 찾지 못했습니다: " +
            $"{assetPath} ({spriteName})");
        return null;
    }

    private static void GetVisibleSpriteBounds(
        Sprite sprite,
        out float minX,
        out float maxX,
        out float minY,
        out float maxY)
    {
        string assetPath = AssetDatabase.GetAssetPath(sprite);
        Texture2D sourceTexture = new Texture2D(
            2,
            2,
            TextureFormat.RGBA32,
            false);

        try
        {
            byte[] imageBytes = File.ReadAllBytes(assetPath);
            if (!sourceTexture.LoadImage(imageBytes, false))
            {
                SetSpriteBoundsFallback(
                    sprite,
                    out minX,
                    out maxX,
                    out minY,
                    out maxY);
                return;
            }

            Color32[] pixels = sourceTexture.GetPixels32();
            int visibleMinX = sourceTexture.width;
            int visibleMaxX = -1;
            int visibleMinY = sourceTexture.height;
            int visibleMaxY = -1;

            for (int y = 0; y < sourceTexture.height; y++)
            {
                int rowOffset = y * sourceTexture.width;
                for (int x = 0; x < sourceTexture.width; x++)
                {
                    if (pixels[rowOffset + x].a <= 8)
                    {
                        continue;
                    }

                    visibleMinX = Mathf.Min(visibleMinX, x);
                    visibleMaxX = Mathf.Max(visibleMaxX, x);
                    visibleMinY = Mathf.Min(visibleMinY, y);
                    visibleMaxY = Mathf.Max(visibleMaxY, y);
                }
            }

            if (visibleMaxX < visibleMinX || visibleMaxY < visibleMinY)
            {
                SetSpriteBoundsFallback(
                    sprite,
                    out minX,
                    out maxX,
                    out minY,
                    out maxY);
                return;
            }

            float rectWidth = sprite.rect.width;
            float rectHeight = sprite.rect.height;
            float pixelsPerUnit = sprite.pixelsPerUnit;
            minX =
                ((float)visibleMinX / sourceTexture.width * rectWidth -
                 sprite.pivot.x) / pixelsPerUnit;
            maxX =
                ((float)(visibleMaxX + 1) / sourceTexture.width * rectWidth -
                 sprite.pivot.x) / pixelsPerUnit;
            minY =
                ((float)visibleMinY / sourceTexture.height * rectHeight -
                 sprite.pivot.y) / pixelsPerUnit;
            maxY =
                ((float)(visibleMaxY + 1) / sourceTexture.height * rectHeight -
                 sprite.pivot.y) / pixelsPerUnit;
        }
        finally
        {
            Object.DestroyImmediate(sourceTexture);
        }
    }

    private static void SetSpriteBoundsFallback(
        Sprite sprite,
        out float minX,
        out float maxX,
        out float minY,
        out float maxY)
    {
        Bounds spriteBounds = sprite.bounds;
        minX = spriteBounds.min.x;
        maxX = spriteBounds.max.x;
        minY = spriteBounds.min.y;
        maxY = spriteBounds.max.y;
    }

    private static GameObject CreateImageObject(
        string objectName,
        Transform parent)
    {
        GameObject imageObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        imageObject.transform.SetParent(parent, false);
        return imageObject;
    }

    private static void StretchToParent(
        RectTransform rectTransform,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;
    }

    private static void SetCenteredSize(
        RectTransform rectTransform,
        Vector2 size)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = size;
    }
}
#endif
