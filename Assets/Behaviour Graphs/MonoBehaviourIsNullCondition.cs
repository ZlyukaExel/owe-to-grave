using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(
    name: "MonoBehaviour is null",
    story: "[MonoBehaviour] is null",
    category: "Conditions",
    id: "7d618970cf64abf6830d07f91cf2798f"
)]
public partial class MonoBehaviourIsNullCondition : Condition
{
    [SerializeReference]
    public BlackboardVariable<MonoBehaviour> MonoBehaviour;

    public override bool IsTrue()
    {
        return MonoBehaviour.Value == null;
    }
}
