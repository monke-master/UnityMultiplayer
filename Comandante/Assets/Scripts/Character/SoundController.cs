using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shotClip;
    [SerializeField] private AudioClip rechargeClip;


    private void Awake()
    {
        audioSource.volume = AudioController._soundsVolume;
    }

    public void PlayShotAudio()
    {
        audioSource.PlayOneShot(shotClip);
    }
    
    public void PlayRechargeAudio()
    {
        Debug.Log("PlayRechargeAudio");
        audioSource.PlayOneShot(rechargeClip);
    }

}
