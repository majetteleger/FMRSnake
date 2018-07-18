using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metronome : MonoBehaviour {

    public BeatIndicator BeatIndicator;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //if (BeatIndicator.CurrentBeat != null && !BeatIndicator.CurrentBeat.HasPlayed)
        //{
        //    if (Vector2.Distance(transform.position, BeatIndicator.CurrentBeat.transform.position) <= 3)
        //    {
        //        BeatIndicator.CurrentBeat.PlayBeat();
        //        BeatIndicator.CurrentBeat.HasPlayed = true;
        //    }
        //}
	}

    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    var passiveBeat = other.GetComponent<PassiveBeat>();

    //    if (passiveBeat != null)
    //    {
    //        BeatIndicator.CurrentBeat = passiveBeat;
    //    }
    //}
}
