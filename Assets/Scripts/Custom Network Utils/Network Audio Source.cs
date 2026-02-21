using Mirror;
using UnityEngine;

public class NetworkAudioSource : NetworkBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Play()
    {
        if (isOwned)
            CmdPlay();
    }

    [Command]
    private void CmdPlay()
    {
        RpcPlay();
    }

    [ClientRpc]
    private void RpcPlay()
    {
        audioSource.Play();
    }
}
