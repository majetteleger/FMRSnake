using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBeat : MonoBehaviour {

    public bool HasPlayed;

    private AudioSource _audioSource;

	// Use this for initialization
	void Start () {
        _audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PlayBeat()
    {
        _audioSource.Play();
    }
}
