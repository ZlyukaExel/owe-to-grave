using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class NewBehaviourScript : MonoBehaviour
{
    public UnityEvent action;
    public void Start()
    {
        //print("Event triggered");
        StartCoroutine(Delay(10));
    }

    IEnumerator Delay(float delay)
    {
        yield return new WaitForSeconds(delay);
        action.Invoke();
    }
}
