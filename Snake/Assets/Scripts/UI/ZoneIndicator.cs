using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ZoneIndicator : MonoBehaviour
{
    public float ScreenBorderOffsetTop;
    public float ScreenBorderOffsetRight;
    public float ScreenBorderOffsetDown;
    public float ScreenBorderOffsetLeft;
    public Image FillImage;
    public float FadeTime;
    public float AlphaValue;

    private Zone _targetZone;
    private CanvasGroup _canvasGroup;
    private bool _isOn;
    
    public void Initialize(Zone targetZone)
    {
        _targetZone = targetZone;
            
        FillImage.color = targetZone.ZoneModifier.Color;
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = AlphaValue;
        UpdateTransform();
    }

    private void Update()
    {
        if (_targetZone == null)
        {
            return;
        }

        if (_targetZone.IsVisibleOnScreen())
        {
            _canvasGroup.DOFade(0f, FadeTime);
            _isOn = false;
            return;
        }
        
        UpdateTransform();

        if (!_isOn)
        {
            _canvasGroup.DOFade(AlphaValue, FadeTime);
            _isOn = true;
        }
    }

    private void UpdateTransform()
    {
        var zonePosition = Camera.main.WorldToScreenPoint(_targetZone.transform.position);
        var newPosition = new Vector3
        {
            x = Mathf.Clamp(zonePosition.x, 0f + ScreenBorderOffsetLeft, Screen.width - ScreenBorderOffsetRight),
            y = Mathf.Clamp(zonePosition.y, 0f + ScreenBorderOffsetDown, Screen.height - ScreenBorderOffsetTop)
        };

        var newScreenPosition = newPosition;
        
        transform.position = newScreenPosition;

        var targetPosLocal = Camera.main.transform.InverseTransformPoint(_targetZone.transform.position);
        var targetAngle = -Mathf.Atan2(targetPosLocal.x, targetPosLocal.y) * Mathf.Rad2Deg - 90;

        transform.eulerAngles = new Vector3(0, 0, targetAngle);
    }
}
