using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public Transform CamTransform;
    public float DecreaseFactor;

    private float _shakeDuration;
    private float _shakeAmount;

    Vector3 originalPos;

    void Awake()
    {
        if (CamTransform == null)
        {
            CamTransform = GetComponent(typeof(Transform)) as Transform;
        }
    }

    void OnEnable()
    {
        originalPos = CamTransform.localPosition;
    }

    void Update()
    {
        if (_shakeDuration > 0)
        {
            CamTransform.localPosition = originalPos + Random.insideUnitSphere * _shakeAmount;

            _shakeDuration -= Time.deltaTime * DecreaseFactor;
        }
        else
        {
            _shakeDuration = 0f;
            CamTransform.localPosition = originalPos;
        }
    }

    public void Shake(float amount, float duration)
    {
        _shakeAmount = amount;
        _shakeDuration += duration;
    }
}
