using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metronome : MonoBehaviour {

    public BeatIndicator BeatIndicator { get; set; }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter2D(Collider2D other)
    {
        var beatLight = other.GetComponent<BeatLight>();

        if (beatLight != null)
        {
            beatLight.Light.color = Color.red;
            BeatIndicator.Play();
            BeatIndicator.IsHot = true;
            Debug.Log("ON");
            BeatIndicator.CurrentBeat = beatLight;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        BeatIndicator.IsHot = false;
        MainManager.Instance.Player.HasMoved = false;
        Debug.Log("OFF");
    }
}
