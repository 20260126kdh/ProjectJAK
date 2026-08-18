using UnityEngine;

/// <summary>
/// 전투 중 플레이어의 능력치를 관리하는 클래스입니다.
/// HP, Block 등의 전투 수치를 관리하며,
/// 실제 데이터는 PlayerData와 연동합니다.
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    [Header("플레이어 사망 전환")]
    [SerializeField]
    private PlayerDeathTransitionSettings deathTransitionSettings =
        new PlayerDeathTransitionSettings
        {
            redColor = new Color(0.42f, 0.015f, 0.02f, 1f),
            deathDelay = 0.35f,
            harpoonTravelDuration = 0.30f,
            openingDuration = 0.28f,
            finishOpeningDuration = 0.20f,
            deathUiDelay = 0.16f,
            deathUiFadeDuration = 0.22f,
            impactHoldDuration = 0.12f,
            impactSpreadDuration = 0.68f,
            harpoonSize = new Vector2(560f, 280f),
            harpoonHeight = 0f,
            impactOffset = new Vector2(0f, 40f),
            harpoonScreenMargin = 220f,
            finishOpeningLead = 0.55f
        };

    [Header("Player Data")]
    [SerializeField]
    private PlayerData playerData;

    [Header("현재 방어도")]
    [SerializeField]
    private int currentBlock;

    [Header("이번 턴 체력 손실 여부")]
    [SerializeField]
    private bool damagedThisTurn;

    [Header("플레이어 애니메이션")]
    [SerializeField]
    private PlayerAnimationController playerAnimationController;

    /// <summary>
    /// 현재 방어도
    /// </summary>
    public int CurrentBlock => currentBlock;

    /// <summary>
    /// 이번 턴 체력을 잃었는지 여부
    /// </summary>
    public bool DamagedThisTurn => damagedThisTurn;

    private void Awake()
    {
        if (playerData == null)
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError(
                    "[PlayerCombat] GameManager.Instance가 없습니다."
                );

                return;
            }

            playerData = GameManager.Instance.PlayerData;

            if (playerData == null)
            {
                Debug.LogError(
                    "[PlayerCombat] GameManager에서 " +
                    "PlayerData를 찾지 못했습니다."
                );
            }
        }

        if (playerAnimationController == null)
        {
            playerAnimationController =
                GetComponent<PlayerAnimationController>();
        }

        if (playerAnimationController == null)
        {
            playerAnimationController =
                GetComponentInChildren<PlayerAnimationController>();
        }

        if (playerAnimationController == null)
        {
            Debug.LogWarning(
                "[PlayerCombat] " +
                "PlayerAnimationController를 찾지 못했습니다.",
                this
            );
        }
    }

    /// <summary>
    /// 방어도를 획득합니다.
    /// Guard(속도), Paralyze(마비), Cripple(손상),
    /// NoBlock(미끄러짐)을 반영합니다.
    /// </summary>
    public void GainBlock(int amount)
    {
        int finalBlock = amount;

        StatusEffectHandler statusEffectHandler =
            GetComponent<StatusEffectHandler>();

        if (statusEffectHandler != null &&
            statusEffectHandler.HasStatusEffect(
                StatusEffectType.NoBlock
            ))
        {
            Debug.Log(
                "[PlayerCombat] 미끄러짐 적용 : " +
                "방어도를 얻을 수 없습니다."
            );

            return;
        }

        if (statusEffectHandler != null)
        {
            int guardValue =
                statusEffectHandler.GetStatusValue(
                    StatusEffectType.Guard
                );

            int paralyzeValue =
                statusEffectHandler.GetStatusValue(
                    StatusEffectType.Paralyze
                );

            int speedValue =
                guardValue - paralyzeValue;

            finalBlock += speedValue;

            Debug.Log(
                $"[PlayerCombat] 속도/마비 적용 : " +
                $"기본 방어도 {amount} + " +
                $"속도 보정 {speedValue} = {finalBlock}"
            );

            if (statusEffectHandler.HasStatusEffect(
                StatusEffectType.Cripple
            ))
            {
                int reducedBlock =
                    Mathf.FloorToInt(
                        finalBlock * 0.7f
                    );

                Debug.Log(
                    $"[PlayerCombat] 손상 적용 : " +
                    $"{finalBlock} → {reducedBlock}"
                );

                finalBlock = reducedBlock;
            }
        }

        if (finalBlock < 0)
        {
            finalBlock = 0;
        }

        currentBlock += finalBlock;

        Debug.Log(
            $"[PlayerCombat] 방어도 획득 : " +
            $"+{finalBlock} (현재 {currentBlock})"
        );
    }

    /// <summary>
    /// 체력을 잃습니다.
    /// 방어도를 먼저 차감합니다.
    /// Vulnerable(취약), Resist(무감각)을 반영합니다.
    /// </summary>
    public void LoseHealth(int amount)
    {
        damagedThisTurn = true;

        StatusEffectHandler statusEffectHandler =
            GetComponent<StatusEffectHandler>();

        if (statusEffectHandler != null &&
            statusEffectHandler.HasStatusEffect(
                StatusEffectType.Vulnerable
            ))
        {
            int increasedDamage =
                Mathf.FloorToInt(
                    amount * 1.4f
                );

            Debug.Log(
                $"[PlayerCombat] 취약 적용 : " +
                $"{amount} → {increasedDamage}"
            );

            amount = increasedDamage;
        }

        if (statusEffectHandler != null &&
            statusEffectHandler.HasStatusEffect(
                StatusEffectType.Resist
            ))
        {
            int reducedDamage =
                Mathf.FloorToInt(
                    amount * 0.7f
                );

            Debug.Log(
                $"[PlayerCombat] 무감각 적용 : " +
                $"{amount} → {reducedDamage}"
            );

            amount = reducedDamage;
        }

        if (currentBlock > 0)
        {
            int absorbed =
                Mathf.Min(currentBlock, amount);

            currentBlock -= absorbed;
            amount -= absorbed;
        }

        if (amount > 0)
        {
            playerData.TakeDamage(amount);

            ProcessImmortal(statusEffectHandler);
        }

        Debug.Log(
            $"[PlayerCombat] 피해 : {amount}"
        );

        TryStartDeathTransition();
    }

    /// <summary>
    /// 적의 공격 피해를 처리합니다.
    /// 방어도, 선원, 플레이어 체력 순서로 피해를 적용합니다.
    /// </summary>
    public void ReceiveAttackDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        int blockBeforeDamage = currentBlock;
        int crewHealthDamage = 0;
        int playerHealthDamage = 0;

        StatusEffectHandler statusEffectHandler =
            GetComponent<StatusEffectHandler>();

        /*
         * 플레이어에게 걸린 취약은
         * 적 공격 피해를 증가시킵니다.
         */
        if (statusEffectHandler != null &&
            statusEffectHandler.HasStatusEffect(
                StatusEffectType.Vulnerable
            ))
        {
            int increasedDamage =
                Mathf.FloorToInt(
                    amount * 1.4f
                );

            Debug.Log(
                $"[PlayerCombat] 취약 적용 : " +
                $"{amount} → {increasedDamage}"
            );

            amount = increasedDamage;
        }

        /*
         * 무감각은 이번 턴 받는 피해를
         * 30% 감소시킵니다.
         */
        if (statusEffectHandler != null &&
            statusEffectHandler.HasStatusEffect(
                StatusEffectType.Resist
            ))
        {
            int reducedDamage =
                Mathf.FloorToInt(
                    amount * 0.7f
                );

            Debug.Log(
                $"[PlayerCombat] 무감각 적용 : " +
                $"{amount} → {reducedDamage}"
            );

            amount = reducedDamage;
        }

        /*
         * 첫 번째 순서: 플레이어 방어도
         */
        if (currentBlock > 0)
        {
            int absorbedDamage =
                Mathf.Min(currentBlock, amount);

            currentBlock -= absorbedDamage;
            amount -= absorbedDamage;

            Debug.Log(
                $"[PlayerCombat] 방어도 피해 흡수 : " +
                $"{absorbedDamage} / " +
                $"남은 방어도 {currentBlock} / " +
                $"남은 피해 {amount}"
            );
        }

        /*
         * 두 번째 순서: 소환된 선원
         */
        if (amount > 0)
        {
            CrewManager crewManager =
                FindFirstObjectByType<CrewManager>();

            if (crewManager != null)
            {
                int damageBeforeCrews = amount;

                amount =
                    crewManager.AbsorbDamageWithCrews(
                        amount
                    );

                crewHealthDamage =
                    damageBeforeCrews - amount;
            }
        }

        /*
         * 세 번째 순서: 플레이어 체력
         */
        if (amount > 0)
        {
            damagedThisTurn = true;

            int previousHP =
                playerData.CurrentHP;

            playerData.TakeDamage(amount);

            int actualHealthDamage =
                previousHP - playerData.CurrentHP;

            playerHealthDamage = actualHealthDamage;

            ProcessImmortal(statusEffectHandler);

            if (actualHealthDamage > 0 &&
                playerAnimationController != null)
            {
                playerAnimationController.PlayHit();
            }

            Debug.Log(
                $"[PlayerCombat] 플레이어 체력 피해 : " +
                $"요청 피해 {amount} / " +
                $"실제 체력 피해 {actualHealthDamage}"
            );
        }

        if (SFXManager.Instance != null)
        {
            bool wasFullyBlocked =
                blockBeforeDamage > 0 &&
                crewHealthDamage <= 0 &&
                playerHealthDamage <= 0;

            int hitDamage =
                playerHealthDamage > 0
                    ? playerHealthDamage
                    : crewHealthDamage;

            SFXManager.Instance.PlayHitResult(
                hitDamage,
                wasFullyBlocked,
                false,
                playerHealthDamage <= 0 && crewHealthDamage > 0
            );
        }

        TryStartDeathTransition();
    }

    /// <summary>
    /// 불사 처리가 끝난 뒤에도 체력이 0이면
    /// 플레이어 사망 전환을 한 번만 시작합니다.
    /// </summary>
    private void TryStartDeathTransition()
    {
        if (playerData == null ||
            playerData.CurrentHP > 0 ||
            PlayerDeathTransitionController.IsPlaying)
        {
            return;
        }

        PlayerDeathTransitionController.Play(
            deathTransitionSettings,
            transform.position
        );
    }

#if UNITY_EDITOR

    /// <summary>
    /// Editor에서 방어도, 선원과 불사 효과를 우회하고
    /// 플레이어 사망 전환을 즉시 테스트합니다.
    /// </summary>
    public void TriggerDeathForTesting()
    {
        if (playerData == null)
        {
            Debug.LogError(
                "[PlayerCombat] PlayerData가 없어 " +
                "사망 테스트를 실행할 수 없습니다."
            );
            return;
        }

        if (PlayerDeathTransitionController.IsPlaying)
        {
            return;
        }

        playerData.TakeDamage(playerData.CurrentHP);
        TryStartDeathTransition();

        Debug.Log(
            "[PlayerCombat] F9 테스트 단축키 - 플레이어 즉시 사망"
        );
    }

#endif

    /// <summary>
    /// 플레이어가 치명적인 피해를 받았을 때
    /// 불사의 존재를 처리합니다.
    /// 이번 전투에서 처음 체력이 0이 되면
    /// 효과를 소비하고 체력 1로 버팁니다.
    /// </summary>
    private void ProcessImmortal(
        StatusEffectHandler statusEffectHandler)
    {
        if (playerData == null)
        {
            return;
        }

        if (playerData.CurrentHP > 0)
        {
            return;
        }

        if (statusEffectHandler == null)
        {
            return;
        }

        if (!statusEffectHandler.HasStatusEffect(
            StatusEffectType.Immortal
        ))
        {
            return;
        }

        statusEffectHandler.RemoveStatusEffect(
            StatusEffectType.Immortal
        );

        playerData.Heal(1);

        Debug.Log(
            "[PlayerCombat] 불사의 존재 발동 : " +
            "치명적인 피해를 버티고 체력 1 유지"
        );
    }

    /// <summary>
    /// 체력을 회복합니다.
    /// </summary>
    public void Heal(int amount)
    {
        playerData.Heal(amount);

        Debug.Log(
            $"[PlayerCombat] 회복 : {amount}"
        );
    }

    /// <summary>
    /// 방어도를 초기화합니다.
    /// 새 플레이어 턴 시작 시 호출됩니다.
    /// </summary>
    public void ClearBlock()
    {
        currentBlock = 0;

        Debug.Log(
            "[PlayerCombat] 방어도 초기화"
        );
    }

    /// <summary>
    /// 턴 종료 시 호출됩니다.
    /// </summary>
    public void EndTurn()
    {
        damagedThisTurn = false;
    }

    /// <summary>
    /// 전투 종료 시 초기화합니다.
    /// </summary>
    public void ResetCombat()
    {
        currentBlock = 0;
        damagedThisTurn = false;
    }
}
