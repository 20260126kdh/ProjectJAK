using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// 폴리싱 테스트의 Run, 전투, 턴과 사망 정보를 메모리에 기록하고
/// 재현 가능한 JSON 및 집계용 CSV 파일로 저장합니다.
/// </summary>
public class PolishTestLogger : MonoBehaviour
{
    [SerializeField] private string outputFolderName = "PolishTestResults";

    private PolishRunRecord currentRun;
    private PolishBattleRecord currentBattle;
    private string outputFolderOverride;

    /// <summary>
    /// 현재 기록 중인 Run입니다.
    /// </summary>
    public PolishRunRecord CurrentRun => currentRun;

#if UNITY_EDITOR

    /// <summary>
    /// EditMode 테스트가 실제 사용자 기록 폴더를 오염시키지 않도록
    /// 현재 Logger의 출력 경로를 테스트 전용 폴더로 변경합니다.
    /// </summary>
    /// <param name="outputFolder">테스트 결과를 저장할 절대 경로</param>
    public void SetOutputFolderForTesting(string outputFolder)
    {
        outputFolderOverride = outputFolder;
    }

#endif

    /// <summary>
    /// 새 Run 기록을 시작합니다.
    /// </summary>
    /// <param name="runId">클래스와 순번을 포함한 Run ID</param>
    /// <param name="playerClass">테스트 클래스</param>
    /// <param name="seed">Run 재현에 사용할 Seed</param>
    public void BeginRun(string runId, PlayerClass playerClass, int seed)
    {
        currentRun = new PolishRunRecord
        {
            runId = runId,
            playerClass = playerClass.ToString(),
            seed = seed,
            result = PolishRunResult.InProgress.ToString(),
            startedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
        };
        currentBattle = null;
    }

    /// <summary>
    /// 현재 Run에 전투 기록을 추가하고 기록을 시작합니다.
    /// </summary>
    public void BeginBattle(
        int stage,
        int battleNumber,
        string encounter,
        int entryHp)
    {
        if (currentRun == null)
        {
            Debug.LogError("[PolishTestLogger] 시작된 Run이 없어 전투를 기록할 수 없습니다.", this);
            return;
        }

        currentBattle = new PolishBattleRecord
        {
            stage = stage,
            battleNumber = battleNumber,
            encounter = encounter ?? string.Empty,
            entryHp = entryHp
        };
        currentRun.battles.Add(currentBattle);
    }

    /// <summary>
    /// 플레이어에게 공개된 현재 상태와 AutoPlayer 행동을 턴 기록으로 남깁니다.
    /// </summary>
    public void RecordTurn(PolishTurnRecord turnRecord)
    {
        if (currentBattle == null || turnRecord == null)
        {
            return;
        }

        currentBattle.turns.Add(turnRecord);
        currentBattle.turnCount = currentBattle.turns.Count;
        currentRun.totalTurns++;
    }

    /// <summary>
    /// 현재 전투의 누적 수치와 결과를 확정합니다.
    /// </summary>
    public void CompleteBattle(
        bool won,
        int exitHp,
        int damageTaken,
        int blockGained,
        int healed)
    {
        if (currentBattle == null)
        {
            return;
        }

        currentBattle.won = won;
        currentBattle.exitHp = exitHp;
        currentBattle.damageTaken = damageTaken;
        currentBattle.blockGained = blockGained;
        currentBattle.healed = healed;
        currentRun.totalBattles++;
        currentBattle = null;
    }

    /// <summary>
    /// 사망 당시 정보와 사망 원인 분류를 현재 Run에 기록합니다.
    /// </summary>
    public void RecordDeath(PolishDeathRecord deathRecord)
    {
        if (currentRun != null)
        {
            currentRun.death = deathRecord;
        }
    }

    /// <summary>
    /// 현재 Run을 종료하고 JSON 및 CSV로 저장합니다.
    /// </summary>
    public void CompleteRun(
        PolishRunResult result,
        int finalStage,
        int finalBattle,
        string errorMessage = "")
    {
        if (currentRun == null)
        {
            return;
        }

        currentRun.result = result.ToString();
        currentRun.finalStage = finalStage;
        currentRun.finalBattle = finalBattle;
        currentRun.errorMessage = errorMessage ?? string.Empty;
        currentRun.finishedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);

        WriteRunJson(currentRun);
        AppendRunCsv(currentRun);
        currentBattle = null;
        currentRun = null;
    }

    private string GetOutputFolder()
    {
        string outputFolder = string.IsNullOrWhiteSpace(outputFolderOverride)
            ? Path.Combine(Application.persistentDataPath, outputFolderName)
            : outputFolderOverride;
        Directory.CreateDirectory(outputFolder);
        return outputFolder;
    }

    private void WriteRunJson(PolishRunRecord runRecord)
    {
        string path = Path.Combine(GetOutputFolder(), $"{runRecord.runId}.json");
        File.WriteAllText(path, JsonUtility.ToJson(runRecord, true), Encoding.UTF8);
    }

    private void AppendRunCsv(PolishRunRecord runRecord)
    {
        string path = Path.Combine(GetOutputFolder(), "RunSummary.csv");
        bool writeHeader = !File.Exists(path);
        using StreamWriter writer = new StreamWriter(path, true, new UTF8Encoding(true));
        if (writeHeader)
        {
            writer.WriteLine("RunId,Class,Seed,Result,FinalStage,FinalBattle,TotalBattle,TotalTurn,Error");
        }

        writer.WriteLine(string.Join(",",
            EscapeCsv(runRecord.runId),
            EscapeCsv(runRecord.playerClass),
            runRecord.seed.ToString(CultureInfo.InvariantCulture),
            EscapeCsv(runRecord.result),
            runRecord.finalStage.ToString(CultureInfo.InvariantCulture),
            runRecord.finalBattle.ToString(CultureInfo.InvariantCulture),
            runRecord.totalBattles.ToString(CultureInfo.InvariantCulture),
            runRecord.totalTurns.ToString(CultureInfo.InvariantCulture),
            EscapeCsv(runRecord.errorMessage)));
    }

    private static string EscapeCsv(string value)
    {
        string safeValue = value ?? string.Empty;
        return $"\"{safeValue.Replace("\"", "\"\"")}\"";
    }
}

/// <summary>
/// 폴리싱 테스트 Run의 종료 상태입니다.
/// </summary>
public enum PolishRunResult
{
    InProgress,
    Clear,
    Death,
    Error,
    Invalid
}

/// <summary>
/// 한 번의 전체 게임 Run 기록입니다.
/// </summary>
[Serializable]
public class PolishRunRecord
{
    public string runId;
    public string playerClass;
    public int seed;
    public string result;
    public int finalStage;
    public int finalBattle;
    public int totalBattles;
    public int totalTurns;
    public string startedAtUtc;
    public string finishedAtUtc;
    public string errorMessage;
    public List<PolishBattleRecord> battles = new List<PolishBattleRecord>();
    public PolishDeathRecord death;
}

/// <summary>
/// Run 내부의 전투별 기록입니다.
/// </summary>
[Serializable]
public class PolishBattleRecord
{
    public int stage;
    public int battleNumber;
    public string encounter;
    public bool won;
    public int entryHp;
    public int exitHp;
    public int damageTaken;
    public int blockGained;
    public int healed;
    public int turnCount;
    public List<PolishTurnRecord> turns = new List<PolishTurnRecord>();
}

/// <summary>
/// 플레이어에게 공개된 정보와 AutoPlayer가 선택한 행동의 턴 기록입니다.
/// </summary>
[Serializable]
public class PolishTurnRecord
{
    public int turn;
    public int hp;
    public int block;
    public int harpoonStack;
    public int crewCount;
    public List<string> hand = new List<string>();
    public List<string> enemyIntents = new List<string>();
    public List<string> buffs = new List<string>();
    public List<string> debuffs = new List<string>();
    public List<string> actions = new List<string>();
    public string observation;
}

/// <summary>
/// 사망 시점의 상세 상태와 사망 원인 분류입니다.
/// </summary>
[Serializable]
public class PolishDeathRecord
{
    public int stage;
    public int battle;
    public string encounter;
    public int turn;
    public int hp;
    public int block;
    public int lastDamage;
    public string damageSource;
    public string primaryCause;
    public string secondaryCause;
    public List<string> hand = new List<string>();
    public List<string> drawPile = new List<string>();
    public List<string> discardPile = new List<string>();
    public List<string> statusEffects = new List<string>();
    public List<string> recentActions = new List<string>();
}
