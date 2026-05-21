using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DelayedAction : MonoBehaviour
{
    public float waitForSeconds = 0;

    [SerializeField]
    private UnityEvent action;

    public void Invoke()
    {
        StartCoroutine(Cor());
    }

    private IEnumerator Cor()
    {
        yield return new WaitForSecondsRealtime(waitForSeconds);
        action.Invoke();
    }
}
