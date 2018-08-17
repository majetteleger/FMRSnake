using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class BeatIndicator : MonoBehaviour {

    public Bar BaseBar;
    public Metronome Metronome;
    public Image MetronomeImage;
    public Image BarImage;
    public GameObject DummyMetronomePrefab;
    public GameObject PassiveBeatPrefab;
    public GameObject ActiveBeatPrefab;
    public float Tempo;
    public Color SuccessColor;
    public Color FailureColor;
    public float BeatTime;

    public Bar IncomingBar { get; set; }
    public bool SwitchingBar { get; set; }
    public Bar Bar { get; set; }
    public ActiveBeat CurrentActiveBeat { get; set; }
    public bool IsHot { get; set; }
    public bool UpdateBarAtNextBeat { get; set; }
    public List<PassiveBeat> PassiveBeats { get; set; }

    private Vector2 _metronomeStartPos;
    private float _halfDistance;
    private List<ActiveBeat> _activeBeats;
    private float _beatTimer;
    private int _beatIndex;

    void Start()
    {
        _activeBeats = new List<ActiveBeat>();

        var y = Metronome.GetComponent<RectTransform>().sizeDelta.y;
        Metronome.GetComponent<CircleCollider2D>().radius = y / 2f;
        Metronome.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        _beatTimer = BeatTime / 2;
    }


    private void Update()        
    {
        if (MainManager.Instance.Player.Dead)
        {
            for (var i = 0; i < _activeBeats.Count; i++)
            {
                var activeBeat = _activeBeats[i];
                RemoveBeat(activeBeat);
            }

            return;
        }
        
        if (_beatTimer > 0f)
        {
            _beatTimer -= Time.deltaTime;

            if (_beatTimer < 0)
            {
                _beatIndex++;

                if (IncomingBar != null)
                {
                    Bar = IncomingBar;
                    IncomingBar = null;
                    _beatIndex = 0;
                    for (int i = 0; i < _activeBeats.Count; i++)
                    {
                        if (!_activeBeats[i].HasPlayed)
                        {
                            RemoveBeat(_activeBeats[i]);
                        }
                    }
                }

                if (_beatIndex > Bar.Beats.Length - 1)
                {
                    _beatIndex = 0;
                }

                if (Bar.Beats[_beatIndex])
                {
                    SpawnActiveBeat();
                }

                _beatTimer = BeatTime / 2;
            }
        }

    }

    public void StartMetronome()
    {
        var y = Metronome.GetComponent<RectTransform>().sizeDelta.y;
        //Metronome.GetComponent<CircleCollider2D>().radius = y/2f;

        //ResetMetronome();
    }

    private void MoveMetronome()
    {
        Metronome.GetComponent<RectTransform>().DOAnchorPosX(PassiveBeats[PassiveBeats.Count - 1].GetComponent<RectTransform>().anchoredPosition.x + _halfDistance, Tempo / 60 * 2).SetEase(Ease.Linear).OnComplete(ResetMetronome);
    }

    private void ResetMetronome()
    {
        //Metronome.GetComponent<RectTransform>().anchoredPosition = _metronomeStartPos;

        //AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.MetronomeReset);

        //for (int i = 0; i < PassiveBeats.Count; i++)
        //{
        //    PassiveBeats[i].HasPlayed = false;
        //}

        //for (int i = 0; i < _activeBeats.Count; i++)
        //{
        //    _activeBeats[i].ResetBeat();
        //    _activeBeats[i].HasPlayed = false;
        //}

        //MoveMetronome();
    }

    public void CreatePassiveBeats()
    {
        if (PassiveBeats != null && PassiveBeats.Count != 0)
        {
            for (int i = 0; i < PassiveBeats.Count; i++)
            {
                Destroy(PassiveBeats[i].gameObject);
            }
        }

        PassiveBeats = new List<PassiveBeat>();

        for (int i = 0; i < 9; i++)
        {

            var yPos = 0;
            var xPos = ((Screen.width / 9) * i) - Screen.width / 2;

            if (i == 0)
            {
                continue;
            }

            var passiveBeat = Instantiate(PassiveBeatPrefab, transform).GetComponent<PassiveBeat>();
            PassiveBeats.Add(passiveBeat);
            passiveBeat.name = "PassiveBeat " + (i - 1).ToString();

            passiveBeat.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, yPos);
        }

        SetMetronomeStartingPos();
    }

    private void SetMetronomeStartingPos()
    {
        var yPos = 0;
        var xPos = - Screen.width / 2;
        _halfDistance = (Vector2.Distance(PassiveBeats[4].GetComponent<RectTransform>().anchoredPosition, PassiveBeats[5].GetComponent<RectTransform>().anchoredPosition)) / 2;
        _metronomeStartPos = new Vector2(xPos + _halfDistance, yPos);
    }

    public void StopBeat()
    {
        Metronome.transform.DOKill();

    }

    public void UpdateBar(Bar newBar)
    {
        UpdateBarAtNextBeat = false;

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
        if (_activeBeats != null && _activeBeats.Count != 0)
        {
            for (int i = 0; i < _activeBeats.Count; i++)
            {
                Destroy(_activeBeats[i].gameObject);
            }
        }

        _activeBeats = new List<ActiveBeat>();

        for (int i = 0; i < Bar.Beats.Length; i++)
        {
            if (Bar.Beats[i])
            {
                if (i % 2 == 0) // this is a BEAT in the first half
                {
                    var yPos = 0;
                    var xPos = PassiveBeats[i / 2].GetComponent<RectTransform>().anchoredPosition.x;
                    var anchoredPos = new Vector2(xPos, yPos);
                    InstantiateBeat(anchoredPos, false);
                }
                else // this is an OFFBEAT in the first half
                {
                    var yPos = 0;
                    var xPos = PassiveBeats[Mathf.FloorToInt(i / 2)].GetComponent<RectTransform>().anchoredPosition.x + _halfDistance;
                    var anchoredPos = new Vector2(xPos, yPos);
                    InstantiateBeat(anchoredPos, true);
                }

                if (i % 2 == 0) // this is a BEAT in the second half
                {
                    var yPos = 0;
                    var xPos = PassiveBeats[i/2 + 4].GetComponent<RectTransform>().anchoredPosition.x;
                    var anchoredPos = new Vector2(xPos, yPos);
                    InstantiateBeat(anchoredPos, false);
                }
                else // this is an OFFBEAT in the second half
                {
                    var yPos = 0;
                    var xPos = PassiveBeats[Mathf.FloorToInt(i / 2) + 4].GetComponent<RectTransform>().anchoredPosition.x + _halfDistance;
                    var anchoredPos = new Vector2(xPos, yPos);
                    InstantiateBeat(anchoredPos, true);
                }
            }
        }
    }

    public void CreateDummyMetronome(bool onBeat)
    {
        if (MainManager.Instance.CurrentState != MainManager.GameState.Play || !MainManager.Instance.Player.MovedOnce)
        {
            return;
        }

        var metronomeDummy = Instantiate(DummyMetronomePrefab, Metronome.transform.position, Quaternion.identity, transform);

        if (onBeat)
        {
            metronomeDummy.GetComponent<Image>().color = SuccessColor;
        }
        else
        {
            metronomeDummy.GetComponent<Image>().color = FailureColor;
            MainManager.Instance.Player.MovementMultiplier -= MainManager.Instance.Player.MultiplierDecreaseOnSpam;

            MainManager.Instance.CameraShake.Shake(MainManager.Instance.Player.MissShakeAmount, MainManager.Instance.PulseTime);
        }

        metronomeDummy.GetComponent<Image>().DOFade(0, 0.5f).OnComplete(() => Destroy(metronomeDummy.gameObject));
        metronomeDummy.transform.DOScale(Vector3.one * 1.3f, 0.5f);
    }

    private void InstantiateBeat(Vector2 anchoredPosition, bool isOffBeat)
    {
        var activeBeat = Instantiate(ActiveBeatPrefab, transform).GetComponent<ActiveBeat>();
        activeBeat.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
        _activeBeats.Add(activeBeat);
        activeBeat.GetComponent<RectTransform>().DOAnchorPosX(-Screen.width / 2, Tempo).SetSpeedBased(true).SetEase(Ease.Linear).OnComplete(() => RemoveBeat(activeBeat));
    }

    private void RemoveBeat(ActiveBeat activeBeat)
    {
        _activeBeats.Remove(activeBeat);
        Destroy(activeBeat.gameObject);
    }

    private void Pop(ActiveBeat activeBeat)
    {
        activeBeat.GetComponent<RectTransform>().DOKill();
        activeBeat.GetComponent<Image>().DOFade(0, 0.2f);
        activeBeat.Image.GetComponent<Image>().DOFade(0, 0.2f);
        activeBeat.GetComponent<RectTransform>().DOScale(Vector3.one * 1.5f, 0.2f).OnComplete(() => RemoveBeat(activeBeat));

        MainManager.Instance.Player.PulseHeadSegment();
        GridPlayground.Instance.PulseObstacles();
        GridPlayground.Instance.PulseFoods();
        AudioManager.Instance.PlayActiveBeat();
        activeBeat.HasPlayed = true;
    }

    public void SpawnActiveBeat()
    {
        var yPos = 0;

        var xPosRight = GetComponent<RectTransform>().rect.width / 2;
        var xPosLeft = -GetComponent<RectTransform>().rect.width / 2;

        var anchoredPosRight = new Vector2(xPosRight, yPos);
        var anchoredPosLeft = new Vector2(xPosLeft, yPos);

        var activeBeatRight = Instantiate(ActiveBeatPrefab, transform).GetComponent<ActiveBeat>();
        var activeBeatLeft = Instantiate(ActiveBeatPrefab, transform).GetComponent<ActiveBeat>();

        activeBeatLeft.GetComponent<CircleCollider2D>().enabled = false;

        var activeBeatRectRight = activeBeatRight.GetComponent<RectTransform>();
        var activeBeatRectLeft = activeBeatLeft.GetComponent<RectTransform>();

        activeBeatRectRight.localScale = Vector3.zero;
        activeBeatRectLeft.localScale = Vector3.zero;

        _activeBeats.Add(activeBeatRight);
        _activeBeats.Add(activeBeatLeft);

        activeBeatRectRight.DOScale(Vector3.one, 0.2f);
        activeBeatRectLeft.DOScale(Vector3.one, 0.2f);

        activeBeatRectRight.anchoredPosition = anchoredPosRight;
        activeBeatRectLeft.anchoredPosition = anchoredPosLeft;

        activeBeatRectRight.DOAnchorPosX(0, Tempo).SetSpeedBased(true).SetEase(Ease.Linear).OnComplete(() => Pop(activeBeatRight));
        activeBeatRectLeft.DOAnchorPosX(0, Tempo).SetSpeedBased(true).SetEase(Ease.Linear).OnComplete(() => Pop(activeBeatLeft));
    }
}