using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ZoneIndicator : MonoBehaviour
{
    public float ScreenBorderOffset;
    public Image FillImage;
    public float FadeTime;
    public float AlphaValue;
    //public float DoMoveDistanceThreshold;
    //public float DoMoveTime;

    private Zone _targetZone;
    private CanvasGroup _canvasGroup;
    private bool _isOn;
    //private ZoneIndicatorShield _shield;
    //private Rect[] _otherZoneIndicatorShieldRects;
    //private Rect _thisZoneIndicatorRect;
    
    public void Initialize(Zone targetZone)
    {
        _targetZone = targetZone;
        //_shield = GetComponent<ZoneIndicatorShield>();
            
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

        /*_otherZoneIndicatorShieldRects = FindObjectsOfType<ZoneIndicatorShield>().Where(x => x.GetComponentInParent<ZoneIndicator>() == null || x.GetComponentInParent<ZoneIndicator>() != this)
            .Select(x => x.GetRect()).ToArray();*/

        UpdateTransform();

        if (!_isOn)
        {
            _canvasGroup.DOFade(AlphaValue, FadeTime);
            _isOn = true;
        }
    }

    private void UpdateTransform()
    {
        // can we clamp by pixel value and not by percentage of screen?
        // can we avoid overlap with other ui elements?

        var zonePosition = Camera.main.WorldToViewportPoint(_targetZone.transform.position);
        var newPosition = new Vector3
        {
            x = Mathf.Clamp(zonePosition.x, 0f + ScreenBorderOffset, 1f - ScreenBorderOffset),
            y = Mathf.Clamp(zonePosition.y, 0f + ScreenBorderOffset, 1f - ScreenBorderOffset)
        };

        var newScreenPosition = Camera.main.ViewportToScreenPoint(newPosition);

        /*var isOverlappingAnother = false;

        if (_otherZoneIndicatorShieldRects != null)
        {
            _thisZoneIndicatorRect = _shield.GetRect();

            foreach (var otherZoneIndicatorShieldRect in _otherZoneIndicatorShieldRects)
            {
                isOverlappingAnother = _thisZoneIndicatorRect.Overlaps(otherZoneIndicatorShieldRect);

                if (isOverlappingAnother)
                {
                    break;
                }
            }
        }*/

        /*if (!isOverlappingAnother)
        {
            if (Vector3.Distance(transform.position, newScreenPosition) > DoMoveDistanceThreshold)
            {
                transform.DOMove(newScreenPosition, DoMoveTime);
            }
            else
            {*/
        transform.position = newScreenPosition;
            /*}
        }*/
        
        var targetPosLocal = Camera.main.transform.InverseTransformPoint(_targetZone.transform.position);
        var targetAngle = -Mathf.Atan2(targetPosLocal.x, targetPosLocal.y) * Mathf.Rad2Deg - 90;

        transform.eulerAngles = new Vector3(0, 0, targetAngle);
    }
}
