using Mirror;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
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

    [Command(requiresAuthority = false)]
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
