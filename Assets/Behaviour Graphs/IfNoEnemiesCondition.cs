using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(
    name: "If no enemies",
    story: "[Enemy_Detector] has no enemies",
    category: "Conditions",
    id: "3b23ef0b20a03e693ca0b54fa14c6a6f"
)]
public partial class IfNoEnemiesCondition : Condition
{
    [SerializeReference]
    public BlackboardVariable<EnemyDetector> Enemy_Detector;

    public override bool IsTrue()
    {
        return Enemy_Detector.Value.GetClosestEnemy() != null;
    }
}
