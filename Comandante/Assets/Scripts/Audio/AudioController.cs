using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour
{
    public static float _soundsVolume = 0.5f;
    public static float _musicVolume = 0.5f;

    public static Action onChanged;


    public static void SetSoundsVolume(float vol)
    {
        _soundsVolume = vol;
        onChanged?.Invoke();
    }
    
    public static void SetMusicVolume(float vol)
    {
        _musicVolume = vol;
        onChanged?.Invoke();
    }


}
