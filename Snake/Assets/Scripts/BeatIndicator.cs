using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class BeatIndicator : MonoBehaviour {

    public Bar Bar;
    public GameObject MetronomePrefab;
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

    private GameObject _metronome;
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
        _metronome = Instantiate(MetronomePrefab, transform);
        _metronome.GetComponent<RectTransform>().sizeDelta = new Vector2(BeatsParent.GetComponent<RectTransform>().rect.size.y, BeatsParent.GetComponent<RectTransform>().rect.size.y);
        _metronome.GetComponent<Metronome>().BeatIndicator = this;
        MoveMetronome();
    }

    private void MoveMetronome()
    {
        _metronome.transform.DOMoveX(Camera.main.ViewportToScreenPoint(new Vector3(1, 0.9f, 10)).x, BeatSpeed).SetEase(Ease.Linear).OnComplete(ResetMetronome);
    }

    private void ResetMetronome()
    {
        var metronomePos = _metronome.GetComponent<RectTransform>().position;
        metronomePos.x = 0;
        _metronome.GetComponent<RectTransform>().position = metronomePos;

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
        _metronome.transform.DOKill();
        ResetMetronome();
    }

    public void CreateIndicator()
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

            beatLight.GetComponent<RectTransform>().sizeDelta = new Vector2(BeatsParent.GetComponent<RectTransform>().rect.size.y,BeatsParent.GetComponent<RectTransform>().rect.size.y);

            var xPos = (Bar.Beats[i].Delay * i / totalTimeOfBar * BeatsParent.GetComponent<RectTransform>().rect.size.x) - BeatsParent.GetComponent<RectTransform>().rect.size.x / 2;

            beatLight.GetComponent<RectTransform>().DOAnchorPosX(xPos, .3f);

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
        Destroy(_metronome);
        StopAllCoroutines();
    }
}