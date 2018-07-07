using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class BeatIndicator : MonoBehaviour {

    public Bar Bar;
    public AudioClip LowBeat;
    public AudioClip HighBeat;
    public GameObject BeatLightPrefab;
    public Color STMYellow;
    public Color STMBlue;
    //public Sprite Quarter;
    //public Sprite Third;
    //public Sprite Half;

    private List<BeatLight> _beatLights;
    private List<AudioSource> _beatSources;
    //private Sprite _currentSection;
    //private List<Image> _beatSections;

    // Use this for initialization
    void Start () {
        _beatSources = new List<AudioSource>();
        _beatSources.Add(gameObject.AddComponent<AudioSource>());
        _beatSources[0].clip = LowBeat;
        _beatSources.Add(gameObject.AddComponent<AudioSource>());
        _beatSources[1].clip = HighBeat;
    }
	
	// Update is called once per frame
	void Update () {
		
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
    }

    public void CreateIndicator()
    {
        //switch (Bar.Beats.Count)
        //{
        //    case 2:
        //        _currentSection = Half;
        //        break;
        //    case 3:
        //        _currentSection = Third;
        //        break;
        //    default:
        //        _currentSection = Quarter;
        //        break;
        //}

        _beatLights = new List<BeatLight>();

        for (int i = 0; i < Bar.Beats.Count - 1; i++)
        {
            var beatLight = Instantiate(BeatLightPrefab, transform).GetComponent<BeatLight>();

            //Image beatSection = new GameObject("Beat " + i).AddComponent<Image>();
            //beatSection.transform.SetParent(transform, false);
            //beatSection.sprite = _currentSection;
            //beatSection.transform.Rotate(new Vector3(0, 0, -360/ Bar.Beats.Count * i));

            beatLight.Light.color = STMYellow;

            _beatLights.Add(beatLight);
        }
    }

    public void StartBeat()
    {
        StartCoroutine(Beat());
    }

    private IEnumerator Beat()
    {
        for (var i = 0; i < Bar.Beats.Count; i++)
        {

            yield return new WaitForSeconds(Bar.Beats[i].Delay);

            if (i == 0)
            {
                for (int ii = 0; ii < _beatLights.Count; ii++)
                {
                    ResetLight(_beatLights[ii]);
                }
            }
            //if (i == 0)
            //{
            //    ResetSection(_beatSections[_beatSections.Count - 1]);
            //}

            if (!Bar.Beats[i].IsHigh)
            {
                _beatSources[0].Play();
            }
            else
            {
                _beatSources[1].Play();

                var success = MainManager.Instance.Player.AttemptMove();

                if (!success)
                {
                    MainManager.Instance.Player.FailMove();
                }
            }

            if (i == Bar.Beats.Count - 1)
            {
                for (int ii = 0; ii < _beatLights.Count; ii++)
                {
                    _beatLights[ii].Light.color = STMBlue;
                    _beatLights[ii].transform.DOPunchScale(Vector3.one / 5, Bar.Beats[i].Delay / 5,1);
                }
            }
            else
            {
                _beatLights[i].Light.DOFade(1, .1f);
            }

            //_beatSections[i].transform.DOScale(Vector3.one * 1.5f, Bar.Beats[i].Delay);
            //_beatSections[i].DOFade(0, Bar.Beats[i].Delay);
        }

        StartCoroutine(Beat());

        //for (int i = 0; i < _beatSections.Count - 1; i++)
        //{
        //    ResetSection(_beatSections[i]);
        //}
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
        StopAllCoroutines();
    }

    //private void ResetSection(Image section)
    //{
    //    section.transform.localScale = Vector3.zero;
    //    var tempColor = section.color;
    //    tempColor.a = 1f;
    //    section.color = tempColor;
    //    section.transform.DOScale(Vector3.one, 0.1f);
    //}
}