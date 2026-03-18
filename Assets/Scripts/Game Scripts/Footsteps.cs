using UnityEngine;

public class Footsteps : MonoBehaviour
{
    private NetworkAudioSource audioSource;

    [HideInInspector]
    public float speed = 0;
    private float timer = 0,
        stepInterval = 0;
    private PlayerLinks l;

    void Start()
    {
        audioSource = GetComponent<NetworkAudioSource>();
        l = GetComponentInParent<PlayerLinks>();
    }

    void Update()
    {
        if (speed == 0 || !l.movement.isGrounded)
        {
            timer = 0;
            return;
        }

        timer += Time.deltaTime;

        if (speed > 1)
            stepInterval = 0.35f;
        else if (speed > 0.5f)
            stepInterval = 0.5f;
        else
            stepInterval = 1;

        if (timer >= stepInterval)
        {
            //audioSource.CmdPlay();
            timer = 0f;
        }
    }
}
