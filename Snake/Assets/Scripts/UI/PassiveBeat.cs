using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveBeat : MonoBehaviour {

    public bool HasPlayed { get; set; }

    private void OnTriggerStay2D(Collider2D other)
    {
        var metronome = other.GetComponent<Metronome>();

        if (metronome != null)
        {
            if (!HasPlayed)
            {
                if (Vector2.Distance(transform.position, metronome.transform.position) <= 3)
                {
                    if (MainPanel.Instance.BeatIndicator.UpdateBarAtNextBeat)
                    {
                        MainPanel.Instance.BeatIndicator.UpdateBar(MainManager.Instance.Player.CurrentCell.ZoneModifier.Bar);
                        return;
                    }

                    if (this == MainPanel.Instance.BeatIndicator.PassiveBeats[MainPanel.Instance.BeatIndicator.PassiveBeats.Count - 1])
                    {
                        AudioManager.Instance.PlayBarReturn();
                    }

                    AudioManager.Instance.PlayPassiveBeat();

                    HasPlayed = true;
                }
            }
        }
    }
}