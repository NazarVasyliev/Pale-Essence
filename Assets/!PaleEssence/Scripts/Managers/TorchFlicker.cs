using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class TorchFlicker : MonoBehaviour
{
    [Header("Intensity Settings")]
    [Tooltip("Minimum light intensity during flicker.")]
    public float minIntensity = 0.5f;
    [Tooltip("Maximum light intensity during flicker.")]
    public float maxIntensity = 1.5f;

    [Header("Range Settings (Optional)")]
    [Tooltip("Enable light range flickering.")]
    public bool affectRange = false;
    [Tooltip("Minimum light range (if affectRange is enabled).")]
    public float minRange = 8f;
    [Tooltip("Maximum light range (if affectRange is enabled).")]
    public float maxRange = 12f;

    [Header("Flicker Speed Settings")]
    [Tooltip("Minimum time (in seconds) before changing the flicker target.")]
    public float minFlickerInterval = 0.1f;
    [Tooltip("Maximum time (in seconds) before changing the flicker target.")]
    public float maxFlickerInterval = 0.5f;

    private Light _lightComponent;
    private float _baseIntensity;
    private float _baseRange;

    void Awake()
    {
        _lightComponent = GetComponent<Light>();
        if (_lightComponent == null)
        {
            Debug.LogError("Light component not found on this object. TorchFlicker script will not work.");
            enabled = false;
            return;
        }


        _baseIntensity = _lightComponent.intensity;
        _baseRange = _lightComponent.range;
    }

    void Start()
    {
        StartCoroutine(Flicker());
    }

    IEnumerator Flicker()
    {
        while (true)
        {

            float targetIntensity = Random.Range(minIntensity, maxIntensity);


            float targetRange = _lightComponent.range;
            if (affectRange)
            {
                targetRange = Random.Range(minRange, maxRange);
            }


            float currentInterval = Random.Range(minFlickerInterval, maxFlickerInterval);
            float timer = 0f;

            float startIntensity = _lightComponent.intensity;
            float startRange = _lightComponent.range;

            while (timer < currentInterval)
            {
                timer += Time.deltaTime;
                float progress = timer / currentInterval;


                _lightComponent.intensity = Mathf.Lerp(startIntensity, targetIntensity, progress);


                if (affectRange)
                {
                    _lightComponent.range = Mathf.Lerp(startRange, targetRange, progress);
                }

                yield return null;
            }


            _lightComponent.intensity = targetIntensity;
            if (affectRange)
            {
                _lightComponent.range = targetRange;
            }
        }
    }
}