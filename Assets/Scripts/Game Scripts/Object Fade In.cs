using System.Collections;
using UnityEngine;

public class ObjectFadeIn : MonoBehaviour
{
    public Animation fade;
    public bool playAnimOnStart = false;
    public float waitBeforeAnimation = 0;

    private void Start()
    {
        if (playAnimOnStart)
            StartCoroutine(bhFade());
    }

    public void PlayFadeAnim(float wfs)
    {
        waitBeforeAnimation = wfs;
        StartCoroutine(bhFade());
    }

    IEnumerator bhFade()
    {
        yield return new WaitForSeconds(waitBeforeAnimation);
        fade.Play();
    }
}
