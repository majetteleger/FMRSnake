using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ZoneIndicator : MonoBehaviour
{
    public float ScreenBorderOffset;
    public float FadeTime;

    private Zone _targetZone;
    private Image _image;
    private bool _isOn;

    public void Initialize(Zone targetZone)
    {
        _targetZone = targetZone;
        _image = GetComponent<Image>();
        
        _image.color = targetZone.ZoneModifier.Color;

        UpdateTransform();
        _image.enabled = true;
    }

    private void Update()
    {
        if (_targetZone == null)
        {
            return;
        }

        if (_targetZone.IsVisibleOnScreen())
        {
            _image.DOFade(0f, FadeTime);
            _isOn = false;
            return;
        }

        UpdateTransform();

        if (!_isOn)
        {
            _image.DOFade(_targetZone.ZoneModifier.Color.a, FadeTime);
            _isOn = true;
        }
    }

    private void UpdateTransform()
    {
        var zonePosition = Camera.main.WorldToViewportPoint(_targetZone.transform.position);
        var newPositionX = Mathf.Clamp(zonePosition.x, 0f + ScreenBorderOffset, 1f - ScreenBorderOffset);
        var newPositionY = Mathf.Clamp(zonePosition.y, 0f + ScreenBorderOffset, 1f - ScreenBorderOffset);

        // can we clamp by pixel value and not by percentage of screen?
        // can we avoid overlap with other ui elements?

        transform.position = Camera.main.ViewportToScreenPoint(new Vector3(newPositionX, newPositionY, 0f));

        var targetPosLocal = Camera.main.transform.InverseTransformPoint(_targetZone.transform.position);
        var targetAngle = -Mathf.Atan2(targetPosLocal.x, targetPosLocal.y) * Mathf.Rad2Deg - 90;

        transform.eulerAngles = new Vector3(0, 0, targetAngle);
    }
}
