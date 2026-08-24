using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// 인간형 폴리싱 테스트 기반의 Seed, Run 발급과 기록 저장을 검증합니다.
/// </summary>
public class PolishTestFoundationTests
{
    private readonly List<string> temporaryFolders = new List<string>();

    [TearDown]
    public void TearDown()
    {
        foreach (string folder in temporaryFolders)
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }

        temporaryFolders.Clear();
    }

    [Test]
    public void CreateRunSeed_SameInput_ReturnsSameSeed()
    {
        int expected = PolishTestSeedController.CreateRunSeed(
            20260822,
            PlayerClass.Physique,
            1);

        for (int iteration = 0; iteration < 50; iteration++)
        {
            int actual = PolishTestSeedController.CreateRunSeed(
                20260822,
                PlayerClass.Physique,
                1);
            Assert.AreEqual(expected, actual);
        }
    }

    [Test]
    public void CreateRunSeed_EntireCampaign_ProducesUniqueSeeds()
    {
        HashSet<int> seeds = new HashSet<int>();
        PlayerClass[] playerClasses =
        {
            PlayerClass.Physique,
            PlayerClass.Technician,
            PlayerClass.Captain
        };

        foreach (PlayerClass playerClass in playerClasses)
        {
            for (int runNumber = 1; runNumber <= 50; runNumber++)
            {
                int seed = PolishTestSeedController.CreateRunSeed(
                    20260822,
                    playerClass,
                    runNumber);
                Assert.IsTrue(
                    seeds.Add(seed),
                    $"Seed 중복: {playerClass}-{runNumber:000} / {seed}");
            }
        }

        Assert.AreEqual(150, seeds.Count);
    }

    [Test]
    public void Initialize_SameSeed_ReproducesUnityRandomSequence()
    {
        const int seed = 19283712;
        PolishTestSeedController.Initialize(seed);
        int[] firstSequence = Enumerable.Range(0, 32)
            .Select(_ => UnityEngine.Random.Range(int.MinValue, int.MaxValue))
            .ToArray();

        PolishTestSeedController.Initialize(seed);
        int[] secondSequence = Enumerable.Range(0, 32)
            .Select(_ => UnityEngine.Random.Range(int.MinValue, int.MaxValue))
            .ToArray();

        CollectionAssert.AreEqual(firstSequence, secondSequence);
    }

    [Test]
    public void Logger_WritesRunBattleTurnAndDeathToJson()
    {
        string outputFolder = CreateTemporaryFolder();
        GameObject testObject = new GameObject("PolishTestLoggerTest");

        try
        {
            PolishTestLogger logger = testObject.AddComponent<PolishTestLogger>();
            logger.SetOutputFolderForTesting(outputFolder);
            logger.BeginRun("PHY-001", PlayerClass.Physique, 19283712);
            logger.BeginBattle(1, 1, "Thief", 75);
            logger.RecordTurn(new PolishTurnRecord
            {
                turn = 1,
                hp = 75,
                block = 5,
                hand = new List<string> { "PHY_ATK_001", "PHY_DEF_001" },
                enemyIntents = new List<string> { "7 Damage" },
                actions = new List<string> { "PHY_DEF_001 -> Player" }
            });
            logger.RecordDeath(new PolishDeathRecord
            {
                stage = 1,
                battle = 1,
                encounter = "Thief",
                turn = 4,
                lastDamage = 8,
                primaryCause = "DefenseShortage"
            });
            logger.CompleteBattle(false, 0, 75, 20, 0);
            logger.CompleteRun(PolishRunResult.Death, 1, 1);

            string jsonPath = Path.Combine(outputFolder, "PHY-001.json");
            Assert.IsTrue(File.Exists(jsonPath));

            PolishRunRecord restored = JsonUtility.FromJson<PolishRunRecord>(
                File.ReadAllText(jsonPath));
            Assert.AreEqual("PHY-001", restored.runId);
            Assert.AreEqual("Death", restored.result);
            Assert.AreEqual(1, restored.battles.Count);
            Assert.AreEqual(1, restored.battles[0].turns.Count);
            Assert.AreEqual("DefenseShortage", restored.death.primaryCause);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(testObject);
        }
    }

    [Test]
    public void Logger_AppendsCsvHeaderOnlyOnce()
    {
        string outputFolder = CreateTemporaryFolder();
        GameObject testObject = new GameObject("PolishTestCsvTest");

        try
        {
            PolishTestLogger logger = testObject.AddComponent<PolishTestLogger>();
            logger.SetOutputFolderForTesting(outputFolder);

            logger.BeginRun("PHY-001", PlayerClass.Physique, 1);
            logger.CompleteRun(PolishRunResult.Clear, 3, 2);
            logger.BeginRun("PHY-002", PlayerClass.Physique, 2);
            logger.CompleteRun(PolishRunResult.Death, 1, 3);

            string[] lines = File.ReadAllLines(
                Path.Combine(outputFolder, "RunSummary.csv"));
            Assert.AreEqual(3, lines.Length);
            Assert.AreEqual(1, lines.Count(line => line.StartsWith("RunId,")));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(testObject);
        }
    }

    [Test]
    public void Logger_RecordsSameIssueOnlyOncePerRun()
    {
        string outputFolder = CreateTemporaryFolder();
        GameObject testObject = new GameObject("PolishIssueDedupeTest");
        try
        {
            PolishTestLogger logger = testObject.AddComponent<PolishTestLogger>();
            logger.SetOutputFolderForTesting(outputFolder);
            logger.BeginRun("PHY-003", PlayerClass.Physique, 3);

            logger.RecordIssue("PHY-003", 3, "반복 오류", "stack-a");
            logger.RecordIssue("PHY-003", 3, "반복 오류", "stack-b");

            string[] lines = File.ReadAllLines(
                Path.Combine(outputFolder, "IssueSummary.csv"));
            Assert.AreEqual(2, lines.Length);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(testObject);
        }
    }

    [Test]
    public void Coordinator_FirstRunContext_IsReproducible()
    {
        (string runId, int seed) first = CreateFirstRunContext();
        (string runId, int seed) second = CreateFirstRunContext();

        Assert.AreEqual("PHY-001", first.runId);
        Assert.AreEqual(first, second);
    }

    [Test]
    public void Coordinator_SingleClass_StopsAfterThirtyRuns()
    {
        string outputFolder = CreateTemporaryFolder();
        GameObject testObject = new GameObject("PolishSingleClassCoordinatorTest");
        try
        {
            PolishTestLogger logger = testObject.AddComponent<PolishTestLogger>();
            logger.SetOutputFolderForTesting(outputFolder);
            PolishTestRunCoordinator coordinator =
                testObject.AddComponent<PolishTestRunCoordinator>();
            FieldInfo loggerField = typeof(PolishTestRunCoordinator).GetField(
                "testLogger",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(loggerField);
            loggerField.SetValue(coordinator, logger);
            Assert.IsTrue(coordinator.ConfigureSingleClass(PlayerClass.Technician));

            for (int run = 1; run <= 30; run++)
            {
                Assert.IsTrue(coordinator.TryBeginNextRun());
                Assert.AreEqual(PlayerClass.Technician, coordinator.CurrentPlayerClass);
                Assert.AreEqual($"TEC-{run:000}", coordinator.CurrentRunId);
                coordinator.CompleteCurrentRun(PolishRunResult.Clear, 3, 2);
            }

            Assert.IsTrue(coordinator.IsCampaignComplete);
            Assert.IsFalse(coordinator.TryBeginNextRun());
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(testObject);
        }
    }

    [Test]
    public void Coordinator_CustomRunCount_StopsAtConfiguredCount()
    {
        GameObject testObject = new GameObject("PolishCustomRunCoordinatorTest");
        try
        {
            PolishTestLogger logger = testObject.AddComponent<PolishTestLogger>();
            logger.SetOutputFolderForTesting(CreateTemporaryFolder());
            PolishTestRunCoordinator coordinator =
                testObject.AddComponent<PolishTestRunCoordinator>();
            FieldInfo loggerField = typeof(PolishTestRunCoordinator).GetField(
                "testLogger",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(loggerField);
            loggerField.SetValue(coordinator, logger);
            Assert.IsTrue(coordinator.ConfigureRunsPerClass(2));
            Assert.IsTrue(coordinator.ConfigureSingleClass(PlayerClass.Captain));

            for (int run = 1; run <= 2; run++)
            {
                Assert.IsTrue(coordinator.TryBeginNextRun());
                Assert.AreEqual($"CAP-{run:000}", coordinator.CurrentRunId);
                coordinator.CompleteCurrentRun(PolishRunResult.Clear, 3, 2);
            }

            Assert.IsTrue(coordinator.IsCampaignComplete);
            Assert.IsFalse(coordinator.TryBeginNextRun());
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(testObject);
        }
    }

    [Test]
    public void SimulationCommandLine_ParsesClassRunsSpeedAndOutput()
    {
        string outputFolder = CreateTemporaryFolder();
        string[] args =
        {
            "ProjectJAK_Simulator.exe",
            "-simulate",
            "-class", "Technician",
            "-runs", "12",
            "-speed", "16",
            "-output", outputFolder
        };

        Assert.IsTrue(PolishSimulationCommandLine.TryParse(
            args,
            out PolishSimulationCommandLine.Settings settings));
        Assert.AreEqual(PlayerClass.Technician, settings.PlayerClass);
        Assert.AreEqual(12, settings.RunsPerClass);
        Assert.AreEqual(16f, settings.PlaybackSpeed);
        Assert.AreEqual(Path.GetFullPath(outputFolder), settings.OutputRootFolder);
    }

    [Test]
    public void SimulationCommandLine_AllClass_UsesNoneAsCampaignSelection()
    {
        Assert.IsTrue(PolishSimulationCommandLine.TryParse(
            new[] { "ProjectJAK_Simulator.exe", "-simulate", "-class", "All" },
            out PolishSimulationCommandLine.Settings settings));
        Assert.AreEqual(PlayerClass.None, settings.PlayerClass);
        Assert.AreEqual(30, settings.RunsPerClass);
        Assert.AreEqual(20f, settings.PlaybackSpeed);
    }

    private (string runId, int seed) CreateFirstRunContext()
    {
        GameObject testObject = new GameObject("PolishCoordinatorTest");
        try
        {
            PolishTestLogger logger = testObject.AddComponent<PolishTestLogger>();
            PolishTestRunCoordinator coordinator =
                testObject.AddComponent<PolishTestRunCoordinator>();
            FieldInfo loggerField = typeof(PolishTestRunCoordinator).GetField(
                "testLogger",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(loggerField);
            loggerField.SetValue(coordinator, logger);

            Assert.IsTrue(coordinator.TryBeginNextRun());
            return (logger.CurrentRun.runId, logger.CurrentRun.seed);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(testObject);
        }
    }

    private string CreateTemporaryFolder()
    {
        string folder = Path.Combine(
            Path.GetTempPath(),
            "ProjectJAK-PolishTest-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        temporaryFolders.Add(folder);
        return folder;
    }
}
