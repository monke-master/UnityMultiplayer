using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuMusicController : MonoBehaviour
{
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        AudioController.onChanged += () =>
        {
            _audioSource.volume = AudioController._musicVolume;
        };
    }
}
