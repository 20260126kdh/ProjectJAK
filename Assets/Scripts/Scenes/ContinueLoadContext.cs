/// <summary>
/// TitleScene에서 불러온 이어하기 데이터를
/// BattleScene으로 전달하기 위한 임시 저장소입니다.
///
/// 실제 저장 파일 관리는 SaveManager가 담당합니다.
/// 이 클래스는 씬 전환 사이에 불러온 데이터를 잠시 유지합니다.
///
/// 이어하기 흐름:
/// 1. TitleScene에서 저장 데이터 불러오기
/// 2. SetPendingSaveData()로 임시 저장
/// 3. BattleScene 이동
/// 4. DeckManager가 PendingSaveData를 사용해 덱 복원
/// 5. 복원 완료 후 Clear() 호출
/// </summary>
public static class ContinueLoadContext
{
    /// <summary>
    /// 현재 이어하기로 불러오는 중인 저장 데이터입니다.
    /// </summary>
    private static GameSaveData pendingSaveData;

    /// <summary>
    /// 현재 이어하기 데이터가 준비되어 있는지 반환합니다.
    /// </summary>
    public static bool HasPendingSaveData =>
        pendingSaveData != null;

    /// <summary>
    /// 이어하기에 사용할 저장 데이터를 임시 보관합니다.
    /// </summary>
    /// <param name="saveData">불러온 저장 데이터</param>
    /// <returns>저장 성공 여부</returns>
    public static bool SetPendingSaveData(
        GameSaveData saveData)
    {
        if (saveData == null)
        {
            UnityEngine.Debug.LogError(
                "[ContinueLoadContext] 저장할 이어하기 데이터가 없습니다."
            );

            return false;
        }

        pendingSaveData =
            saveData;

        UnityEngine.Debug.Log(
            "[ContinueLoadContext] 이어하기 데이터 임시 저장 완료"
        );

        return true;
    }

    /// <summary>
    /// 현재 임시 저장된 이어하기 데이터를 반환합니다.
    ///
    /// 데이터를 제거하지 않으므로,
    /// 여러 복원 단계에서 동일한 데이터를 확인할 수 있습니다.
    /// </summary>
    public static GameSaveData GetPendingSaveData()
    {
        return pendingSaveData;
    }

    /// <summary>
    /// 현재 이어하기 데이터를 안전하게 가져옵니다.
    /// </summary>
    /// <param name="saveData">현재 임시 저장 데이터</param>
    /// <returns>데이터 존재 여부</returns>
    public static bool TryGetPendingSaveData(
        out GameSaveData saveData)
    {
        saveData =
            pendingSaveData;

        return saveData != null;
    }

    /// <summary>
    /// 이어하기 데이터 사용이 완료되면
    /// 임시 저장 데이터를 제거합니다.
    /// </summary>
    public static void Clear()
    {
        pendingSaveData =
            null;

        UnityEngine.Debug.Log(
            "[ContinueLoadContext] 이어하기 임시 데이터 제거"
        );
    }
}