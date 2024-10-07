using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSFX = null;
    [SerializeField] private AudioSource _audioBGM = null;

    [Header("BGM Clip"), SerializeField] private AudioClip _clipBgm = null;
    [Header("SFX Clip"), SerializeField] private List<DataManager.SoundTemplate> _sfxTemplate = null;

    public void Initialize()
    {

    }

    public void PlayBgm()
    {
        _audioBGM.clip = _clipBgm;
        _audioBGM.playOnAwake = true;
        _audioBGM.loop = true;

        _audioBGM.Play();
    }

    public void PlaySfx(eSound type)
    {
        var a = _sfxTemplate.Find(x => x.type == type);
    }

    public void MuteSfx(Action<bool> onCallback)
    {
        _audioSFX.mute = _audioSFX.mute == true ? false : true;

        onCallback?.Invoke(_audioSFX.mute);
    }

    public void MuteBgm(Action<bool> onCallback)
    {
        _audioBGM.mute = _audioBGM.mute == true ? false : true;

        onCallback?.Invoke(_audioBGM.mute);
    }
}
