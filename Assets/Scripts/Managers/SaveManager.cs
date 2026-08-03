using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 게임의 이어하기 저장 파일을 관리합니다.
///
/// 담당 기능:
/// - 현재 게임 진행 데이터 수집
/// - GameSaveData를 JSON 파일로 저장
/// - 저장된 JSON 파일 불러오기
/// - 저장 데이터 존재 여부 확인
/// - 저장 데이터 삭제
/// - 손상된 저장 파일 예외 처리
///
/// 전투 내부 상태는 저장하지 않습니다.
/// 이어하기 시 저장된 진행 위치의 전투를 처음부터 시작합니다.
/// </summary>
public class SaveManager : MonoBehaviour
{
    #region Singleton

    public static SaveManager Instance { get; private set; }

    #endregion

    #region Constants

    /// <summary>
    /// 현재 지원하는 저장 데이터 버전입니다.
    /// </summary>
    private const int CurrentSaveVersion = 1;

    #endregion

    #region Inspector

    [Header("저장 파일 설정")]

    [Tooltip("저장 파일 이름입니다.")]
    [SerializeField]
    private string saveFileName = "game_save.json";

    [Header("Debug")]

    [SerializeField]
    private bool debugMode = true;

    #endregion

    #region Property

    /// <summary>
    /// 실제 저장 파일의 전체 경로입니다.
    /// </summary>
    public string SaveFilePath =>
        Path.Combine(
            Application.persistentDataPath,
            saveFileName
        );

    /// <summary>
    /// 이어하기 가능한 유효한 저장 데이터가 있는지 반환합니다.
    /// </summary>
    public bool HasValidSaveData =>
        TryLoadSaveData(
            out _,
            false
        );

    #endregion

    #region Unity

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        ValidateSaveFileName();
    }

    #endregion

    #region Current Game Save

    /// <summary>
    /// 현재 게임의 플레이어, 스테이지, 덱 데이터를 수집하여 저장합니다.
    ///
    /// BattleScene의 안전한 저장 시점에서 호출합니다.
    /// 전투 도중 손패, 적 체력, 상태 효과 등은 저장하지 않습니다.
    /// </summary>
    /// <returns>저장 성공 여부</returns>
    public bool SaveCurrentGame()
    {
        if (!TryCreateCurrentSaveData(
                out GameSaveData saveData))
        {
            Debug.LogError(
                "[SaveManager] 현재 게임 데이터를 생성하지 못해 " +
                "저장을 취소합니다."
            );

            return false;
        }

        return SaveGame(
            saveData
        );
    }

    /// <summary>
    /// 현재 게임의 각 매니저에서 이어하기 저장 데이터를 수집합니다.
    /// </summary>
    /// <param name="saveData">생성한 저장 데이터</param>
    /// <returns>데이터 생성 성공 여부</returns>
    public bool TryCreateCurrentSaveData(
        out GameSaveData saveData)
    {
        saveData = null;

        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "[SaveManager] GameManager.Instance를 찾지 못했습니다."
            );

            return false;
        }

        PlayerData playerData =
            GameManager.Instance.PlayerData;

        if (playerData == null)
        {
            Debug.LogError(
                "[SaveManager] PlayerData를 찾지 못했습니다."
            );

            return false;
        }

        StageManager stageManager =
            StageManager.Instance;

        if (stageManager == null)
        {
            stageManager =
                FindFirstObjectByType<StageManager>();
        }

        if (stageManager == null)
        {
            Debug.LogError(
                "[SaveManager] StageManager를 찾지 못했습니다."
            );

            return false;
        }

        DeckManager deckManager =
            FindFirstObjectByType<DeckManager>();

        if (deckManager == null)
        {
            Debug.LogError(
                "[SaveManager] DeckManager를 찾지 못했습니다."
            );

            return false;
        }

        if (!ValidateCurrentGameForSave(
                playerData,
                stageManager,
                deckManager))
        {
            return false;
        }

        saveData =
            new GameSaveData
            {
                saveVersion =
                    CurrentSaveVersion,

                playerClass =
                    (int)playerData.PlayerClass,

                currentHP =
                    playerData.CurrentHP,

                maxHP =
                    playerData.MaxHP,

                currentStage =
                    stageManager.CurrentStage,

                currentBattleCount =
                    stageManager.CurrentBattleCount,

                currentPhase =
                    (int)stageManager.CurrentPhase,

                currentBossSequence =
                    stageManager.CurrentBossSequence,

                isGameClear =
                    stageManager.IsGameClear
            };

        for (int i = 0;
             i < deckManager.CurrentDeck.Count;
             i++)
        {
            CardData card =
                deckManager.CurrentDeck[i];

            if (card == null)
            {
                Debug.LogWarning(
                    $"[SaveManager] 현재 덱의 {i}번 카드가 null이라 " +
                    "저장 목록에서 제외합니다."
                );

                continue;
            }

            if (string.IsNullOrWhiteSpace(
                    card.cardID))
            {
                Debug.LogError(
                    $"[SaveManager] 저장할 카드의 ID가 비어 있습니다: " +
                    $"{card.cardName}"
                );

                saveData = null;
                return false;
            }

            SavedCardData savedCard =
                new SavedCardData(
                    card.cardID,
                    card.IsUpgraded
                );

            saveData.cards.Add(
                savedCard
            );
        }

        if (saveData.cards.Count <= 0)
        {
            Debug.LogError(
                "[SaveManager] 저장할 카드가 없어 저장 데이터를 " +
                "생성할 수 없습니다."
            );

            saveData = null;
            return false;
        }

        if (debugMode)
        {
            int upgradedCardCount = 0;

            for (int i = 0;
                 i < saveData.cards.Count;
                 i++)
            {
                if (saveData.cards[i].isUpgraded)
                {
                    upgradedCardCount++;
                }
            }

            Debug.Log(
                $"[SaveManager] 현재 게임 데이터 수집 완료 / " +
                $"Class: {(PlayerClass)saveData.playerClass} / " +
                $"HP: {saveData.currentHP}/{saveData.maxHP} / " +
                $"Stage: {saveData.currentStage} / " +
                $"Battle: {saveData.currentBattleCount} / " +
                $"Phase: {(StagePhase)saveData.currentPhase} / " +
                $"Cards: {saveData.cards.Count} / " +
                $"Upgraded: {upgradedCardCount}"
            );
        }

        return true;
    }

    /// <summary>
    /// 현재 게임이 이어하기 저장 가능한 상태인지 확인합니다.
    /// </summary>
    private bool ValidateCurrentGameForSave(
        PlayerData playerData,
        StageManager stageManager,
        DeckManager deckManager)
    {
        if (playerData.PlayerClass ==
                PlayerClass.None ||
            playerData.PlayerClass ==
                PlayerClass.All)
        {
            Debug.LogWarning(
                $"[SaveManager] 선택 가능한 클래스가 설정되지 않아 " +
                $"저장할 수 없습니다: {playerData.PlayerClass}"
            );

            return false;
        }

        if (playerData.MaxHP <= 0 ||
            playerData.CurrentHP <= 0 ||
            playerData.CurrentHP >
            playerData.MaxHP)
        {
            Debug.LogWarning(
                $"[SaveManager] 플레이어 체력이 유효하지 않습니다: " +
                $"{playerData.CurrentHP}/{playerData.MaxHP}"
            );

            return false;
        }

        if (stageManager.IsGameClear)
        {
            Debug.LogWarning(
                "[SaveManager] 게임 클리어 상태는 " +
                "이어하기 데이터로 저장하지 않습니다."
            );

            return false;
        }

        if (deckManager.CurrentDeck == null ||
            deckManager.CurrentDeck.Count <= 0)
        {
            Debug.LogWarning(
                "[SaveManager] 현재 덱이 비어 있어 저장할 수 없습니다."
            );

            return false;
        }

        return true;
    }

    #endregion

    #region Save File

    /// <summary>
    /// 전달받은 게임 데이터를 JSON 파일로 저장합니다.
    /// </summary>
    /// <param name="saveData">저장할 게임 데이터</param>
    /// <returns>저장 성공 여부</returns>
    public bool SaveGame(
        GameSaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogError(
                "[SaveManager] 저장할 GameSaveData가 없습니다."
            );

            return false;
        }

        ValidateSaveFileName();

        try
        {
            saveData.saveVersion =
                CurrentSaveVersion;

            saveData.savedAt =
                DateTime.Now.ToString(
                    "yyyy-MM-dd HH:mm:ss"
                );

            string json =
                JsonUtility.ToJson(
                    saveData,
                    true
                );

            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogError(
                    "[SaveManager] 저장 JSON 생성에 실패했습니다."
                );

                return false;
            }

            string directoryPath =
                Path.GetDirectoryName(
                    SaveFilePath
                );

            if (!string.IsNullOrEmpty(directoryPath) &&
                !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(
                    directoryPath
                );
            }

            string temporaryFilePath =
                SaveFilePath + ".tmp";

            File.WriteAllText(
                temporaryFilePath,
                json
            );

            if (File.Exists(SaveFilePath))
            {
                File.Delete(
                    SaveFilePath
                );
            }

            File.Move(
                temporaryFilePath,
                SaveFilePath
            );

            if (debugMode)
            {
                Debug.Log(
                    $"[SaveManager] 게임 저장 완료\n" +
                    $"경로: {SaveFilePath}\n" +
                    $"저장 시각: {saveData.savedAt}"
                );
            }

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[SaveManager] 게임 저장 중 오류가 발생했습니다.\n" +
                $"{exception}"
            );

            return false;
        }
    }

    /// <summary>
    /// 저장 파일을 읽어 GameSaveData로 반환합니다.
    /// </summary>
    public GameSaveData LoadSaveData()
    {
        bool succeeded =
            TryLoadSaveData(
                out GameSaveData saveData,
                true
            );

        return succeeded
            ? saveData
            : null;
    }

    /// <summary>
    /// 저장 데이터를 안전하게 불러옵니다.
    /// </summary>
    public bool TryLoadSaveData(
        out GameSaveData saveData)
    {
        return TryLoadSaveData(
            out saveData,
            true
        );
    }

    /// <summary>
    /// 저장 파일의 존재 여부만 반환합니다.
    /// </summary>
    public bool SaveFileExists()
    {
        ValidateSaveFileName();

        return File.Exists(
            SaveFilePath
        );
    }

    /// <summary>
    /// 이어하기 저장 파일과 임시 파일을 삭제합니다.
    /// </summary>
    public bool DeleteSaveData()
    {
        ValidateSaveFileName();

        try
        {
            bool deletedAnyFile = false;

            if (File.Exists(SaveFilePath))
            {
                File.Delete(
                    SaveFilePath
                );

                deletedAnyFile = true;
            }

            string temporaryFilePath =
                SaveFilePath + ".tmp";

            if (File.Exists(
                    temporaryFilePath))
            {
                File.Delete(
                    temporaryFilePath
                );

                deletedAnyFile = true;
            }

            if (debugMode)
            {
                Debug.Log(
                    deletedAnyFile
                        ? "[SaveManager] 저장 데이터 삭제 완료"
                        : "[SaveManager] 삭제할 저장 데이터가 없습니다."
                );
            }

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[SaveManager] 저장 데이터 삭제 중 " +
                $"오류가 발생했습니다.\n" +
                $"{exception}"
            );

            return false;
        }
    }

    #endregion

    #region Load Validation

    /// <summary>
    /// 저장 파일을 읽고 유효성을 검사합니다.
    /// </summary>
    private bool TryLoadSaveData(
        out GameSaveData saveData,
        bool logMessage)
    {
        saveData = null;

        ValidateSaveFileName();

        if (!File.Exists(SaveFilePath))
        {
            if (logMessage && debugMode)
            {
                Debug.Log(
                    "[SaveManager] 저장 파일이 없습니다."
                );
            }

            return false;
        }

        try
        {
            string json =
                File.ReadAllText(
                    SaveFilePath
                );

            if (string.IsNullOrWhiteSpace(json))
            {
                if (logMessage)
                {
                    Debug.LogWarning(
                        "[SaveManager] 저장 파일이 비어 있습니다."
                    );
                }

                return false;
            }

            saveData =
                JsonUtility.FromJson<GameSaveData>(
                    json
                );

            if (!ValidateSaveData(
                    saveData,
                    logMessage))
            {
                saveData = null;
                return false;
            }

            if (logMessage && debugMode)
            {
                Debug.Log(
                    $"[SaveManager] 저장 데이터 불러오기 완료\n" +
                    $"저장 시각: {saveData.savedAt}\n" +
                    $"Stage: {saveData.currentStage}\n" +
                    $"Battle: {saveData.currentBattleCount}\n" +
                    $"카드 수: {saveData.cards.Count}"
                );
            }

            return true;
        }
        catch (Exception exception)
        {
            if (logMessage)
            {
                Debug.LogError(
                    $"[SaveManager] 저장 파일을 불러오는 중 " +
                    $"오류가 발생했습니다.\n" +
                    $"{exception}"
                );
            }

            saveData = null;
            return false;
        }
    }

    /// <summary>
    /// 불러온 저장 데이터의 기본 유효성을 검사합니다.
    /// </summary>
    private bool ValidateSaveData(
        GameSaveData saveData,
        bool logMessage)
    {
        if (saveData == null)
        {
            if (logMessage)
            {
                Debug.LogWarning(
                    "[SaveManager] 저장 데이터 변환에 실패했습니다."
                );
            }

            return false;
        }

        if (saveData.saveVersion <= 0 ||
            saveData.saveVersion >
            CurrentSaveVersion)
        {
            if (logMessage)
            {
                Debug.LogWarning(
                    $"[SaveManager] 지원하지 않는 저장 버전입니다. " +
                    $"저장 버전: {saveData.saveVersion} / " +
                    $"지원 버전: {CurrentSaveVersion}"
                );
            }

            return false;
        }

        if (!Enum.IsDefined(
                typeof(PlayerClass),
                saveData.playerClass))
        {
            if (logMessage)
            {
                Debug.LogWarning(
                    $"[SaveManager] 잘못된 플레이어 클래스입니다: " +
                    $"{saveData.playerClass}"
                );
            }

            return false;
        }

        PlayerClass savedPlayerClass =
            (PlayerClass)saveData.playerClass;

        if (savedPlayerClass ==
                PlayerClass.None ||
            savedPlayerClass ==
                PlayerClass.All)
        {
            if (logMessage)
            {
                Debug.LogWarning(
                    $"[SaveManager] 이어하기에 사용할 수 없는 " +
                    $"플레이어 클래스입니다: {savedPlayerClass}"
                );
            }

            return false;
        }

        if (!Enum.IsDefined(
                typeof(StagePhase),
                saveData.currentPhase))
        {
            if (logMessage)
            {
                Debug.LogWarning(
                    $"[SaveManager] 잘못된 StagePhase입니다: " +
                    $"{saveData.currentPhase}"
                );
            }

            return false;
        }

        if (saveData.currentStage < 1 ||
            saveData.currentStage > 3)
        {
            if (logMessage)
            {
                Debug.LogWarning(
                    $"[SaveManager] 잘못된 스테이지 값입니다: " +
                    $"{saveData.currentStage}"
                );
            }

            return false;
        }

        if (saveData.currentBattleCount < 0)
        {
            if (logMessage)
            {
                Debug.LogWarning(
                    $"[SaveManager] 잘못된 전투 횟수입니다: " +
                    $"{saveData.currentBattleCount}"
                );
            }

            return false;
        }

        if (saveData.maxHP <= 0)
        {
            if (logMessage)
            {
                Debug.LogWarning(
                    $"[SaveManager] 잘못된 최대 체력입니다: " +
                    $"{saveData.maxHP}"
                );
            }

            return false;
        }

        if (saveData.currentHP <= 0 ||
            saveData.currentHP >
            saveData.maxHP)
        {
            if (logMessage)
            {
                Debug.LogWarning(
                    $"[SaveManager] 잘못된 현재 체력입니다: " +
                    $"{saveData.currentHP}/{saveData.maxHP}"
                );
            }

            return false;
        }

        if (saveData.isGameClear)
        {
            if (logMessage)
            {
                Debug.LogWarning(
                    "[SaveManager] 게임 클리어 저장 데이터는 " +
                    "이어하기에 사용할 수 없습니다."
                );
            }

            return false;
        }

        if (saveData.cards == null ||
            saveData.cards.Count == 0)
        {
            if (logMessage)
            {
                Debug.LogWarning(
                    "[SaveManager] 저장된 카드가 없습니다."
                );
            }

            return false;
        }

        for (int i = 0;
             i < saveData.cards.Count;
             i++)
        {
            SavedCardData savedCard =
                saveData.cards[i];

            if (savedCard == null ||
                string.IsNullOrWhiteSpace(
                    savedCard.cardID))
            {
                if (logMessage)
                {
                    Debug.LogWarning(
                        $"[SaveManager] {i}번 카드 저장 정보가 " +
                        "유효하지 않습니다."
                    );
                }

                return false;
            }
        }

        return true;
    }

    #endregion

    #region Utility

    /// <summary>
    /// 저장 파일이 생성되는 폴더를 운영체제 탐색기로 엽니다.
    /// </summary>
    [ContextMenu("저장 폴더 열기")]
    public void OpenSaveFolder()
    {
        string directoryPath =
            Application.persistentDataPath;

        Application.OpenURL(
            "file://" + directoryPath
        );

        if (debugMode)
        {
            Debug.Log(
                $"[SaveManager] 저장 폴더 열기: " +
                $"{directoryPath}"
            );
        }
    }

    /// <summary>
    /// Inspector Context Menu에서 저장 데이터를 삭제합니다.
    /// </summary>
    [ContextMenu("저장 데이터 삭제")]
    private void DeleteSaveDataFromContextMenu()
    {
        DeleteSaveData();
    }

    /// <summary>
    /// 저장 파일 이름을 검증하고 필요하면 확장자를 추가합니다.
    /// </summary>
    private void ValidateSaveFileName()
    {
        if (string.IsNullOrWhiteSpace(
                saveFileName))
        {
            saveFileName =
                "game_save.json";

            return;
        }

        if (!saveFileName.EndsWith(
                ".json",
                StringComparison.OrdinalIgnoreCase))
        {
            saveFileName +=
                ".json";
        }
    }

    #endregion

#if UNITY_EDITOR

    private void OnValidate()
    {
        ValidateSaveFileName();
    }

#endif
}