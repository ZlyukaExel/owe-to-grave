using System.Collections.Generic;
using UnityEngine;

public abstract class Trigger<T> : MonoBehaviour
    where T : Component
{
    public List<T> triggerList = new();

    public bool isTriggered() => triggerList.Count > 0;

    public virtual void Update()
    {
        if (isTriggered())
        {
            for (int i = 0; i < triggerList.Count; i++)
            {
                T component = triggerList[i];
                if (!component || !component.gameObject.activeInHierarchy)
                {
                    triggerList.Remove(component);
                    OnComponentRemoved(component);
                }
            }
        }
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        T[] components = other.GetComponents<T>();
        foreach (var component in components)
        {
            //if the object is not already in the list
            if (!triggerList.Contains(component))
                //add object to the list
                triggerList.Add(component);
        }
    }

    public virtual void OnTriggerExit(Collider other)
    {
        T[] components = other.GetComponents<T>();
        foreach (var component in components)
        {
            //if the object is in the list
            if (triggerList.Contains(component))
            {
                //remove it from the list
                triggerList.Remove(component);
            }
        }
    }

    public virtual void OnDisable()
    {
        ResetTrigger();
    }

    public void ResetTrigger()
    {
        triggerList.Clear();
    }

    public virtual void OnComponentRemoved(T component) { }
}
