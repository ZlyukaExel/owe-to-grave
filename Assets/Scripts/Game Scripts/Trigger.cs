using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    //The list of colliders inside the trigger
    public List<Collider> triggerList = new();

    public bool isTriggered() => triggerList.Count > 0;

    private void Update()
    {
        if (isTriggered())
        {
            for (int i = 0; i < triggerList.Count; i++)
                if (triggerList[i] == null || !triggerList[i].gameObject.activeInHierarchy)
                    triggerList.RemoveAt(i);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //if the object is not already in the list
        if (!triggerList.Contains(other))
            //add object to the list
            triggerList.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        //if the object is in the list
        if (triggerList.Contains(other))
        {
            //remove it from the list
            triggerList.Remove(other);
        }
    }

    void OnDisable()
    {
        ResetTrigger();
    }

    public void ResetTrigger()
    {
        triggerList.Clear();
    }
}
