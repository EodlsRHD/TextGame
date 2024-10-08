using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource _audioBGM = null;
    [SerializeField] private AudioSource _audioSFX = null;

    [SerializeField] private AudioSource _audioButton = null;
    [SerializeField] private AudioSource _audioBook = null;
    [SerializeField] private AudioSource _audioAttack = null;
    [SerializeField] private AudioSource _audioHit = null;
    [SerializeField] private AudioSource _audioDefence = null;
    [SerializeField] private AudioSource _audioOther = null;

    [Header("Lobby BGM Clip"), SerializeField] private List<AudioClip> _clipLobbyBgm = null;
    [Header("InGame BGM Clip"), SerializeField] private List<AudioClip> _clipInGameBgm = null;
    [Header("Battle BGM Clip"), SerializeField] private List<AudioClip> _clipBattleBgm = null;
    [Header("Shop BGM Clip"), SerializeField] private List<AudioClip> _clipShopBgm = null;

    [Header("SFX Clip"), SerializeField] private List<DataManager.SoundTemplate> _sfxTemplate = null;

    public void Initialize()
    {
        _audioBGM.volume = 0;
    }

    public void PlayBgm(eBgm type)
    {
        switch(type)
        {
            case eBgm.Lobby:
                VolumeUp(_clipLobbyBgm[Random(_clipLobbyBgm.Count)]);
                break;

            case eBgm.Ingame:
                VolumeUp(_clipInGameBgm[Random(_clipInGameBgm.Count)]);
                break;

            case eBgm.Battle:
                VolumeUp(_clipBattleBgm[Random(_clipBattleBgm.Count)]);
                break;

            case eBgm.Shop:
                VolumeUp(_clipShopBgm[Random(_clipShopBgm.Count)]);
                break;
        }

        _audioBGM.loop = true;
        _audioBGM.Play();
    }

    public void PlaySfx(eSfx type)
    {
        DataManager.SoundTemplate template = _sfxTemplate.Find(x => x.type == type);

        if (type == eSfx.ButtonPress)
        {
            Play(_audioButton, template);

            return;
        }

        if (type == eSfx.Attack)
        {
            Play(_audioAttack, template);

            return;
        }

        if (type == eSfx.Hit_light || type == eSfx.Hit_hard)
        {
            Play(_audioHit, template);

            return;
        }

        if (type == eSfx.Blocked)
        {
            Play(_audioDefence, template);

            return;
        }

        if (type == eSfx.SceneChange)
        {
            Play(_audioBook, template);

            return;
        }

        Play(_audioOther, template);
    }

    public void Play(AudioSource souece, DataManager.SoundTemplate template)
    {
        if (template.clip != null)
        {
            souece.clip = template.clip;
            souece.Play();

            return;
        }

        if(template.clips != null)
        {
            souece.clip = template.clips[Random(template.clips.Count)];
            souece.Play();
        }
    }

    public void MuteSfx(Action<bool> onCallback)
    {
        _audioSFX.mute = _audioSFX.mute == true ? false : true;
        _audioButton.mute = _audioButton.mute == true ? false : true;
        PlayerPrefs.SetInt("SFX", _audioSFX.mute == true ? 0 : 1);

        onCallback?.Invoke(_audioSFX.mute);
    }

    public void MuteBgm(Action<bool> onCallback)
    {
        _audioBGM.mute = _audioBGM.mute == true ? false : true;
        PlayerPrefs.SetInt("BGM", _audioBGM.mute == true ? 0 : 1);

        onCallback?.Invoke(_audioBGM.mute);
    }

    public void MuteSfx(bool isMute)
    {
        _audioSFX.mute = isMute;
    }

    public void MuteBgm(bool isMute)
    {
        _audioBGM.mute = isMute;
    }

    private void VolumeUp(AudioClip newBgm)
    {
        _audioBGM.volume = 0;
        _audioBGM.clip = newBgm;

        StartCoroutine(Co_VolumeUp());
    }

    IEnumerator Co_VolumeUp()
    {
        while(true)
        {
            yield return null;

            if (_audioBGM.volume >= 0.8f)
            {
                _audioBGM.volume = 0.8f;

                break;
            }

            _audioBGM.volume += 0.01f;
        }

        yield return null;
    }

    public void VolumeDown(Action onResultCallback)
    {
        if(_audioBGM.volume == 0)
        {
            onResultCallback?.Invoke();

            return;
        }

        StartCoroutine(Co_VolumeDown(onResultCallback));
    }

    IEnumerator Co_VolumeDown(Action onResultCallback)
    {
        while (true)
        {
            yield return null;

            if (_audioBGM.volume == 0)
            {
                break;
            }

            _audioBGM.volume -= 0.05f;
        }

        onResultCallback?.Invoke();

        yield return null;
    }

    private int Random(int maximum)
    {
        return UnityEngine.Random.Range(0, maximum);
    }
}
