using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveBeat : MonoBehaviour {

    public Image Image;
    public Color NormalColor;
    public Color HighlightedColor;
    public Color SuccessColor;
    public Color FailureColor;

    public bool Activated { get; set; }
    public bool HasPlayed { get; set; }

    public void ResetBeat()
    {
        Image.color = NormalColor;
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
                    //MainManager.Instance.Player.PulseHeadSegment();
                    //GridPlayground.Instance.PulseObstacles();
                    //GridPlayground.Instance.PulseFoods();
                    //AudioManager.Instance.PlayActiveBeat();
                    //HasPlayed = true;
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
                Image.color = HighlightedColor;
            }
            else
            {
                Debug.Log("Beats overlapping");
            }

            if (!HasPlayed)
            {
            MainManager.Instance.Player.PulseHeadSegment();
            GridPlayground.Instance.PulseObstacles();
            GridPlayground.Instance.PulseFoods();
            AudioManager.Instance.PlayActiveBeat();
            HasPlayed = true;
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
                Image.color = FailureColor;
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