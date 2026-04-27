using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class Monologue : MonoBehaviour
{
 [SerializeField] private InputActionReference interactAction;
    public enum AnswerAction { None, ShowAnswers, EndConversation }

    [TextArea(3, 5)] 
    [SerializeField] private string[] sentences;
    [SerializeField] private float[] delays;
    [SerializeField] private string[] animationTriggers; 
    [SerializeField] private AudioClip[] audios; 

    [Header("What to do at the end")]
    [SerializeField] private AnswerAction answerOption;

    private Animator animator;
    private AudioSource audioSource;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (delays.Length -1 != sentences.Length)
            Debug.Log("Delays should be equal to sentenses + 1");
    }

    public void StartConversation()
    {
        StartCoroutine(PlayMonologue());
    }

    private IEnumerator PlayMonologue()
    {
        yield return new WaitForSeconds(delays[0]);
        for (int i = 0; i < sentences.Length; i++)
        {
            Debug.Log($"[UI Текст]: {sentences[i]}");

            if (i < animationTriggers.Length && !string.IsNullOrEmpty(animationTriggers[i]))
            {
                animator.SetTrigger(animationTriggers[i]);
            }

            if (audioSource != null && i < audios.Length && audios[i] != null)
            {
                audioSource.clip = audios[i];
                audioSource.Play();
            }

            float currentDelay = (i < delays.Length) ? delays[i + 1] : 2f;
            yield return new WaitForSeconds(currentDelay);
        }

        HandleEnding();
    }

    private void HandleEnding()
    {
        switch (answerOption)
        {
            case AnswerAction.ShowAnswers:
                Debug.Log("[UI]: Показываем кнопки");
                break;
            case AnswerAction.EndConversation:
                Debug.Log("Монолог окончен.");
                break;
            default:
                break;
        }
    }


}