using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneStartScript : MonoBehaviour
{
    public Animator mAnimator;
    public GameObject mLoadingScreen;
    public GameObject mScene;
    public GameObject mUI;

    void Start()
    {
        StartCoroutine(mSceneStarted());    
    }

    IEnumerator mSceneStarted()
    {
        mAnimator.Play("Closing");
        yield return new WaitForSeconds(1);
        
        //mLoadingScreen.SetActive(false);
    }
}
