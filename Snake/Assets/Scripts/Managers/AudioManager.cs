using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AudioManager : MonoBehaviour {

    public static AudioManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    [Header("Ambiance")]
    public float FadeTime;
    public AudioClip AmbianceMenu;
    public AudioSource FadeFrom;
    public AudioSource FadeTo;

    [Header("SFX")]
    public AudioClip GameStart;
    public AudioClip GameEnd;
    public AudioClip GetFood;
    public AudioClip MenuInteraction;
    public AudioSource PassiveBeat;
    public AudioSource ActiveBeat;
    public AudioSource BarReturnBeat;
    public AudioSource[] OtherSFXs;

    // Use this for initialization
    void Start () {
        FadeFrom.volume = 0;
        FadeTo.volume = 0;
        FadeAmbianceTo(AmbianceMenu);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void FadeAmbianceTo(AudioClip clip)
    {
        if (!FadeFrom.isPlaying)
        {
            FadeFrom.Play();
        }

        if (FadeFrom.clip != clip)
        {
            FadeFrom.DOFade(0, FadeTime);

            FadeTo.clip = clip;
            if (!FadeTo.isPlaying)
            {
                FadeTo.Play();
            }
            FadeTo.DOFade(1, FadeTime);

            AudioSource temp = FadeFrom;
            FadeFrom = FadeTo;
            FadeTo = temp;
        }
    }

    public void PlayPassiveBeat()
    {
        PassiveBeat.Play();
    }

    public void PlayBarReturn()
    {
        BarReturnBeat.Play();
    }

    public void PlayActiveBeat()
    {
        ActiveBeat.Play();
    }

    public void PlayOtherSFX(AudioClip clip)
    {
        for (int i = 0; i < OtherSFXs.Length; i++)
        {
            if (!OtherSFXs[i].isPlaying)
            {
                OtherSFXs[i].clip = clip;
                OtherSFXs[i].Play();
                return;
            }
        }
    }
}