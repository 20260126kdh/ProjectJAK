#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 보스 프리팹의 기존 전투 구성을 보존하면서 사용 가능한 Spine 표시 오브젝트를 연결합니다.
/// </summary>
[InitializeOnLoad]
public static class BossSpinePrefabSetup
{
    private const string MenuPath = "Tools/ProjectJAK/Apply Available Boss Spine";
    private const string VisualName = "SpineVisual";
    private const string AmphitriteAttackSkeletonDataPath =
        "Assets/Art/Enemy/Spine/Monster_Spine/Attack/Attack_Boss/Boss 2st/Ampitrite_Attack/Ampitrite_SkeletonData.asset";
    private const float HumanBossTargetHeight = 1.485f;
    private const float HumanBossFloorOffset = -0.1f;
    private const float HumanBossUiHeight = 0.58f;
    private const float BossCaptainVisualOffset = -0.4f;
    private const float BossCaptainUiOffset = -0.2f;
    private const float HelmsmanHorizontalOffset = 0.2f;
    private const float AmphitriteTargetHeight = 1.8f;
    private const float AmphitriteFloorOffset = -0.12f;
    private const float AmphitriteHorizontalOffset = -0.15f;
    private const float AspidocheloneTargetHeight = 2.1f;
    private const float AspidocheloneFloorOffset = -0.25f;
    private const float AspidocheloneHorizontalOffset = -0.2f;
    private const float MorbaelTargetHeight = 2.625f;
    private const float MorbaelFloorOffset = -1f;
    private const float MorbaelHorizontalOffset = -0.25f;
    private const float ArielTargetHeight = 2.52f;
    private const float ArielFloorOffset = -0.65f;
    private const float ArielHorizontalOffset = -0.2f;
    private static readonly Vector3 ShipwreckVisualPosition = new Vector3(-0.65f, -0.55f, 0f);
    private static readonly Vector3 ShipwreckVisualScale = new Vector3(0.42708f, 0.3007f, 1f);
    private static readonly Vector3 ShipwreckUiPosition = new Vector3(-1f, 1.15f, 0f);
    private static readonly Vector3 AmphitriteUiPosition = new Vector3(-0.1f, 0.82f, 0f);
    private static readonly Vector3 AspidocheloneUiPosition = new Vector3(-0.2f, 0.95f, 0f);
    private static readonly Vector3 MorbaelUiPosition = new Vector3(-0.25f, 1.05f, 0f);
    private static readonly Vector3 ArielUiPosition = new Vector3(-0.2f, 1.15f, 0f);

    private sealed class SetupEntry
    {
        public string PrefabPath { get; }
        public string SkeletonDataPath { get; }

        public SetupEntry(string prefabPath, string skeletonDataPath)
        {
            PrefabPath = prefabPath;
            SkeletonDataPath = skeletonDataPath;
        }
    }

    private static readonly IReadOnlyList<SetupEntry> Entries = new[]
    {
        new SetupEntry(
            "Assets/Prefabs/Enemy/BossBattle/1Stage/BossCaptain.prefab",
            "Assets/Art/Enemy/Spine/Monster_Spine/Idle/Idle_Boss/Boss 1st/BossCaptain/ghost2_SkeletonData.asset"),
        new SetupEntry(
            "Assets/Prefabs/Enemy/BossBattle/1Stage/Helmsman.prefab",
            "Assets/Art/Enemy/Spine/Monster_Spine/Idle/Idle_Boss/Boss 1st/Helmsman/ghost1_SkeletonData.asset"),
        new SetupEntry(
            "Assets/Prefabs/Enemy/BossBattle/1Stage/SecondMate.prefab",
            "Assets/Art/Enemy/Spine/Monster_Spine/Idle/Idle_Boss/Boss 1st/SecondMate/ghost3_SkeletonData.asset"),
        new SetupEntry(
            "Assets/Prefabs/Enemy/BossBattle/1Stage/ShipwreckCrab.prefab",
            "Assets/Art/Enemy/Spine/Monster_Spine/Idle/Idle_Boss/Boss 1st/ShipwreckCrab/shipcrab_SkeletonData.asset"),
        new SetupEntry(
            "Assets/Prefabs/Enemy/BossBattle/2Stage/Amphitrite.prefab",
            "Assets/Art/Enemy/Spine/Monster_Spine/Idle/Idle_Boss/Boss 2st/Ampitrite_Idle/Ampitrite_SkeletonData.asset"),
        new SetupEntry(
            "Assets/Prefabs/Enemy/BossBattle/2Stage/Aspidochelone.prefab",
            "Assets/Art/Enemy/Spine/Monster_Spine/Idle/Idle_Boss/Boss 2st/Aspidochelone/whaleW_SkeletonData.asset"),
        new SetupEntry(
            "Assets/Prefabs/Enemy/BossBattle/3Stage/Morbael.prefab",
            "Assets/Art/Enemy/Spine/Monster_Spine/Idle/Idle_Boss/Boss 3st/Morbael/whaleB_SkeletonData.asset"),
        new SetupEntry(
            "Assets/Prefabs/Enemy/BossBattle/3Stage/Ariel.prefab",
            "Assets/Art/Enemy/Spine/Monster_Spine/Idle/Idle_Boss/Boss 3st/Ariel/Ariel_SkeletonData.asset")
    };

    static BossSpinePrefabSetup()
    {
        EditorApplication.delayCall += ApplyMissingSetups;
    }

    [MenuItem(MenuPath)]
    public static void ApplyAll()
    {
        ApplySetups(onlyMissing: false);
    }

    private static void ApplyMissingSetups()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            return;

        ApplySetups(onlyMissing: true);
    }

    private static void ApplySetups(bool onlyMissing)
    {
        int appliedCount = 0;

        foreach (SetupEntry entry in Entries)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(entry.PrefabPath);
            SkeletonDataAsset skeletonDataAsset =
                AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(entry.SkeletonDataPath);

            if (prefabAsset == null)
            {
                Debug.LogError($"[BossSpinePrefabSetup] 프리팹을 찾지 못했습니다: {entry.PrefabPath}");
                continue;
            }

            if (skeletonDataAsset == null)
            {
                if (!onlyMissing)
                {
                    Debug.LogError($"[BossSpinePrefabSetup] SkeletonData를 찾지 못했습니다: {entry.SkeletonDataPath}");
                }
                continue;
            }

            if (onlyMissing)
            {
                Transform visual = prefabAsset.transform.Find(VisualName);
                SkeletonRenderer renderer = visual != null ? visual.GetComponent<SkeletonRenderer>() : null;
                GetLayout(prefabAsset, skeletonDataAsset, out Vector3 expectedPosition,
                    out Vector3 expectedScale, out Vector3 expectedUiPosition);
                bool sizeAdjusted = visual != null &&
                                    Vector3.Distance(visual.localScale, expectedScale) < 0.0001f;
                bool positionAdjusted = visual != null &&
                                        Vector3.Distance(visual.localPosition, expectedPosition) < 0.0001f;
                Transform uiRoot = prefabAsset.transform.Find("EnemyUIRoot");
                bool uiAdjusted = uiRoot != null &&
                                  Vector3.Distance(uiRoot.localPosition, expectedUiPosition) < 0.0001f;
                bool attackAnimationAdjusted = prefabAsset.name != "Amphitrite" ||
                    prefabAsset.GetComponent<EnemySpineAnimationController>() != null;
                if (renderer != null && renderer.SkeletonDataAsset == skeletonDataAsset &&
                    sizeAdjusted && positionAdjusted && uiAdjusted && attackAnimationAdjusted)
                    continue;
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(entry.PrefabPath);
            try
            {
                ConfigurePrefab(prefabRoot, skeletonDataAsset);
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
            Debug.Log($"[BossSpinePrefabSetup] 보스 Spine 적용 완료: {appliedCount}/{Entries.Count}");
        }
    }

    private static void ConfigurePrefab(GameObject prefabRoot, SkeletonDataAsset skeletonDataAsset)
    {
        SpriteRenderer legacyRenderer = prefabRoot.GetComponent<SpriteRenderer>();
        if (legacyRenderer != null)
            legacyRenderer.enabled = false;

        Transform existingVisual = prefabRoot.transform.Find(VisualName);
        GameObject visualObject;
        if (existingVisual == null)
        {
            visualObject = new GameObject(VisualName);
            visualObject.transform.SetParent(prefabRoot.transform, false);
        }
        else
        {
            visualObject = existingVisual.gameObject;
        }

        GetLayout(prefabRoot, skeletonDataAsset, out Vector3 visualPosition,
            out Vector3 visualScale, out Vector3 uiPosition);
        visualObject.transform.localPosition = visualPosition;
        visualObject.transform.localRotation = Quaternion.identity;
        visualObject.transform.localScale = visualScale;

        bool wasActive = visualObject.activeSelf;
        visualObject.SetActive(false);

        SkeletonRenderer skeletonRenderer = visualObject.GetComponent<SkeletonRenderer>();
        SkeletonAnimation skeletonAnimation = visualObject.GetComponent<SkeletonAnimation>();
        if (skeletonRenderer == null || skeletonAnimation == null)
        {
            DestroySpineComponents(visualObject);
            visualObject.AddComponent<MeshFilter>();
            visualObject.AddComponent<MeshRenderer>();
            skeletonRenderer = visualObject.AddComponent<SkeletonRenderer>();
            skeletonAnimation = visualObject.AddComponent<SkeletonAnimation>();
        }

        string idleAnimation = FindIdleAnimation(skeletonDataAsset);
        SerializedObject rendererProperties = new SerializedObject(skeletonRenderer);
        rendererProperties.FindProperty("skeletonDataAsset").objectReferenceValue = skeletonDataAsset;
        SetBooleanIfPresent(rendererProperties, "wasDeprecatedTransferred", true);
        rendererProperties.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject animationProperties = new SerializedObject(skeletonAnimation);
        animationProperties.FindProperty("animationName").stringValue = idleAnimation;
        animationProperties.FindProperty("loop").boolValue = true;
        SetBooleanIfPresent(animationProperties, "wasDeprecatedTransferred", true);
        animationProperties.ApplyModifiedPropertiesWithoutUndo();

        skeletonRenderer.Animation = skeletonAnimation;

        if (prefabRoot.name == "Amphitrite")
        {
            ConfigureAmphitriteAttack(
                prefabRoot,
                skeletonAnimation,
                skeletonDataAsset,
                visualPosition,
                visualScale);
        }

        MeshRenderer meshRenderer = visualObject.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingLayerID = legacyRenderer != null ? legacyRenderer.sortingLayerID : 0;
            meshRenderer.sortingOrder = legacyRenderer != null ? legacyRenderer.sortingOrder : 1;
        }

        visualObject.SetActive(wasActive);

        Transform uiRoot = prefabRoot.transform.Find("EnemyUIRoot");
        if (uiRoot != null)
            uiRoot.localPosition = uiPosition;
    }

    private static void ConfigureAmphitriteAttack(
        GameObject prefabRoot,
        SkeletonAnimation skeletonAnimation,
        SkeletonDataAsset idleSkeletonData,
        Vector3 idlePosition,
        Vector3 idleScale)
    {
        SkeletonDataAsset attackSkeletonData =
            AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(
                AmphitriteAttackSkeletonDataPath);
        if (attackSkeletonData == null)
        {
            Debug.LogError(
                $"[BossSpinePrefabSetup] 암피트리테 공격 SkeletonData를 찾지 못했습니다: " +
                AmphitriteAttackSkeletonDataPath);
            return;
        }

        CalculateBoundsLayout(
            prefabRoot,
            attackSkeletonData,
            AmphitriteTargetHeight,
            AmphitriteFloorOffset,
            AmphitriteHorizontalOffset,
            out Vector3 attackPosition,
            out Vector3 attackScale);

        EnemySpineAnimationController animationController =
            prefabRoot.GetComponent<EnemySpineAnimationController>();
        if (animationController == null)
        {
            animationController =
                prefabRoot.AddComponent<EnemySpineAnimationController>();
        }

        animationController.Configure(
            skeletonAnimation,
            idleSkeletonData,
            attackSkeletonData,
            idlePosition,
            idleScale,
            attackPosition,
            attackScale);
    }

    private static void SetBooleanIfPresent(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
            property.boolValue = value;
    }

    private static void GetLayout(GameObject prefabRoot, SkeletonDataAsset skeletonDataAsset,
        out Vector3 visualPosition, out Vector3 visualScale, out Vector3 uiPosition)
    {
        if (prefabRoot.name == "ShipwreckCrab")
        {
            visualPosition = ShipwreckVisualPosition;
            visualScale = ShipwreckVisualScale;
            uiPosition = ShipwreckUiPosition;
            return;
        }

        if (prefabRoot.name == "Amphitrite")
        {
            CalculateBoundsLayout(prefabRoot, skeletonDataAsset, AmphitriteTargetHeight,
                AmphitriteFloorOffset, AmphitriteHorizontalOffset,
                out visualPosition, out visualScale);
            uiPosition = AmphitriteUiPosition;
            return;
        }

        if (prefabRoot.name == "Aspidochelone")
        {
            CalculateBoundsLayout(prefabRoot, skeletonDataAsset,
                AspidocheloneTargetHeight, AspidocheloneFloorOffset,
                AspidocheloneHorizontalOffset, out visualPosition,
                out visualScale);
            uiPosition = AspidocheloneUiPosition;
            return;
        }

        if (prefabRoot.name == "Morbael")
        {
            CalculateBoundsLayout(prefabRoot, skeletonDataAsset, MorbaelTargetHeight,
                MorbaelFloorOffset, MorbaelHorizontalOffset,
                out visualPosition, out visualScale);
            uiPosition = MorbaelUiPosition;
            return;
        }

        if (prefabRoot.name == "Ariel")
        {
            CalculateBoundsLayout(prefabRoot, skeletonDataAsset, ArielTargetHeight,
                ArielFloorOffset, ArielHorizontalOffset,
                out visualPosition, out visualScale);
            uiPosition = ArielUiPosition;
            return;
        }

        float visualHorizontalOffset = prefabRoot.name switch
        {
            "BossCaptain" => BossCaptainVisualOffset,
            "Helmsman" => HelmsmanHorizontalOffset,
            _ => 0f
        };
        float uiHorizontalOffset = prefabRoot.name switch
        {
            "BossCaptain" => BossCaptainUiOffset,
            "Helmsman" => HelmsmanHorizontalOffset,
            _ => 0f
        };
        CalculateBoundsLayout(prefabRoot, skeletonDataAsset, HumanBossTargetHeight,
            HumanBossFloorOffset, visualHorizontalOffset,
            out visualPosition, out visualScale);
        uiPosition = new Vector3(uiHorizontalOffset, HumanBossUiHeight, 0f);
    }

    /// <summary>
    /// Spine 원본 비율을 유지하면서 최저점을 전투 바닥선에 맞춥니다.
    /// 루트의 비균등 스케일을 보정하여 화면에서 캐릭터가 찌그러지지 않게 합니다.
    /// </summary>
    private static void CalculateBoundsLayout(GameObject prefabRoot,
        SkeletonDataAsset skeletonDataAsset, float targetHeight, float floorOffset,
        float horizontalOffset, out Vector3 visualPosition, out Vector3 visualScale)
    {
        Spine.SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(true);
        Spine.Skeleton skeleton = new Spine.Skeleton(skeletonData);
        skeleton.UpdateWorldTransform(Spine.Physics.Pose);

        float[] vertices = null;
        skeleton.GetBounds(out float minX, out float minY, out float width,
            out float height, ref vertices);
        float scaleY = height > 0.0001f ? targetHeight / height : 0.075175f;
        float rootScaleX = Mathf.Abs(prefabRoot.transform.localScale.x);
        float rootScaleY = Mathf.Abs(prefabRoot.transform.localScale.y);
        float aspectCompensation = rootScaleX > 0.0001f
            ? rootScaleY / rootScaleX
            : 1f;
        float scaleX = scaleY * aspectCompensation;

        visualScale = new Vector3(scaleX, scaleY, 1f);
        visualPosition = new Vector3(
            -(minX + width * 0.5f) * scaleX + horizontalOffset,
            -minY * scaleY + floorOffset,
            0f);
    }

    private static void DestroySpineComponents(GameObject visualObject)
    {
        SkeletonAnimation animation = visualObject.GetComponent<SkeletonAnimation>();
        SkeletonRenderer renderer = visualObject.GetComponent<SkeletonRenderer>();
        MeshRenderer meshRenderer = visualObject.GetComponent<MeshRenderer>();
        MeshFilter meshFilter = visualObject.GetComponent<MeshFilter>();

        if (animation != null)
            UnityEngine.Object.DestroyImmediate(animation);
        if (renderer != null)
            UnityEngine.Object.DestroyImmediate(renderer);
        if (meshRenderer != null)
            UnityEngine.Object.DestroyImmediate(meshRenderer);
        if (meshFilter != null)
            UnityEngine.Object.DestroyImmediate(meshFilter);
    }

    private static string FindIdleAnimation(SkeletonDataAsset skeletonDataAsset)
    {
        Spine.SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(true);
        if (skeletonData == null || skeletonData.Animations.Count == 0)
            return string.Empty;

        for (int i = 0; i < skeletonData.Animations.Count; i++)
        {
            string animationName = skeletonData.Animations.Items[i].Name;
            if (animationName.IndexOf("idle", StringComparison.OrdinalIgnoreCase) >= 0)
                return animationName;
        }

        return skeletonData.Animations.Items[0].Name;
    }
}
#endif
