using NUnit.Framework;
using UnityEngine;

public class PolishHumanActionExecutorTests
{
    private GameObject executorObject;
    private PolishHumanActionExecutor executor;

    [SetUp]
    public void SetUp()
    {
        executorObject = new GameObject("PolishHumanActionExecutorTests");
        executor = executorObject.AddComponent<PolishHumanActionExecutor>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(executorObject);
    }

    [Test]
    public void Execute_NullDecision_ReturnsInvalidDecision()
    {
        PolishActionExecutionResult result = executor.Execute(null);

        Assert.AreEqual(PolishActionExecutionResult.InvalidDecision, result);
    }

    [Test]
    public void Execute_EndTurnWithoutTurnManager_ReturnsMissingComponent()
    {
        PolishHumanDecision decision = new PolishHumanDecision
        {
            decisionType = PolishDecisionType.EndTurn
        };

        PolishActionExecutionResult result = executor.Execute(decision);

        Assert.AreEqual(
            PolishActionExecutionResult.MissingBattleComponent,
            result);
    }

    [Test]
    public void Execute_CardWithoutBattleManagers_ReturnsMissingComponent()
    {
        PolishHumanDecision decision = new PolishHumanDecision
        {
            decisionType = PolishDecisionType.UseCard,
            handIndex = 0,
            target = PolishDecisionTarget.Enemy,
            enemyIndex = 0
        };

        PolishActionExecutionResult result = executor.Execute(decision);

        Assert.AreEqual(
            PolishActionExecutionResult.MissingBattleComponent,
            result);
    }
}
