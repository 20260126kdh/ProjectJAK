#if UNITY_EDITOR || POLISH_SIMULATION_BUILD

using System;
using System.Globalization;
using System.IO;
using UnityEngine;

/// <summary>
/// 독립 시뮬레이터 빌드의 명령줄 인수를 해석하고 자동 캠페인을 시작합니다.
/// </summary>
public static class PolishSimulationCommandLine
{
    private const int DefaultRunsPerClass = 30;
    private const float DefaultPlaybackSpeed = 20f;
    private const int SimulationWindowWidth = 1280;
    private const int SimulationWindowHeight = 720;

    /// <summary>
    /// 현재 프로세스가 명령줄에서 독립 시뮬레이션 실행을 요청했는지 반환합니다.
    /// </summary>
    public static bool IsSimulationRequested =>
        HasArgument(Environment.GetCommandLineArgs(), "-simulate");

    /// <summary>
    /// 현재 명령줄 설정으로 타이틀 화면에서 자동 캠페인을 시작합니다.
    /// </summary>
    /// <param name="titleManager">시뮬레이션 전용 새 게임을 시작할 타이틀 매니저</param>
    /// <returns>인수가 유효하고 자동 캠페인을 시작했다면 true</returns>
    public static bool TryStart(TitleManager titleManager)
    {
        if (!TryParse(Environment.GetCommandLineArgs(), out Settings settings))
        {
            return false;
        }

        Screen.fullScreenMode = FullScreenMode.Windowed;
        Screen.SetResolution(
            SimulationWindowWidth,
            SimulationWindowHeight,
            false);

        Debug.Log(
            $"[PolishSimulationCommandLine] 독립 시뮬레이션 시작 / " +
            $"Class: {settings.PlayerClass} / Runs: {settings.RunsPerClass} / " +
            $"Speed: x{settings.PlaybackSpeed} / Output: {settings.OutputRootFolder}");

        return PolishCampaignController.TryStartFromTitle(
            titleManager,
            true,
            settings.PlayerClass,
            settings.RunsPerClass,
            settings.PlaybackSpeed,
            settings.OutputRootFolder,
            true);
    }

    /// <summary>
    /// 전달된 명령줄 인수를 시뮬레이션 설정으로 변환합니다.
    /// </summary>
    /// <param name="args">실행 파일에 전달된 전체 명령줄 인수</param>
    /// <param name="settings">검증이 끝난 시뮬레이션 설정</param>
    /// <returns>필수 플래그와 모든 값이 유효하다면 true</returns>
    public static bool TryParse(string[] args, out Settings settings)
    {
        settings = default;
        if (!HasArgument(args, "-simulate"))
        {
            return false;
        }

        PlayerClass playerClass = PlayerClass.None;
        string classValue = GetValue(args, "-class");
        if (!string.IsNullOrWhiteSpace(classValue) &&
            !string.Equals(classValue, "All", StringComparison.OrdinalIgnoreCase) &&
            !Enum.TryParse(classValue, true, out playerClass))
        {
            Debug.LogError($"[PolishSimulationCommandLine] 지원하지 않는 클래스: {classValue}");
            return false;
        }

        if (playerClass == PlayerClass.All)
        {
            playerClass = PlayerClass.None;
        }
        if (playerClass != PlayerClass.None &&
            playerClass != PlayerClass.Physique &&
            playerClass != PlayerClass.Technician &&
            playerClass != PlayerClass.Captain)
        {
            Debug.LogError($"[PolishSimulationCommandLine] 지원하지 않는 클래스: {classValue}");
            return false;
        }

        int runs = DefaultRunsPerClass;
        string runsValue = GetValue(args, "-runs");
        if (!string.IsNullOrWhiteSpace(runsValue) &&
            (!int.TryParse(runsValue, NumberStyles.Integer,
                CultureInfo.InvariantCulture, out runs) || runs <= 0))
        {
            Debug.LogError($"[PolishSimulationCommandLine] 잘못된 Run 횟수: {runsValue}");
            return false;
        }

        float speed = DefaultPlaybackSpeed;
        string speedValue = GetValue(args, "-speed");
        if (!string.IsNullOrWhiteSpace(speedValue) &&
            (!float.TryParse(speedValue, NumberStyles.Float,
                CultureInfo.InvariantCulture, out speed) || speed < 1f))
        {
            Debug.LogError($"[PolishSimulationCommandLine] 잘못된 배속: {speedValue}");
            return false;
        }

        string outputRoot = GetValue(args, "-output");
        if (string.IsNullOrWhiteSpace(outputRoot))
        {
            outputRoot = Path.Combine(
                Application.persistentDataPath,
                "PolishSimulationResults");
        }
        else
        {
            try
            {
                outputRoot = Path.GetFullPath(outputRoot);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[PolishSimulationCommandLine] 잘못된 결과 경로: " +
                    $"{outputRoot} / {exception.Message}");
                return false;
            }
        }

        settings = new Settings(playerClass, runs, speed, outputRoot);
        return true;
    }

    private static bool HasArgument(string[] args, string name)
    {
        if (args == null)
        {
            return false;
        }
        for (int i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private static string GetValue(string[] args, string name)
    {
        if (args == null)
        {
            return null;
        }
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }
        return null;
    }

    /// <summary>
    /// 검증된 독립 시뮬레이션 실행 설정입니다.
    /// </summary>
    public readonly struct Settings
    {
        /// <summary>단독 실행 클래스이며 None이면 전체 클래스입니다.</summary>
        public PlayerClass PlayerClass { get; }
        /// <summary>클래스마다 실행할 Run 수입니다.</summary>
        public int RunsPerClass { get; }
        /// <summary>시뮬레이션 배속입니다.</summary>
        public float PlaybackSpeed { get; }
        /// <summary>세션 결과를 기록할 루트 폴더입니다.</summary>
        public string OutputRootFolder { get; }

        /// <summary>
        /// 검증된 시뮬레이션 설정을 생성합니다.
        /// </summary>
        public Settings(
            PlayerClass playerClass,
            int runsPerClass,
            float playbackSpeed,
            string outputRootFolder)
        {
            PlayerClass = playerClass;
            RunsPerClass = runsPerClass;
            PlaybackSpeed = playbackSpeed;
            OutputRootFolder = outputRootFolder;
        }
    }
}

#endif
