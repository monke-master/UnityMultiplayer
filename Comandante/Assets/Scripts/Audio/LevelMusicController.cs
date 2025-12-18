
using UnityEngine;

public class LevelMusicController : MonoBehaviour
{
    private AudioSource _audioSource;
    [SerializeField] private AudioClip[] musicClips;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.volume = AudioController._musicVolume;
    }
    
    void Update()
    {
        if (!_audioSource.isPlaying)
        {
            _audioSource.PlayOneShot(musicClips[UnityEngine.Random.Range(0, musicClips.Length)]);
        }
    }

    
}
