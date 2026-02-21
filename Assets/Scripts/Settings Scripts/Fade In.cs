using System.Collections;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    private Animator animator, appearingAnimator;
    public GameObject appearingButtons;

    void Start()
    {
        animator = GetComponent<Animator>();
        appearingAnimator = appearingButtons.GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            StartCoroutine(mFade());
    }

    public void mFadeButtons()
    {
        StartCoroutine(mFade());
    }

    private IEnumerator mFade()
    {
        animator.Play("Fading");
        yield return new WaitForSeconds(0.6f);
        gameObject.SetActive(false);

        appearingButtons.SetActive(true);
        appearingAnimator.Play("FadeOut");

        yield return null;
    }
}
