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
        if (BeatIndicator.CurrentBeat != null && !BeatIndicator.CurrentBeat.HasPlayed)
        {
            if (Vector2.Distance(GetComponent<RectTransform>().anchoredPosition, BeatIndicator.CurrentBeat.GetComponent<RectTransform>().anchoredPosition) <= 5)
            {
                BeatIndicator.CurrentBeat.PlayBeat();
                BeatIndicator.CurrentBeat.HasPlayed = true;
            }
        }
	}

    private void OnTriggerEnter2D(Collider2D other)
    {
        var activeBeat = other.GetComponent<ActiveBeat>();
        var passiveBeat = other.GetComponent<PassiveBeat>();

        if (activeBeat != null)
        {
            activeBeat.Light.color = Color.red;
            BeatIndicator.IsHot = true;
            Debug.Log("ON");
            BeatIndicator.CurrentBeat = activeBeat;
        }
        else if (passiveBeat != null)
        {
            BeatIndicator.CurrentBeat = passiveBeat;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var activeBeat = other.GetComponent<ActiveBeat>();

        if (activeBeat != null)
        {
            BeatIndicator.IsHot = false;
            MainManager.Instance.Player.HasMoved = false;
            Debug.Log("OFF");
        }

    }
}
