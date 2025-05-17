using System;
using UnityEngine;

[Serializable]
public class Sound
{
    public string Name;
    public AudioClip Clip;
}

public class AudioManager : SingletonPersistent<AudioManager>
{
    [SerializeField] private Sound[] _musics, _sfx, _side;
    [SerializeField] private AudioSource _musicSource, _sfxSource, _sideSource;

    public void Initialize()
    {
        LoadAudioPrefs();
        PlayMusic("Menu");
    }

    public void PlaySFX(string name)
    {
        var s = Array.Find(_sfx, s => s.Name == name);
        if (s != null)
        {
            _sfxSource.PlayOneShot(s.Clip);
        }
    }

    public void PlaySideAudio(string name)
    {
        var s = Array.Find(_side, s => s.Name == name);
        if (s != null)
        {
            _sideSource.clip = s.Clip;
            _sideSource.Play();
        }
    }

    public void StopSideAudio()
    {
        _sideSource.Stop();
    }

    public void PlayMusic(string name)
    {
        var s = Array.Find(_musics, s => s.Name == name);
        if (s != null)
        {
            _musicSource.clip = s.Clip;
            _musicSource.Play();
        }
    }

    public void StopMusic()
    {
        _musicSource.Stop();
    }

    public void ToggleSFX()
    {
        _sfxSource.mute = !_sfxSource.mute;
        _sideSource.mute = !_sideSource.mute;
        PlayerPrefs.SetInt(GameConstants.PLAYER_PREFS_SFX_TOGGLE, _sfxSource.mute ? 0 : 1);
        PlayerPrefs.Save();
    }

    public void ToggleMusic()
    {
        _musicSource.mute = !_musicSource.mute;
        PlayerPrefs.SetInt(GameConstants.PLAYER_PREFS_MUSIC_TOGGLE, _musicSource.mute ? 0 : 1);
        PlayerPrefs.Save();
    }

    private void LoadAudioPrefs()
    {
        _sfxSource.mute = PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_SFX_TOGGLE, 1) != 1;
        _sideSource.mute = PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_SFX_TOGGLE, 1) != 1;
        _musicSource.mute = PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_MUSIC_TOGGLE, 1) != 1;
    }
}
