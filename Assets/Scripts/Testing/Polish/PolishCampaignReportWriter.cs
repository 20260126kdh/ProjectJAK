#if UNITY_EDITOR || POLISH_SIMULATION_BUILD
using System;
using System.IO;
using System.Text;

/// <summary>
/// 완료된 전체 폴리싱 캠페인의 사람이 읽을 수 있는 최종 보고서를 생성합니다.
/// </summary>
public static class PolishCampaignReportWriter
{
    /// <summary>
    /// 세션 요약 CSV와 최종 Markdown 보고서를 생성합니다.
    /// </summary>
    public static string Write(
        string folder,
        int completedRuns,
        int totalRuns = 90,
        string executionMode = "Normal",
        string selectedClass = "All",
        float elapsedSeconds = 0f)
    {
        Directory.CreateDirectory(folder);
        string runSummary = Path.Combine(folder, "RunSummary.csv");
        string[] lines = File.Exists(runSummary)
            ? File.ReadAllLines(runSummary)
            : Array.Empty<string>();
        int clear = 0, death = 0, error = 0;
        int[] classRuns = new int[3];
        int[] classClears = new int[3];
        for (int i = 1; i < lines.Length; i++)
        {
            int classIndex = lines[i].StartsWith("\"PHY-") ? 0 :
                lines[i].StartsWith("\"TEC-") ? 1 : 2;
            classRuns[classIndex]++;
            if (lines[i].Contains("\"Clear\""))
            {
                clear++;
                classClears[classIndex]++;
            }
            else if (lines[i].Contains("\"Death\"")) death++;
            else if (lines[i].Contains("\"Error\"")) error++;
        }
        string classSummary = Path.Combine(folder, "ClassSummary.csv");
        File.WriteAllText(classSummary,
            "Class,Runs,Clears\n" +
            $"Physique,{classRuns[0]},{classClears[0]}\n" +
            $"Technician,{classRuns[1]},{classClears[1]}\n" +
            $"Captain,{classRuns[2]},{classClears[2]}\n",
            new UTF8Encoding(true));
        string report = Path.Combine(folder, "FinalReport.md");
        File.WriteAllText(report,
            $"# 폴리싱 자동 테스트 결과\n\n" +
            $"- 실행 모드: {executionMode}\n" +
            $"- 대상 클래스: {selectedClass}\n" +
            $"- 실제 소요 시간: {TimeSpan.FromSeconds(elapsedSeconds):hh\\:mm\\:ss}\n" +
            $"- 완료 Run: {completedRuns}/{totalRuns}\n- 클리어: {clear}\n" +
            $"- 사망: {death}\n- 오류: {error}\n\n" +
            $"세부 Run은 `Runs`, 발견 오류는 `IssueSummary.csv`를 확인하세요.\n",
            new UTF8Encoding(true));
        return report;
    }
}
#endif
