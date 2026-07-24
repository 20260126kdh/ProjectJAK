using System;
using UnityEngine;

/// <summary>
/// 스테이지 번호에 따라 전투 배경과 바닥 Sprite를 변경합니다.
/// BattleScene의 BackGrounds 오브젝트에 부착합니다.
/// </summary>
public class StageBackgroundController : MonoBehaviour
{
    [Serializable]
    private class StageBackgroundData
    {
        [Header("적용할 스테이지")]
        [Min(1)]
        public int stage = 1;

        [Header("배경 이미지")]
        public Sprite backSprite;

        [Header("바닥 이미지")]
        public Sprite floorSprite;
    }

    [Header("배경 SpriteRenderer")]
    [SerializeField]
    private SpriteRenderer backRenderer;

    [SerializeField]
    private SpriteRenderer floorRenderer;

    [Header("스테이지별 배경")]
    [SerializeField]
    private StageBackgroundData[] stageBackgrounds;

    private void Start()
    {
        ApplyCurrentStageBackground();
    }

    /// <summary>
    /// 현재 StageManager의 스테이지에 맞는 배경을 적용합니다.
    /// </summary>
    public void ApplyCurrentStageBackground()
    {
        if (StageManager.Instance == null)
        {
            Debug.LogWarning(
                "[StageBackgroundController] StageManager.Instance를 찾지 못했습니다."
            );
            return;
        }

        ApplyBackground(StageManager.Instance.CurrentStage);
    }

    /// <summary>
    /// 전달받은 스테이지 번호에 맞는 배경을 적용합니다.
    /// </summary>
    public void ApplyBackground(int stage)
    {
        if (backRenderer == null || floorRenderer == null)
        {
            Debug.LogWarning(
                "[StageBackgroundController] Back 또는 Floor SpriteRenderer가 연결되지 않았습니다."
            );
            return;
        }

        if (stageBackgrounds == null || stageBackgrounds.Length == 0)
        {
            Debug.LogWarning(
                "[StageBackgroundController] 스테이지 배경 데이터가 없습니다."
            );
            return;
        }

        StageBackgroundData selectedData = null;

        for (int i = 0; i < stageBackgrounds.Length; i++)
        {
            StageBackgroundData data = stageBackgrounds[i];

            if (data != null && data.stage == stage)
            {
                selectedData = data;
                break;
            }
        }

        if (selectedData == null)
        {
            Debug.LogWarning(
                $"[StageBackgroundController] Stage {stage}의 배경 데이터를 찾지 못했습니다."
            );
            return;
        }

        if (selectedData.backSprite != null)
        {
            backRenderer.sprite = selectedData.backSprite;
        }
        else
        {
            Debug.LogWarning(
                $"[StageBackgroundController] Stage {stage}의 Back Sprite가 없습니다."
            );
        }

        if (selectedData.floorSprite != null)
        {
            floorRenderer.sprite = selectedData.floorSprite;
        }
        else
        {
            Debug.LogWarning(
                $"[StageBackgroundController] Stage {stage}의 Floor Sprite가 없습니다."
            );
        }

        Debug.Log(
            $"[StageBackgroundController] Stage {stage} 배경 적용 완료"
        );
    }
}