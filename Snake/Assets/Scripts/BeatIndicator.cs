using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BeatIndicator : MonoBehaviour {

    public Bar Bar;
    public AudioClip LowBeat;
    public AudioClip HighBeat;
    public Sprite Quarter;
    public Sprite Third;
    public Sprite Half;
    public Color STMYellow;
    public Color STMBlue;

    private List<Image> _beatSections;
    private List<AudioSource> _beatSources;
    private Sprite _currentSection;

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

        if (_beatSections != null)
        {
            for (int i = 0; i < _beatSections.Count; i++)
            {
                Destroy(_beatSections[i].gameObject);
            }
        }

        CreateIndicator();
    }

    public void CreateIndicator()
    {
        switch (Bar.Beats.Count)
        {
            case 2:
                _currentSection = Half;
                break;
            case 3:
                _currentSection = Third;
                break;
            default:
                _currentSection = Quarter;
                break;
        }

        _beatSections = new List<Image>();

        for (int i = 0; i < Bar.Beats.Count; i++)
        {
            Image beatSection = new GameObject("Beat " + i).AddComponent<Image>();
            beatSection.transform.SetParent(transform, false);
            beatSection.sprite = _currentSection;
            beatSection.transform.Rotate(new Vector3(0, 0, -360/ Bar.Beats.Count * i));
            if (i == Bar.Beats.Count - 1)
            {
                beatSection.color = STMBlue;
            }
            else
            {
                beatSection.color = STMYellow;
            }
            _beatSections.Add(beatSection);
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
                ResetSection(_beatSections[_beatSections.Count - 1]);
            }

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

            _beatSections[i].transform.DOScale(Vector3.one * 1.5f, Bar.Beats[i].Delay);
            _beatSections[i].DOFade(0, Bar.Beats[i].Delay);
        }

        StartCoroutine(Beat());

        for (int i = 0; i < _beatSections.Count - 1; i++)
        {
            ResetSection(_beatSections[i]);
        }
    }

    private void ResetSection(Image section)
    {
        section.transform.localScale = Vector3.zero;
        var tempColor = section.color;
        tempColor.a = 1f;
        section.color = tempColor;
        section.transform.DOScale(Vector3.one, 0.1f);
    }
}
