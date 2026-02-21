using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundsPlaySceript : MonoBehaviour
{
    public AudioClip[] sounds;
    private AudioSource audioSrc => GetComponent<AudioSource>();

    //To use need to be child of this script
    public void PlaySound(AudioClip clip, float volume, bool destroyed)
    {
        if (destroyed)
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
        else
            audioSrc.PlayOneShot(clip, volume);
    }
}
