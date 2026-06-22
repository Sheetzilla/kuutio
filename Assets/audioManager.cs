using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class audioManager : MonoBehaviour
{
    public static audioManager instance;

    [SerializeField] private AudioSource SoundObject;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

        }
    }

    public void PlayAudio(AudioClip audioclip, Transform spawnTransform, float volume)
    {

        //spawn in gameobject

        AudioSource audioSource = Instantiate(SoundObject, spawnTransform.position, Quaternion.identity);

        // assign the audio
        audioSource.clip = audioclip;

        //asign volume
        audioSource.volume = volume;

        Debug.Log($"Playing {audioclip.name}, length={audioclip.length}");

        // play sound
        audioSource.Play();


        // get lenght of sound
        float clipLength = audioSource.clip.length;


        //destroy after sound is done
        Destroy(audioSource.gameObject, clipLength);
    }

}
