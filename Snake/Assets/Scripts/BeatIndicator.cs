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
    public GameObject ActiveBeatPrefab;
    public float Tempo;
    public GameObject BeatLightPrefab;

    public ActiveBeat CurrentActiveBeat { get; set; }
    public bool IsHot { get; set; }

    private Vector2 _metronomeStartPos;
    private float _halfDistance;
    private List<PassiveBeat> _passiveBeats;
    private List<ActiveBeat> _activeBeats;
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
        Metronome.GetComponent<RectTransform>().DOAnchorPosX(_passiveBeats[_passiveBeats.Count - 1].GetComponent<RectTransform>().anchoredPosition.x + _halfDistance, Tempo / 60 * 2).SetEase(Ease.Linear).OnComplete(ResetMetronome);
    }

    private void ResetMetronome()
    {
        //var metronomePos = Metronome.GetComponent<RectTransform>().anchoredPosition;
        //metronomePos.x = 0f;
        Metronome.GetComponent<RectTransform>().anchoredPosition = _metronomeStartPos;

        for (int i = 0; i < _passiveBeats.Count; i++)
        {
            _passiveBeats[i].HasPlayed = false;
        }

        for (int i = 0; i < _activeBeats.Count; i++)
        {
            _activeBeats[i].ResetColor();
            _activeBeats[i].HasPlayed = false;
        }

        MoveMetronome();
    }

    public void CreatePassiveBeats()
    {
        _passiveBeats = new List<PassiveBeat>();

        for (int i = 0; i < 9; i++)
        {

            var yPos = 0;
            var xPos = ((Screen.width / 9) * i) - Screen.width / 2;

            if (i == 0)
            {
                continue;
            }

            var passiveBeat = Instantiate(PassiveBeatPrefab, transform).GetComponent<PassiveBeat>();
            _passiveBeats.Add(passiveBeat);
            passiveBeat.name = "PassiveBeat " + (i - 1).ToString();

            passiveBeat.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, yPos);
        }

        SetMetronomeStartingPos();
    }

    private void SetMetronomeStartingPos()
    {
        var yPos = 0;
        var xPos = - Screen.width / 2;
        _halfDistance = (Vector2.Distance(_passiveBeats[4].GetComponent<RectTransform>().anchoredPosition, _passiveBeats[5].GetComponent<RectTransform>().anchoredPosition)) / 2;
        _metronomeStartPos = new Vector2(xPos + _halfDistance, yPos);
    }

    public void StopBeat()
    {
        Metronome.transform.DOKill();
    }

    public void UpdateBar(Bar newBar)
    {
        Bar = newBar;

        if (_activeBeats.Count > 0)
        {
            for (int i = 0; i < _activeBeats.Count; i++)
            {
                Destroy(_activeBeats[i].gameObject);
            }
        }

        CreateActiveBeats();
        Metronome.transform.DOKill();
        ResetMetronome();
    }

    public void CreateActiveBeats()
    {
        _activeBeats = new List<ActiveBeat>();

        for (int i = 0; i < Bar.Beats.Length; i++)
        {
            if (Bar.Beats[i])
            {
                if (i % 2 == 0) // this is a BEAT
                {
                    var activeBeat = Instantiate(ActiveBeatPrefab, transform).GetComponent<ActiveBeat>();
                    var yPos = 0;
                    var xPos = _passiveBeats[i/2 + 4].GetComponent<RectTransform>().anchoredPosition.x;
                    activeBeat.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, yPos);
                    _activeBeats.Add(activeBeat);
                }
                else // this is an OFFBEAT
                { 
                    var activeBeat = Instantiate(ActiveBeatPrefab, transform).GetComponent<ActiveBeat>();
                    var yPos = 0;
                    var xPos = _passiveBeats[Mathf.FloorToInt(i / 2) + 4].GetComponent<RectTransform>().anchoredPosition.x + _halfDistance;
                    activeBeat.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, yPos);
                    _activeBeats.Add(activeBeat);
                }
            }
        }

        //var halfScreen = MainPanel.Instance.GetComponent<RectTransform>().rect.width;
        //var numberOfBeats = Bar.Beats.Count;
        //float totalTimeOfBar = 0;
        //for (int i = 0; i < Bar.Beats.Count; i++)
        //{
        //    totalTimeOfBar += Bar.Beats[i].Delay;
        //}

        //_beatLights = new List<ActiveBeat>();

        //for (int i = 0; i < Bar.Beats.Count; i++)
        //{
        //    var beatLight = Instantiate(BeatLightPrefab, BeatsParent).GetComponent<ActiveBeat>();
            
        //    var xPos = (Bar.Beats[i].Delay * i / totalTimeOfBar * (Screen.width/2f));

        //    if(firstTime)
        //    {
        //        var yPos = beatLight.GetComponent<RectTransform>().anchoredPosition.y;
        //        beatLight.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, yPos);
        //    }
        //    else
        //    {
        //        beatLight.GetComponent<RectTransform>().DOAnchorPosX(xPos, .3f);
        //    }
            

        //    //beatLight.Light.color = STMYellow;

        //    _beatLights.Add(beatLight);
        //}
    }
}