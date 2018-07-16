using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class BeatIndicator : MonoBehaviour {

    public Bar Bar;
    public Metronome Metronome;
    public GameObject PassiveBeatPrefab;
    public GameObject BeatPrefab;
    public Transform BeatsParent;
    public float BeatSpeed;
    public GameObject BeatLightPrefab;

    public UIBeat CurrentBeat { get; set; }
    public bool IsHot { get; set; }

    private List<PassiveBeat> _passiveBeats;
    private List<ActiveBeat> _beatLights;
    private List<AudioSource> _beatSources;

    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    void Update() {

	}

    public void StartMetronome()
    {
        var y = Metronome.GetComponent<RectTransform>().sizeDelta.y;
        Metronome.GetComponent<CircleCollider2D>().radius = y/2f;

        Metronome.GetComponent<Metronome>().BeatIndicator = this;

        ResetMetronome();
    }

    private void MoveMetronome()
    {
        Metronome.transform.DOMoveX(Camera.main.ViewportToScreenPoint(new Vector3(1, 0.9f, 10)).x, BeatSpeed).SetEase(Ease.Linear).OnComplete(ResetMetronome);
    }

    private void ResetMetronome()
    {
        var metronomePos = Metronome.GetComponent<RectTransform>().position;
        metronomePos.x = 0;
        Metronome.GetComponent<RectTransform>().position = metronomePos;

        for (int i = 0; i < _beatLights.Count; i++)
        {
            _beatLights[i].ResetColor();
        }

        MoveMetronome();
    }

    public void CreatePassiveBeats()
    {
        for (int i = 0; i < 9; i++)
        {
            if (i == 0)
            {
                continue;
            }

            var passiveBeat = Instantiate(PassiveBeatPrefab, transform).GetComponent<PassiveBeat>(); 
            _passiveBeats.Add(passiveBeat);
            var yPos = 0;
            var xPos = ((Screen.width / 9) * i) - Screen.width/2;

            passiveBeat.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, yPos);
        }
    }

    public void UpdateIndicator(Bar newBar)
    {
        Bar = newBar;

        if (_beatLights.Count > 0)
        {
            for (int i = 0; i < _beatLights.Count; i++)
            {
                Destroy(_beatLights[i].gameObject);
            }
        }

        CreateIndicator();
        Metronome.transform.DOKill();
        ResetMetronome();
    }

    public void CreateIndicator(bool firstTime = false)
    {
        var halfScreen = MainPanel.Instance.GetComponent<RectTransform>().rect.width;
        var numberOfBeats = Bar.Beats.Count;
        float totalTimeOfBar = 0;
        for (int i = 0; i < Bar.Beats.Count; i++)
        {
            totalTimeOfBar += Bar.Beats[i].Delay;
        }

        BeatSpeed = totalTimeOfBar * 2f;

        _beatLights = new List<ActiveBeat>();

        for (int i = 0; i < Bar.Beats.Count; i++)
        {
            var beatLight = Instantiate(BeatLightPrefab, BeatsParent).GetComponent<ActiveBeat>();
            
            var xPos = (Bar.Beats[i].Delay * i / totalTimeOfBar * (Screen.width/2f));

            if(firstTime)
            {
                var yPos = beatLight.GetComponent<RectTransform>().anchoredPosition.y;
                beatLight.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, yPos);
            }
            else
            {
                beatLight.GetComponent<RectTransform>().DOAnchorPosX(xPos, .3f);
            }
            

            //beatLight.Light.color = STMYellow;

            _beatLights.Add(beatLight);
        }
    }
}