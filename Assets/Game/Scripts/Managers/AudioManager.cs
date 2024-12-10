using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioSource _soundTrackSource;
    [SerializeField] private AudioSource _soundEffectSource;

    [SerializeField] private AudioMixer _soundTrackMixer;
    [SerializeField] private AudioMixer _soundEffectMixer;

    [SerializeField] private SoundTrackDictionary _soundTracks;
    [SerializeField] private SoundEffectDictionary _soundEffects;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySoundTrack(SoundTrackType soundTrackType)
    {
        if (_soundTracks.TryGetValue(soundTrackType, out var clip))
        {
            _soundTrackSource.loop = true;
            _soundTrackSource.clip = clip;
            _soundTrackSource.Play();
        }
        else
        {
            _soundTrackSource.Stop();
        }
    }

    public void PlaySoundEffect(SoundEffectType soundEffectType, float pitch = 1)
    {
        if (_soundEffects.TryGetValue(soundEffectType, out var clip))
        {
            _soundEffectSource.pitch = pitch;
            _soundEffectSource.PlayOneShot(clip);
        }
        else
        {
            _soundEffectSource.Stop();
        }
    }
}

[Serializable]
public class SoundTrackDictionary : SerializableDictionary<SoundTrackType, AudioClip, SoundTrackStorage>
{
}

[Serializable]
public class SoundTrackStorage : SerializableDictionary.Storage<AudioClip>
{
}

[Serializable]
public class SoundEffectDictionary : SerializableDictionary<SoundEffectType, AudioClip, SoundEffectStorage>
{
}

[Serializable]
public class SoundEffectStorage : SerializableDictionary.Storage<AudioClip>
{
}

public enum SoundTrackType
{
    MainMenu = 0,
    Gameplay = 1,
    Lose = 2,
}

public enum SoundEffectType
{
    None = 0,
    Click = 1,
    PickAndDrop = 2,
    SendButton = 4,
    QuizPopup = 5,
}