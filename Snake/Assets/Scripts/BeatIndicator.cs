using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class BeatIndicator : MonoBehaviour {

    public Bar Bar;
    public Metronome Metronome;
    public GameObject BeatPrefab;
    public Transform BeatsParent;
    public float BeatSpeed;
    public AudioClip LowBeat;
    public AudioClip HighBeat;
    public GameObject BeatLightPrefab;
    public Color STMYellow;

    public Color STMBlue;
    public BeatLight CurrentBeat { get; set; }
    public float BeatWindowPadding;
    public bool IsHot { get; set; }
    
    private float beatTimer;
    private List<BeatLight> _beatLights;
    private List<AudioSource> _beatSources;
    private float wtv;

    // Use this for initialization
    void Start () {
        beatTimer = Bar.Beats[0].Delay;

        _beatSources = new List<AudioSource>();
        _beatSources.Add(gameObject.AddComponent<AudioSource>());
        _beatSources[0].clip = LowBeat;
        _beatSources.Add(gameObject.AddComponent<AudioSource>());
        _beatSources[1].clip = HighBeat;
    }

    // Update is called once per frame
    void Update()
    {
        //if (beatTimer <= (Bar.Beats[beatIndex].Delay / 2))
        //{
        //    IsHot = true;
        //}
        //else if (beatTimer <= Bar.Beats[beatIndex].Delay - (Bar.Beats[beatIndex].Delay / 2))
        //{
        //    IsHot = false;
        //    MainManager.Instance.Player.HasMoved = false;
        //}

        //if (beatTimer > 0)
        //{
        //    beatTimer -= Time.deltaTime;

        //    if (beatTimer <= 0)
        //    {
        //        _beatSources[0].Play();
        //        beatIndex++;

        //        if (beatIndex > Bar.Beats.Count - 1)
        //        {
        //            beatIndex = 0;
        //            SpawnMetronome();
        //        }

        //        beatTimer = Bar.Beats[beatIndex].Delay;
        //    }
        //}
	}

    public void Play()
    {
        _beatSources[0].Play();
    }

    public void StartMetronome()
    {
        var y = Metronome.GetComponent<RectTransform>().sizeDelta.y;
        Metronome.GetComponent<CircleCollider2D>().radius = y/2f;

        Metronome.GetComponent<Metronome>().BeatIndicator = this;

        MoveMetronome();
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

        _beatLights = new List<BeatLight>();

        for (int i = 0; i < Bar.Beats.Count; i++)
        {
            var beatLight = Instantiate(BeatLightPrefab, BeatsParent).GetComponent<BeatLight>();
            
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
            

            beatLight.Light.color = STMYellow;

            _beatLights.Add(beatLight);
        }
    }

    public void StartBeat()
    {
        //StartCoroutine(Beat());
    }

    private IEnumerator Beat()
    {
        for (var i = 0; i < Bar.Beats.Count; i++)
        {
            yield return new WaitForSeconds(Bar.Beats[i].Delay);

            IsHot = true;

            yield return new WaitForSeconds(BeatWindowPadding);

            if (!Bar.Beats[i].IsHigh)
            {
                _beatSources[0].Play();
            }
            else
            {
                _beatSources[1].Play();
            }

            yield return new WaitForSeconds(BeatWindowPadding);

            IsHot = false;
        }

        Debug.Log(wtv);
        wtv = 0f;
        StartCoroutine(Beat());

    }

    private void ResetLight(BeatLight beatLight)
    {
        beatLight.Light.DOColor(STMYellow, .1f);
    }

    public void StopBeat()
    {
        if (_beatLights.Count > 0)
        {
            for (int i = 0; i < _beatLights.Count; i++)
            {
                Destroy(_beatLights[i].gameObject);
            }
        }
        Destroy(Metronome);
        StopAllCoroutines();
    }
}