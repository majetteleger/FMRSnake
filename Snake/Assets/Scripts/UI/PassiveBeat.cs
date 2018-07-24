﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveBeat : UIBeat {
    
    private void OnTriggerStay2D(Collider2D other)
    {
        var metronome = other.GetComponent<Metronome>();

        if (metronome != null)
        {
            if (!HasPlayed)
            {
                if (Vector2.Distance(transform.position, metronome.transform.position) <= 3)
                {
                    if (metronome.BeatIndicator.UpdateBarAtNextBeat)
                    {
                        MainPanel.Instance.BeatIndicator.UpdateBar(MainManager.Instance.Player.CurrentCell.ZoneModifier.Bar);
                    }

                    PlayBeat();
                    HasPlayed = true;
                }
            }
        }
    }
}