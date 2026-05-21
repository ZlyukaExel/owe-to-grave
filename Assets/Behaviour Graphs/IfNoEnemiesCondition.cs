using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(
    name: "If no enemies",
    story: "[Enemy_Detector] has no enemies [Must_Have_No_Enemies]",
    category: "Conditions",
    id: "3b23ef0b20a03e693ca0b54fa14c6a6f"
)]
public partial class IfNoEnemiesCondition : Condition
{
    [SerializeReference]
    public BlackboardVariable<EnemyDetector> Enemy_Detector;

    [SerializeReference]
    public BlackboardVariable<bool> Must_Have_No_Enemies;

    public override bool IsTrue()
    {
        return Enemy_Detector.Value.GetClosestEnemy() != Must_Have_No_Enemies.Value;
    }
}
