using TMPro;
using UnityEngine;

/// <summary>
/// 적의 현재 방어도를 화면에 표시합니다.
/// 부모 오브젝트에서 Enemy를 자동으로 찾아 연결합니다.
/// </summary>
public class EnemyBlockUI : MonoBehaviour
{
    [Header("방어도 숫자 텍스트")]
    [SerializeField]
    private TMP_Text blockText;

    private Enemy targetEnemy;

    private void Awake()
    {
        targetEnemy =
            GetComponentInParent<Enemy>();

        if (targetEnemy == null)
        {
            Debug.LogWarning(
                "[EnemyBlockUI] 부모 오브젝트에서 Enemy를 찾지 못했습니다.",
                this
            );
        }
    }

    private void OnEnable()
    {
        TrySubscribe();
        RefreshBlock();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    /// <summary>
    /// 대상 적의 방어도 변경 이벤트를 연결합니다.
    /// </summary>
    private void TrySubscribe()
    {
        if (targetEnemy == null)
        {
            return;
        }

        targetEnemy.BlockChanged -= OnBlockChanged;
        targetEnemy.BlockChanged += OnBlockChanged;
    }

    /// <summary>
    /// 대상 적의 방어도 변경 이벤트 연결을 해제합니다.
    /// </summary>
    private void Unsubscribe()
    {
        if (targetEnemy == null)
        {
            return;
        }

        targetEnemy.BlockChanged -= OnBlockChanged;
    }

    /// <summary>
    /// 현재 방어도를 직접 읽어 초기 표시를 갱신합니다.
    /// </summary>
    private void RefreshBlock()
    {
        if (targetEnemy == null)
        {
            return;
        }

        OnBlockChanged(
            targetEnemy.CurrentBlock
        );
    }

    /// <summary>
    /// 방어도 숫자 표시를 갱신합니다.
    /// 방어도가 0이어도 계속 표시합니다.
    /// </summary>
    private void OnBlockChanged(
        int currentBlock)
    {
        if (blockText == null)
        {
            return;
        }

        blockText.text =
            currentBlock.ToString();
    }
}