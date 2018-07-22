using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveBeat : UIBeat {

    public Image Light;
    public bool Activated { get; set; }

    public void ResetBeat()
    {
        Light.color = Color.black;
        Activated = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        var metronome = other.GetComponent<Metronome>();

        if (metronome != null)
        {
            if (!HasPlayed)
            {
                if (Vector2.Distance(transform.position, metronome.transform.position) <= 3)
                {
                    PlayBeat();
                    HasPlayed = true;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var metronome = other.GetComponent<Metronome>();

        if (metronome != null)
        {
            if (metronome.BeatIndicator.CurrentActiveBeat == null)
            {
                metronome.BeatIndicator.CurrentActiveBeat = this;
            }
            else
            {
                Debug.Log("Beats overlapping");
            }
            metronome.BeatIndicator.IsHot = true;
            //Debug.Log("ON");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var metronome = other.GetComponent<Metronome>();

        if (metronome != null)
        {
            if (!Activated)
            {
                Light.color = Color.red;
                MainManager.Instance.Player.FailBeat();
            }

            if (metronome.BeatIndicator.CurrentActiveBeat == this)
            {
                metronome.BeatIndicator.CurrentActiveBeat = null;
            }
            else
            {
                Debug.Log("Beats overlapping");
            }

            metronome.BeatIndicator.IsHot = false;
            MainManager.Instance.Player.HasMoved = false;
            //Debug.Log("OFF");
        }
    }
}