using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class OneFrameDelay : MonoBehaviour
{
    [SerializeField]
    private UnityEvent action;

    public void Invoke()
    {
        StartCoroutine(Cor());
    }

    private IEnumerator Cor()
    {
        yield return null;
        action.Invoke();
    }
}
