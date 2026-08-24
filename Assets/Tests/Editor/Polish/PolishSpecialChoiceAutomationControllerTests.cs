using NUnit.Framework;
using UnityEngine;

/// <summary>
/// 계시 자동 선택의 생존 우선순위를 검증합니다.
/// </summary>
public class PolishSpecialChoiceAutomationControllerTests
{
    [Test]
    public void ChooseRevelation_LowHpChoosesBrokenWill()
    {
        GameObject target = new GameObject("HRevelationTest");
        HRevelationController controller = target.AddComponent<HRevelationController>();
        PolishVisibleBattleSnapshot snapshot = new PolishVisibleBattleSnapshot
        {
            playerHp = 20,
            playerMaxHp = 100
        };

        HRevelationType result = PolishSpecialChoiceAutomationController.
            ChooseRevelation(snapshot, controller);

        Assert.AreEqual(HRevelationType.BrokenWill, result);
        Object.DestroyImmediate(target);
    }
}
