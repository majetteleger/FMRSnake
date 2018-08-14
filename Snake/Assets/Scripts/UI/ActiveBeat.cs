using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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
        //var metronome = other.GetComponent<Metronome>();

        //if (metronome != null)
        //{
        //    if (!HasPlayed)
        //    {
        //        if (Vector2.Distance(transform.position, metronome.transform.position) <= 3)
        //        {
        //            MainManager.Instance.Player.PulseHeadSegment();
        //            GridPlayground.Instance.PulseObstacles();
        //            GridPlayground.Instance.PulseFoods();
        //            AudioManager.Instance.PlayActiveBeat();
        //            HasPlayed = true;
        //        }
        //    }
        //}
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var metronome = other.GetComponent<Metronome>();

        if (metronome != null)
        {
            if (MainPanel.Instance.BeatIndicator.CurrentActiveBeat == null)
            {
                MainPanel.Instance.BeatIndicator.CurrentActiveBeat = this;
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

            MainPanel.Instance.BeatIndicator.IsHot = true;
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

            if (MainPanel.Instance.BeatIndicator.CurrentActiveBeat == this)
            {
                MainPanel.Instance.BeatIndicator.CurrentActiveBeat = null;
            }
            else
            {
                Debug.Log("Beats overlapping");
            }

            MainPanel.Instance.BeatIndicator.IsHot = false;
            MainManager.Instance.Player.HasMoved = false;
            GetComponent<RectTransform>().DOScale(Vector3.zero, 0.2f);
            //Debug.Log("OFF");
        }
    }
}