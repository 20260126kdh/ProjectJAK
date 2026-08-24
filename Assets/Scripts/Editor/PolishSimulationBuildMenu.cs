using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 일반 플레이 빌드와 분리된 Windows용 폴리싱 시뮬레이터를 생성합니다.
/// </summary>
internal static class PolishSimulationBuildMenu
{
    private const string SimulationDefine = "POLISH_SIMULATION_BUILD";
    private const string ExecutableName = "ProjectJAK_Simulator.exe";

    [MenuItem("Tools/Build/Simulation Build")]
    private static void BuildSimulationPlayer()
    {
        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
        string defaultFolder = Path.Combine(
            projectRoot ?? Application.dataPath,
            "Builds",
            "Simulation");
        string outputFolder = EditorUtility.SaveFolderPanel(
            "시뮬레이터 빌드 저장 폴더",
            defaultFolder,
            string.Empty);
        if (string.IsNullOrWhiteSpace(outputFolder))
        {
            return;
        }

        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled && !string.IsNullOrWhiteSpace(scene.path))
            .Select(scene => scene.path)
            .ToArray();
        if (scenes.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "시뮬레이터 빌드 실패",
                "Build Settings에 활성화된 Scene이 없습니다.",
                "확인");
            return;
        }

        Directory.CreateDirectory(outputFolder);
        string executablePath = Path.Combine(outputFolder, ExecutableName);
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = executablePath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
            extraScriptingDefines = new[] { SimulationDefine }
        };

        Debug.Log(
            $"[PolishSimulationBuildMenu] 시뮬레이터 빌드 시작: " +
            $"{executablePath}");
        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        if (summary.result != BuildResult.Succeeded)
        {
            Debug.LogError(
                $"[PolishSimulationBuildMenu] 빌드 실패: {summary.result} / " +
                $"Errors: {summary.totalErrors}");
            EditorUtility.DisplayDialog(
                "시뮬레이터 빌드 실패",
                $"결과: {summary.result}\n오류: {summary.totalErrors}\n" +
                "Console과 Editor.log를 확인하세요.",
                "확인");
            return;
        }

        string exampleCommand =
            $"\"{executablePath}\" -simulate -class Physique " +
            "-runs 30 -speed 20";
        Debug.Log(
            $"[PolishSimulationBuildMenu] 빌드 완료: {executablePath}\n" +
            $"실행 예시: {exampleCommand}");
        EditorUtility.DisplayDialog(
            "시뮬레이터 빌드 완료",
            $"저장 위치:\n{executablePath}\n\n" +
            $"실행 예시:\n{exampleCommand}",
            "확인");
        EditorUtility.RevealInFinder(executablePath);
    }
}
