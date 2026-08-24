#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 사용 가능한 일반 몬스터 프리팹에 공격·피격 이미지를 연결합니다.
/// </summary>
[InitializeOnLoad]
public static class EnemyCombatImagePrefabSetup
{
    private const string MenuPath =
        "Tools/ProjectJAK/Apply Available Enemy Combat Images";
    private const string IdleVisualName = "SpineVisual";
    private const string StateVisualName = "CombatStateVisual";

    private sealed class SetupEntry
    {
        public string PrefabPath { get; }
        public string AttackImagePath { get; }
        public string HitImagePath { get; }

        public SetupEntry(
            string prefabPath,
            string attackImagePath,
            string hitImagePath)
        {
            PrefabPath = prefabPath;
            AttackImagePath = attackImagePath;
            HitImagePath = hitImagePath;
        }
    }

    private static readonly IReadOnlyList<SetupEntry> Entries = new[]
    {
        CreateEntry(1, "Thief1", "Thief_Attack.png", "Thief_Hit.png"),
        CreateEntry(1, "Thief2", "Thief_Attack.png", "Thief_Hit.png"),
        CreateEntry(1, "Goby1", "Goby_Attack.png", "Goby_Hit.png"),
        CreateEntry(1, "Goby2", "Goby_Attack.png", "Goby_Hit.png"),
        CreateEntry(1, "Mermaid", "Mermaid_Attack.png", "Mermaid_Hit.png"),
        CreateEntry(1, "SeaCrab1", "SeaCrab_Attack.png", "SeaCrab_Hit.png"),
        CreateEntry(1, "SeaCrab2", "SeaCrab_Attack.png", "SeaCrab_Hit.png"),
        CreateEntry(1, "Mimic1", "Mimic_Attack.png", "Mimic_Hit.png"),
        CreateEntry(1, "Mimic2", "Mimic_Attack.png", "Mimic_Hit.png"),
        CreateEntry(2, "Drowned1", "Drowned_Attack.png", "Drowned_Hit.png"),
        CreateEntry(2, "Drowned2", "Drowned_Attack.png", "Drowned_Hit.png"),
        CreateEntry(2, "JellyfishMermaid", "JellyfishMermaid_Attack.png", "JellyfishMermaid_Hit.png"),
        CreateEntry(2, "OldMermaid", "UndeadMermaid_Attack_redrawn.png", "UndeadMermaid_Hit_redrawn.png"),
        CreateEntry(2, "ShortFinnedSandfish", "Sawfish_Attack_redrawn.png", "Sawfish_Hit_redrawn.png"),
        CreateEntry(2, "Turtle1", "Turtle_Attack_redrawn.png", "Turtle_Hit_redrawn.png"),
        CreateEntry(2, "Turtle2", "Turtle_Attack_redrawn.png", "Turtle_Hit_redrawn.png"),
        CreateEntry(3, "Edward", "Edward_Attack.png", "Edward_Hit.png"),
        CreateEntry(3, "Templeguardian", "Templeguardian_Attack.png", "Templeguardian_Hit.png"),
        CreateEntry(3, "Undeadcommander", "Undeadcommander_Attack.png", "Undeadcommander_Hit.png"),
        CreateEntry(3, "SeaBeast", "SeaBeast_Attack.png", "SeaBeast_Hit.png"),
        CreateEntry(3, "Shipcollector", "Shipcollector_Attack.png", "Shipcollector_Hit.png"),
        CreateBossEntry(1, "ShipwreckCrab", "ShipwreckCrab_Attack.png", "ShipwreckCrab_Hit.png"),
        CreateBossEntry(2, "Aspidochelone", "Aspidochelone_Attack.png", "Aspidochelone_Hit.png"),
        CreateBossEntry(3, "Morbael", "Morbael_Attack.png", "Morbael_Hit.png")
    };

    static EnemyCombatImagePrefabSetup()
    {
        EditorApplication.delayCall += ApplyMissingSetups;
    }

    [MenuItem(MenuPath)]
    public static void ApplyAll()
    {
        ApplySetups(onlyMissing: false);
    }

    private static SetupEntry CreateEntry(
        int stage,
        string prefabName,
        string attackImageName,
        string hitImageName)
    {
        return new SetupEntry(
            $"Assets/Prefabs/Enemy/NormalBattle/{stage}Stage/{prefabName}.prefab",
            $"Assets/Art/Enemy/CombatImages/Stage{stage}/{attackImageName}",
            $"Assets/Art/Enemy/CombatImages/Stage{stage}/{hitImageName}");
    }

    private static SetupEntry CreateBossEntry(
        int stage,
        string prefabName,
        string attackImageName,
        string hitImageName)
    {
        return new SetupEntry(
            $"Assets/Prefabs/Enemy/BossBattle/{stage}Stage/{prefabName}.prefab",
            $"Assets/Art/Enemy/CombatImages/Boss/{attackImageName}",
            $"Assets/Art/Enemy/CombatImages/Boss/{hitImageName}");
    }

    private static void ApplyMissingSetups()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            EditorApplication.isCompiling)
        {
            return;
        }

        ApplySetups(onlyMissing: true);
    }

    private static void ApplySetups(bool onlyMissing)
    {
        int appliedCount = 0;

        foreach (SetupEntry entry in Entries)
        {
            GameObject prefabAsset =
                AssetDatabase.LoadAssetAtPath<GameObject>(entry.PrefabPath);
            if (prefabAsset == null)
            {
                Debug.LogError(
                    $"[EnemyCombatImagePrefabSetup] 프리팹을 찾지 못했습니다: " +
                    entry.PrefabPath);
                continue;
            }

            Sprite attackSprite = LoadCombatSprite(entry.AttackImagePath);
            Sprite hitSprite = LoadCombatSprite(entry.HitImagePath);
            if (attackSprite == null || hitSprite == null)
            {
                if (!onlyMissing)
                {
                    Debug.LogError(
                        $"[EnemyCombatImagePrefabSetup] 전투 이미지를 불러오지 못했습니다: " +
                        entry.PrefabPath);
                }
                continue;
            }

            if (onlyMissing &&
                prefabAsset.GetComponent<EnemyCombatVisualController>() != null)
            {
                continue;
            }

            GameObject prefabRoot =
                PrefabUtility.LoadPrefabContents(entry.PrefabPath);
            try
            {
                ConfigurePrefab(prefabRoot, attackSprite, hitSprite);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, entry.PrefabPath);
                appliedCount++;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        if (appliedCount > 0)
        {
            AssetDatabase.SaveAssets();
            Debug.Log(
                $"[EnemyCombatImagePrefabSetup] " +
                $"일반 몬스터 전투 이미지 적용 완료: {appliedCount}/{Entries.Count}");
        }
    }

    private static Sprite LoadCombatSprite(string assetPath)
    {
        TextureImporter importer =
            AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            return null;
        }

        if (importer.textureType != TextureImporterType.Sprite ||
            importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static void ConfigurePrefab(
        GameObject prefabRoot,
        Sprite attackSprite,
        Sprite hitSprite)
    {
        Transform idleVisual = prefabRoot.transform.Find(IdleVisualName);
        SkeletonAnimation idleAnimation =
            idleVisual != null
                ? idleVisual.GetComponent<SkeletonAnimation>()
                : null;
        if (idleAnimation == null || idleAnimation.SkeletonDataAsset == null)
        {
            Debug.LogError(
                $"[EnemyCombatImagePrefabSetup] 대기 Spine을 찾지 못했습니다: " +
                prefabRoot.name);
            return;
        }

        Transform existingStateVisual =
            prefabRoot.transform.Find(StateVisualName);
        GameObject stateVisual;
        if (existingStateVisual == null)
        {
            stateVisual = new GameObject(StateVisualName);
            stateVisual.transform.SetParent(prefabRoot.transform, false);
        }
        else
        {
            stateVisual = existingStateVisual.gameObject;
        }

        SpriteRenderer stateRenderer =
            stateVisual.GetComponent<SpriteRenderer>();
        if (stateRenderer == null)
        {
            stateRenderer = stateVisual.AddComponent<SpriteRenderer>();
        }

        MeshRenderer idleRenderer = idleVisual.GetComponent<MeshRenderer>();
        if (idleRenderer != null)
        {
            stateRenderer.sortingLayerID = idleRenderer.sortingLayerID;
            stateRenderer.sortingOrder = idleRenderer.sortingOrder;
        }

        GetIdleBounds(
            idleAnimation,
            out float targetCenterX,
            out float targetFloorY,
            out float targetHeight);
        CalculateSpriteLayout(
            prefabRoot,
            attackSprite,
            targetCenterX,
            targetFloorY,
            targetHeight,
            out Vector3 attackPosition,
            out Vector3 attackScale);
        CalculateSpriteLayout(
            prefabRoot,
            hitSprite,
            targetCenterX,
            targetFloorY,
            targetHeight,
            out Vector3 hitPosition,
            out Vector3 hitScale);

        EnemyCombatVisualController controller =
            prefabRoot.GetComponent<EnemyCombatVisualController>();
        if (controller == null)
        {
            controller =
                prefabRoot.AddComponent<EnemyCombatVisualController>();
        }

        controller.Configure(
            idleVisual.gameObject,
            stateRenderer,
            attackSprite,
            attackPosition,
            attackScale,
            hitSprite,
            hitPosition,
            hitScale);
    }

    private static void GetIdleBounds(
        SkeletonAnimation idleAnimation,
        out float centerX,
        out float floorY,
        out float height)
    {
        Spine.SkeletonData skeletonData =
            idleAnimation.SkeletonDataAsset.GetSkeletonData(true);
        Spine.Skeleton skeleton = new Spine.Skeleton(skeletonData);
        skeleton.UpdateWorldTransform(Spine.Physics.Pose);

        float[] vertices = null;
        skeleton.GetBounds(
            out float minX,
            out float minY,
            out float width,
            out float rawHeight,
            ref vertices);

        Transform visualTransform = idleAnimation.transform;
        float firstX =
            minX * visualTransform.localScale.x +
            visualTransform.localPosition.x;
        float secondX =
            (minX + width) * visualTransform.localScale.x +
            visualTransform.localPosition.x;
        float firstY =
            minY * visualTransform.localScale.y +
            visualTransform.localPosition.y;
        float secondY =
            (minY + rawHeight) * visualTransform.localScale.y +
            visualTransform.localPosition.y;

        centerX = (firstX + secondX) * 0.5f;
        floorY = Mathf.Min(firstY, secondY);
        height = Mathf.Abs(secondY - firstY);
    }

    private static void CalculateSpriteLayout(
        GameObject prefabRoot,
        Sprite sprite,
        float targetCenterX,
        float targetFloorY,
        float targetHeight,
        out Vector3 localPosition,
        out Vector3 localScale)
    {
        GetVisibleSpriteBounds(
            sprite,
            out float minX,
            out float maxX,
            out float minY,
            out float maxY);

        float spriteHeight = Mathf.Max(0.0001f, maxY - minY);
        float scaleY = targetHeight / spriteHeight;
        float rootScaleX = Mathf.Abs(prefabRoot.transform.localScale.x);
        float rootScaleY = Mathf.Abs(prefabRoot.transform.localScale.y);
        float scaleX = scaleY *
            (rootScaleX > 0.0001f ? rootScaleY / rootScaleX : 1f);

        localScale = new Vector3(scaleX, scaleY, 1f);
        localPosition = new Vector3(
            targetCenterX - (minX + maxX) * 0.5f * scaleX,
            targetFloorY - minY * scaleY,
            0f);
    }

    /// <summary>
    /// PNG의 투명 여백을 제외한 실제 불투명 픽셀 영역을
    /// Sprite 로컬 좌표로 변환합니다.
    /// </summary>
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
            byte[] pngBytes = File.ReadAllBytes(assetPath);
            if (!sourceTexture.LoadImage(pngBytes, false))
            {
                GetSpriteVertexBounds(
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
                GetSpriteVertexBounds(
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
            float normalizedMinX =
                (float)visibleMinX / sourceTexture.width;
            float normalizedMaxX =
                (float)(visibleMaxX + 1) / sourceTexture.width;
            float normalizedMinY =
                (float)visibleMinY / sourceTexture.height;
            float normalizedMaxY =
                (float)(visibleMaxY + 1) / sourceTexture.height;

            minX =
                (normalizedMinX * rectWidth - sprite.pivot.x) /
                pixelsPerUnit;
            maxX =
                (normalizedMaxX * rectWidth - sprite.pivot.x) /
                pixelsPerUnit;
            minY =
                (normalizedMinY * rectHeight - sprite.pivot.y) /
                pixelsPerUnit;
            maxY =
                (normalizedMaxY * rectHeight - sprite.pivot.y) /
                pixelsPerUnit;
        }
        finally
        {
            Object.DestroyImmediate(sourceTexture);
        }
    }

    private static void GetSpriteVertexBounds(
        Sprite sprite,
        out float minX,
        out float maxX,
        out float minY,
        out float maxY)
    {
        Vector2[] vertices = sprite.vertices;
        minX = float.PositiveInfinity;
        maxX = float.NegativeInfinity;
        minY = float.PositiveInfinity;
        maxY = float.NegativeInfinity;
        for (int i = 0; i < vertices.Length; i++)
        {
            minX = Mathf.Min(minX, vertices[i].x);
            maxX = Mathf.Max(maxX, vertices[i].x);
            minY = Mathf.Min(minY, vertices[i].y);
            maxY = Mathf.Max(maxY, vertices[i].y);
        }
    }
}
#endif
