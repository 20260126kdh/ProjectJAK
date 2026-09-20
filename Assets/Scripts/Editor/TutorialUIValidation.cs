using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class TutorialUIValidation
{
    public static void Run()
    {
        try
        {
            EditorSceneManager.OpenScene("Assets/Scenes/PlayScene/BattleScene.unity");
            GameObject battle = GameObject.Find("BattlePanel");
            if (battle != null) battle.SetActive(true);
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath("fa9d6f197b499cc45a2f028a860af93a"));
            GameObject worldObject = new GameObject("ValidationWorldUI", typeof(RectTransform), typeof(Canvas));
            Canvas world = worldObject.GetComponent<Canvas>();
            world.renderMode = RenderMode.WorldSpace;
            world.sortingOrder = 7;
            int originalLayer = world.sortingLayerID;
            bool originalOverride = world.overrideSorting;
            GameObject enemyUI = new GameObject("EnemyUI", typeof(RectTransform), typeof(Image));
            enemyUI.transform.SetParent(worldObject.transform, false);
            ((RectTransform)enemyUI.transform).sizeDelta = new Vector2(280, 55);
            worldObject.transform.localScale = Vector3.one * 0.003f;
            GameObject root = new GameObject("ValidationOverview");
            var overview = root.AddComponent<TutorialUIOverview>();
            overview.Show(font, () => {});
            Canvas.ForceUpdateCanvases();
            var dim = root.GetComponentsInChildren<Image>().Single(x => x.name == "Black60Percent");
            Check(Mathf.Approximately(dim.color.a, 0.6f), "Black dim alpha = 0.6");
            Check(world.sortingOrder > dim.canvas.sortingOrder, "World UI draws after dim");
            var notes = root.GetComponentsInChildren<RectTransform>().Where(x => x.name.StartsWith("Note_")).ToArray();
            Check(notes.Length >= 6, "At least six scene UI explanations found: " + notes.Length);
            for (int i = 0; i < notes.Length; i++)
            {
                Rect a = new Rect(notes[i].anchoredPosition - notes[i].sizeDelta / 2, notes[i].sizeDelta);
                for (int j = i + 1; j < notes.Length; j++)
                {
                    Rect b = new Rect(notes[j].anchoredPosition - notes[j].sizeDelta / 2, notes[j].sizeDelta);
                    Check(!a.Overlaps(b), notes[i].name + " / " + notes[j].name + " do not overlap");
                }
                TMP_Text text = notes[i].GetComponentInChildren<TMP_Text>();
                text.ForceMeshUpdate();
                Check(!text.isTextOverflowing, notes[i].name + " text fits");
            }
            RectTransform target = GameObject.Find("AttackDefenseUseCountPanel").GetComponent<RectTransform>();
            GameObject border = TutorialUIOverview.CreateRedBorder(target);
            Check(border.GetComponentsInChildren<Image>().Length == 4, "Count border has four edges");
            Check(border.GetComponentsInChildren<Image>().All(x => !x.raycastTarget), "Count border does not intercept input");
            root.SetActive(false);
            Check(world.sortingOrder == 7 && world.sortingLayerID == originalLayer && world.overrideSorting == originalOverride, "Original canvas state restored");
            Debug.Log("TUTORIAL_UI_VALIDATION_PASSED");
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }
    private static void Check(bool result, string name)
    {
        if (!result) throw new Exception("Validation failed: " + name);
        Debug.Log("UI_CHECK_OK: " + name);
    }
}
