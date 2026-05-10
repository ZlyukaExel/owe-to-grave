using System;
using Unity.Behavior;
using Unity.Properties;
using Composite = Unity.Behavior.Composite;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Any Result Sequence",
    story: "Execute children until they all finish, regardless of success or failure",
    category: "Flow",
    id: "cc9eb2b0624a287c496f049eabad19ea"
)]
public partial class AnyResultSequence : Composite
{
    [CreateProperty]
    int m_CurrentChild;

    protected override Status OnStart()
    {
        m_CurrentChild = 0;
        if (Children.Count == 0)
            return Status.Success;

        return StartChild(m_CurrentChild);
    }

    protected override Status OnUpdate()
    {
        var currentChild = Children[m_CurrentChild];
        Status childStatus = currentChild.CurrentStatus;

        if (childStatus == Status.Success || childStatus == Status.Failure)
        {
            m_CurrentChild++;
            return StartChild(m_CurrentChild);
        }

        return Status.Waiting;
    }

    protected Status StartChild(int childIndex)
    {
        if (childIndex >= Children.Count)
        {
            return Status.Success;
        }

        var childStatus = StartNode(Children[childIndex]);

        if (childStatus == Status.Success || childStatus == Status.Failure)
        {
            m_CurrentChild = childIndex + 1;
            return StartChild(m_CurrentChild);
        }

        return Status.Waiting;
    }
}
