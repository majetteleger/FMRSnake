using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ZoneIndicator : MonoBehaviour
{
    public float ScreenBorderOffset;
    public float MobileBorderOffset;
    public Image FillImage;
    public float FadeTime;
    public float AlphaValue;

    private Zone _targetZone;
    private CanvasGroup _canvasGroup;
    private bool _isOn;
    private float _actualScreenBorderOffset;

    public void Initialize(Zone targetZone)
    {
        _targetZone = targetZone;
            
        FillImage.color = targetZone.ZoneModifier.Color;
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = AlphaValue;
        UpdateTransform();

        _actualScreenBorderOffset = Screen.width < Screen.height ? MobileBorderOffset : ScreenBorderOffset;
    }

    private void Update()
    {
        if (_targetZone == null)
        {
            return;
        }

        if (_targetZone.IsVisibleOnScreen())
        {
            _canvasGroup.DOFade(0f, MainManager.Instance.PulseTime);
            _isOn = false;
            return;
        }
        
        UpdateTransform();

        if (!_isOn)
        {
            _canvasGroup.DOFade(AlphaValue, MainManager.Instance.PulseTime);
            _isOn = true;
        }
    }

    private void UpdateTransform()
    {
        var zonePosition = Camera.main.WorldToScreenPoint(_targetZone.transform.position);
        var newPosition = new Vector3
        {
            x = Mathf.Clamp(zonePosition.x, 0f + _actualScreenBorderOffset, Screen.width - _actualScreenBorderOffset),
            y = Mathf.Clamp(zonePosition.y, 0f + _actualScreenBorderOffset, Screen.height - _actualScreenBorderOffset)
        };

        var newScreenPosition = newPosition;
        
        transform.position = newScreenPosition;

        var targetPosLocal = Camera.main.transform.InverseTransformPoint(_targetZone.transform.position);
        var targetAngle = -Mathf.Atan2(targetPosLocal.x, targetPosLocal.y) * Mathf.Rad2Deg - 90;

        transform.eulerAngles = new Vector3(0, 0, targetAngle);
    }
}
