using UnityEngine;

/// <summary>
/// 폴리싱 테스트 Run의 Unity 난수 Seed를 설정하고 현재 값을 기록합니다.
/// 같은 Seed라도 난수 호출 순서가 달라지면 결과가 달라질 수 있으므로
/// 완전한 재현 여부는 실제 Run 연결 후 별도로 검증합니다.
/// </summary>
public static class PolishTestSeedController
{
    /// <summary>
    /// 현재 Run에 적용한 Seed입니다.
    /// </summary>
    public static int CurrentSeed { get; private set; }

    /// <summary>
    /// 지정한 Seed로 Unity 전역 난수 상태를 초기화합니다.
    /// </summary>
    /// <param name="seed">현재 Run에 사용할 Seed</param>
    public static void Initialize(int seed)
    {
        CurrentSeed = seed;
        Random.InitState(seed);

        Debug.Log($"[PolishTestSeedController] Seed 초기화: {seed}");
    }

    /// <summary>
    /// 기준 Seed, 클래스와 Run 번호로 반복 가능한 Run Seed를 생성합니다.
    /// </summary>
    /// <param name="baseSeed">테스트 묶음의 기준 Seed</param>
    /// <param name="playerClass">테스트 클래스</param>
    /// <param name="runNumber">클래스 내부 Run 번호</param>
    /// <returns>해당 Run에 사용할 Seed</returns>
    public static int CreateRunSeed(
        int baseSeed,
        PlayerClass playerClass,
        int runNumber)
    {
        unchecked
        {
            int seed = baseSeed;
            seed = seed * 397 ^ (int)playerClass;
            seed = seed * 397 ^ runNumber;
            return seed == 0 ? 1 : seed;
        }
    }
}
