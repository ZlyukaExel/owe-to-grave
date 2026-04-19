using UnityEngine;

public class TagTrigger : Trigger<NameTag>
{
    public override void OnTriggerEnter(Collider other)
    {
        NameTag[] components = other.transform.root.GetComponents<NameTag>();
        foreach (var component in components)
        {
            if (component.nameTagObject == null)
                continue;

            // If the object is not already in the list
            if (!triggerList.Contains(component))
                // Add object to the list
                triggerList.Add(component);

            component.nameTagObject.SetActive(true);
        }
    }

    public override void OnTriggerExit(Collider other)
    {
        NameTag[] components = other.transform.root.GetComponents<NameTag>();
        foreach (var component in components)
        {
            if (component.nameTagObject == null)
                continue;

            // If the object is in the list
            if (triggerList.Contains(component))
            {
                // Remove it from the list
                triggerList.Remove(component);
            }

            component.nameTagObject.SetActive(false);
        }
    }

    public override void OnComponentRemoved(NameTag component)
    {
        if (!component || !component.nameTagObject)
            return;
        component.nameTagObject.SetActive(false);
    }
}
