using System.Reflection;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AlphaChanger : MonoBehaviour
{
    public float fadeDuration = 0.5f;  
    public float visibleAlpha = 1f;
    public float hiddenAlpha = 0f;   

    private Component targetComponent;
    private PropertyInfo colorProperty;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        targetComponent = GetComponent<Image>() as Component
                       ?? GetComponent<RawImage>() as Component
                       ?? GetComponent<Text>() as Component;

        if (targetComponent != null)
        {
            colorProperty = targetComponent.GetType().GetProperty("color");
        }
    }

    void OnEnable()
    {
        StartFade(visibleAlpha);
    }

    void OnDisable()
    {
        StartFade(hiddenAlpha);
    }

    public void StartFade(float targetAlpha)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToAlpha(targetAlpha));
    }

    private IEnumerator FadeToAlpha(float targetAlpha)
    {
        if (targetComponent == null || colorProperty == null)
            yield break;

        Color currentColor = (Color)colorProperty.GetValue(targetComponent, null);
        float startAlpha = currentColor.a;
        float time = 0f;

        while (time < fadeDuration)
        {
            float t = time / fadeDuration;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            currentColor.a = newAlpha;
            colorProperty.SetValue(targetComponent, currentColor, null);
            time += Time.unscaledDeltaTime; 
            yield return null;
        }

        currentColor.a = targetAlpha;
        colorProperty.SetValue(targetComponent, currentColor, null);
    }
}