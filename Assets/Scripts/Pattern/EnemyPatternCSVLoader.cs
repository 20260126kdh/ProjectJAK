using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resources 폴더의 EnemyPatterns.csv를 읽어
/// 적 패턴 데이터를 생성하고 관리합니다.
///
/// CSV 헤더:
/// EnemyId,PatternTurn,ExecutionOrder,ActionType,Value,
/// RepeatCount,TargetType,StatusType,Duration,IsPermanent,
/// ConditionType,ConditionValue
/// </summary>
public class EnemyPatternCSVLoader : MonoBehaviour
{
    [Header("CSV 파일")]
    [SerializeField]
    private TextAsset enemyPatternsCSV;

    [Header("불러온 패턴 데이터")]
    [SerializeField]
    private List<EnemyPatternData> allPatternData =
        new List<EnemyPatternData>();

    [Header("CSV 자동 로드")]
    [SerializeField]
    private bool loadOnAwake = true;

    /// <summary>
    /// 불러온 전체 적 패턴 데이터입니다.
    /// </summary>
    public IReadOnlyList<EnemyPatternData> AllPatternData =>
        allPatternData;

    private void Awake()
    {
        if (loadOnAwake)
        {
            LoadCSV();
        }
    }

    /// <summary>
    /// 연결된 CSV 파일을 읽어 패턴 데이터를 생성합니다.
    /// </summary>
    public void LoadCSV()
    {
        allPatternData.Clear();

        if (enemyPatternsCSV == null)
        {
            /*
             * Inspector 연결이 없다면 Resources 폴더에서
             * EnemyPatterns 파일을 자동으로 찾습니다.
             *
             * 확장자 .csv는 작성하지 않습니다.
             */
            enemyPatternsCSV =
                Resources.Load<TextAsset>(
                    "CSV/EnemyPatterns"
                );
        }

        if (enemyPatternsCSV == null)
        {
            Debug.LogError(
                "[EnemyPatternCSVLoader] " +
                "EnemyPatterns.csv를 찾지 못했습니다.",
                this
            );

            return;
        }

        string[] lines =
            enemyPatternsCSV.text.Split(
                new[]
                {
                    "\r\n",
                    "\n"
                },
                StringSplitOptions.RemoveEmptyEntries
            );

        if (lines.Length <= 1)
        {
            Debug.LogWarning(
                "[EnemyPatternCSVLoader] " +
                "CSV에 패턴 데이터가 없습니다.",
                this
            );

            return;
        }

        /*
         * 0번째 줄은 헤더이므로 1번째 줄부터 읽습니다.
         */
        for (int lineIndex = 1;
             lineIndex < lines.Length;
             lineIndex++)
        {
            string line =
                lines[lineIndex].Trim();

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            /*
             * #으로 시작하는 줄은 주석으로 취급합니다.
             */
            if (line.StartsWith("#"))
            {
                continue;
            }

            if (!TryParseLine(
                    line,
                    lineIndex + 1,
                    out EnemyPatternData patternData
                ))
            {
                continue;
            }

            allPatternData.Add(patternData);
        }

        /*
         * 적 ID → 패턴 턴 → 실행 순서 기준으로 정렬합니다.
         */
        allPatternData.Sort(
            ComparePatternData
        );

        Debug.Log(
            $"[EnemyPatternCSVLoader] " +
            $"적 패턴 데이터 {allPatternData.Count}개 로드 완료",
            this
        );
    }

    /// <summary>
    /// 특정 적의 특정 패턴 턴에 해당하는
    /// 행동 목록을 반환합니다.
    /// </summary>
    public List<EnemyPatternData> GetPatternActions(
        string enemyId,
        int patternTurn)
    {
        List<EnemyPatternData> result =
            new List<EnemyPatternData>();

        if (string.IsNullOrWhiteSpace(enemyId))
        {
            Debug.LogWarning(
                "[EnemyPatternCSVLoader] EnemyId가 비어 있습니다.",
                this
            );

            return result;
        }

        for (int i = 0;
             i < allPatternData.Count;
             i++)
        {
            EnemyPatternData data =
                allPatternData[i];

            if (!string.Equals(
                    data.enemyId,
                    enemyId,
                    StringComparison.OrdinalIgnoreCase
                ))
            {
                continue;
            }

            if (data.patternTurn != patternTurn)
            {
                continue;
            }

            result.Add(data);
        }

        /*
         * 같은 턴의 행동을 실행 순서대로 다시 정렬합니다.
         */
        result.Sort(
            (left, right) =>
                left.executionOrder.CompareTo(
                    right.executionOrder
                )
        );

        return result;
    }

    /// <summary>
    /// 특정 적에게 등록된 가장 큰 패턴 턴 번호를 반환합니다.
    ///
    /// 패턴 데이터가 없다면 0을 반환합니다.
    /// </summary>
    public int GetPatternLength(string enemyId)
    {
        if (string.IsNullOrWhiteSpace(enemyId))
        {
            return 0;
        }

        int maxPatternTurn = 0;

        for (int i = 0;
             i < allPatternData.Count;
             i++)
        {
            EnemyPatternData data =
                allPatternData[i];

            if (!string.Equals(
                    data.enemyId,
                    enemyId,
                    StringComparison.OrdinalIgnoreCase
                ))
            {
                continue;
            }

            if (data.patternTurn > maxPatternTurn)
            {
                maxPatternTurn =
                    data.patternTurn;
            }
        }

        return maxPatternTurn;
    }

    /// <summary>
    /// CSV 한 줄을 EnemyPatternData로 변환합니다.
    /// </summary>
    private bool TryParseLine(
        string line,
        int csvLineNumber,
        out EnemyPatternData patternData)
    {
        patternData = null;

        string[] values =
            line.Split(',');

        const int requiredColumnCount = 12;

        if (values.Length < requiredColumnCount)
        {
            Debug.LogError(
                $"[EnemyPatternCSVLoader] " +
                $"CSV {csvLineNumber}번째 줄의 열이 부족합니다. " +
                $"필요: {requiredColumnCount}, 현재: {values.Length}\n" +
                $"{line}",
                this
            );

            return false;
        }

        for (int i = 0;
             i < values.Length;
             i++)
        {
            values[i] =
                values[i].Trim();
        }

        EnemyPatternData newData =
            new EnemyPatternData();

        newData.enemyId =
            values[0];

        if (string.IsNullOrWhiteSpace(
                newData.enemyId
            ))
        {
            LogParseError(
                csvLineNumber,
                "EnemyId가 비어 있습니다.",
                line
            );

            return false;
        }

        if (!TryParseInt(
                values[1],
                out newData.patternTurn
            ))
        {
            LogParseError(
                csvLineNumber,
                $"PatternTurn 변환 실패: {values[1]}",
                line
            );

            return false;
        }

        if (!TryParseInt(
                values[2],
                out newData.executionOrder
            ))
        {
            LogParseError(
                csvLineNumber,
                $"ExecutionOrder 변환 실패: {values[2]}",
                line
            );

            return false;
        }

        if (!TryParseEnum(
                values[3],
                out newData.actionType
            ))
        {
            LogParseError(
                csvLineNumber,
                $"ActionType 변환 실패: {values[3]}",
                line
            );

            return false;
        }

        if (!TryParseInt(
                values[4],
                out newData.value
            ))
        {
            LogParseError(
                csvLineNumber,
                $"Value 변환 실패: {values[4]}",
                line
            );

            return false;
        }

        if (!TryParseInt(
                values[5],
                out newData.repeatCount
            ))
        {
            LogParseError(
                csvLineNumber,
                $"RepeatCount 변환 실패: {values[5]}",
                line
            );

            return false;
        }

        if (!TryParseEnum(
                values[6],
                out newData.targetType
            ))
        {
            LogParseError(
                csvLineNumber,
                $"TargetType 변환 실패: {values[6]}",
                line
            );

            return false;
        }

        /*
         * StatusType은 빈 값 또는 None을 허용합니다.
         * StatusEffectType enum에 None을 추가하지 않아도 됩니다.
         */
        newData.hasStatusType =
            TryParseOptionalStatusType(
                values[7],
                out newData.statusType
            );

        if (!TryParseInt(
                values[8],
                out newData.duration
            ))
        {
            LogParseError(
                csvLineNumber,
                $"Duration 변환 실패: {values[8]}",
                line
            );

            return false;
        }

        if (!bool.TryParse(
                values[9],
                out newData.isPermanent
            ))
        {
            LogParseError(
                csvLineNumber,
                $"IsPermanent 변환 실패: {values[9]}",
                line
            );

            return false;
        }

        if (!TryParseEnum(
                values[10],
                out newData.conditionType
            ))
        {
            LogParseError(
                csvLineNumber,
                $"ConditionType 변환 실패: {values[10]}",
                line
            );

            return false;
        }

        if (!TryParseInt(
                values[11],
                out newData.conditionValue
            ))
        {
            LogParseError(
                csvLineNumber,
                $"ConditionValue 변환 실패: {values[11]}",
                line
            );

            return false;
        }

        /*
         * 잘못된 반복 횟수는 최소 1로 보정합니다.
         */
        newData.repeatCount =
            Mathf.Max(
                1,
                newData.repeatCount
            );

        if (newData.patternTurn <= 0)
        {
            LogParseError(
                csvLineNumber,
                "PatternTurn은 1 이상이어야 합니다.",
                line
            );

            return false;
        }

        if (newData.executionOrder <= 0)
        {
            LogParseError(
                csvLineNumber,
                "ExecutionOrder는 1 이상이어야 합니다.",
                line
            );

            return false;
        }

        patternData = newData;
        return true;
    }

    /// <summary>
    /// 문자열을 정수로 변환합니다.
    /// </summary>
    private bool TryParseInt(
        string value,
        out int result)
    {
        return int.TryParse(
            value,
            out result
        );
    }

    /// <summary>
    /// 문자열을 지정된 enum으로 변환합니다.
    /// 대소문자를 구분하지 않습니다.
    /// </summary>
    private bool TryParseEnum<TEnum>(
        string value,
        out TEnum result)
        where TEnum : struct
    {
        return Enum.TryParse(
            value,
            true,
            out result
        );
    }

    /// <summary>
    /// 선택적으로 사용하는 상태효과 문자열을 변환합니다.
    ///
    /// 빈 값 또는 None이면 false를 반환합니다.
    /// </summary>
    private bool TryParseOptionalStatusType(
        string value,
        out StatusEffectType statusType)
    {
        statusType = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (string.Equals(
                value,
                "None",
                StringComparison.OrdinalIgnoreCase
            ))
        {
            return false;
        }

        bool parsed =
            Enum.TryParse(
                value,
                true,
                out statusType
            );

        if (!parsed)
        {
            Debug.LogWarning(
                $"[EnemyPatternCSVLoader] " +
                $"등록되지 않은 StatusType입니다: {value}",
                this
            );
        }

        return parsed;
    }

    /// <summary>
    /// 전체 패턴 데이터를 안정적으로 정렬합니다.
    /// </summary>
    private int ComparePatternData(
        EnemyPatternData left,
        EnemyPatternData right)
    {
        int enemyCompare =
            string.Compare(
                left.enemyId,
                right.enemyId,
                StringComparison.OrdinalIgnoreCase
            );

        if (enemyCompare != 0)
        {
            return enemyCompare;
        }

        int turnCompare =
            left.patternTurn.CompareTo(
                right.patternTurn
            );

        if (turnCompare != 0)
        {
            return turnCompare;
        }

        return left.executionOrder.CompareTo(
            right.executionOrder
        );
    }

    /// <summary>
    /// CSV 파싱 오류를 Console에 출력합니다.
    /// </summary>
    private void LogParseError(
        int csvLineNumber,
        string message,
        string originalLine)
    {
        Debug.LogError(
            $"[EnemyPatternCSVLoader] " +
            $"CSV {csvLineNumber}번째 줄 오류: {message}\n" +
            $"{originalLine}",
            this
        );
    }

    /// <summary>
    /// Inspector에서 CSV를 다시 읽습니다.
    /// </summary>
    [ContextMenu("적 패턴 CSV 다시 불러오기")]
    private void TestReloadCSV()
    {
        LoadCSV();
    }

    /// <summary>
    /// Inspector에서 모르바엘의 패턴 데이터를 확인합니다.
    /// </summary>
    [ContextMenu("모르바엘 패턴 데이터 테스트")]
    private void TestMorbaelPatternData()
    {
        const string testEnemyId =
            "Morbael";

        int patternLength =
            GetPatternLength(testEnemyId);

        Debug.Log(
            $"[EnemyPatternCSVLoader] " +
            $"{testEnemyId} 패턴 길이: {patternLength}",
            this
        );

        for (int turn = 1;
             turn <= patternLength;
             turn++)
        {
            List<EnemyPatternData> actions =
                GetPatternActions(
                    testEnemyId,
                    turn
                );

            Debug.Log(
                $"[EnemyPatternCSVLoader] " +
                $"{testEnemyId} {turn}턴 행동 수: " +
                $"{actions.Count}",
                this
            );

            for (int i = 0;
                 i < actions.Count;
                 i++)
            {
                EnemyPatternData action =
                    actions[i];

                Debug.Log(
                    $"[EnemyPatternCSVLoader] " +
                    $"턴 {action.patternTurn} / " +
                    $"순서 {action.executionOrder} / " +
                    $"행동 {action.actionType} / " +
                    $"수치 {action.value} / " +
                    $"반복 {action.repeatCount} / " +
                    $"조건 {action.conditionType}",
                    this
                );
            }
        }
    }
}