using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOneShot : MonoBehaviour
{
    public AudioClip sound;
    public AudioSource mAudioSource;

    public void Play()
    {
        mAudioSource.PlayOneShot(sound);
    }
}
