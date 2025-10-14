using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class OrbController : MonoBehaviour
{
    [Header("Main Settings")]
    [Tooltip("Maximum value of health or stamina.")]
    public float maxValue = 100f;

    [Tooltip("Current value. Change in inspector only for debugging.")]
    [SerializeField] private float currentValue;

    [Tooltip("Speed at which the orb fills to the target value (units per second).")]
    public float animationSpeed = 50f;

    [Header("Regeneration")]
    [Tooltip("Enable automatic regeneration?")]
    public bool enableRegeneration = true;

    [Tooltip("Delay in seconds before regeneration starts after the last change.")]
    public float regenerationDelay = 3.0f;

    [Tooltip("Amount restored per second.")]
    public float regenerationRate = 10.0f;

    [Header("Low Health Effect")]
    [Tooltip("Enable pulsation effect when health is low?")]
    public bool enableLowHealthEffect = true;

    [Tooltip("Threshold (0–1) below which the effect is activated.")]
    [Range(0f, 1f)]
    public float lowHealthThreshold = 0.25f;

    [Tooltip("Speed of the pulsation effect.")]
    public float lowHealthPulseSpeed = 3.0f;

    [Header("Damage Flash Effect")]
    [Tooltip("Enable flash effect when taking damage?")]
    public bool enableDamageFlashEffect = true;

    [Tooltip("Maximum flash intensity.")]
    [Range(0f, 1f)]
    public float damageFlashIntensity = 1.0f;

    [Tooltip("Duration of the flash in seconds.")]
    public float damageFlashDuration = 0.5f;

    [Tooltip("Fade curve for the flash effect.")]
    public AnimationCurve damageFlashCurve = new AnimationCurve(
        new Keyframe(0, 1),
        new Keyframe(0.2f, 0.8f),
        new Keyframe(1, 0)
    );

    private Image uiImage;
    private Material orbMaterial;
    private float targetValue;
    private float timeSinceLastChangeForRegen;
    private float timeSinceDamageTakenForFlash;

    private static readonly int HealthProp = Shader.PropertyToID("_Health");
    private static readonly int InitialSplashMagnitudeProp = Shader.PropertyToID("_InitialSplashMagnitude");
    private static readonly int TimeSinceLastHPChangeProp = Shader.PropertyToID("_TimeSinceLastHPChange");
    private static readonly int EffectIntensityProp = Shader.PropertyToID("_EffectIntensity");

    void Awake()
    {
        uiImage = GetComponent<Image>();
        if (uiImage.material == null)
        {
            this.enabled = false;
            return;
        }

        orbMaterial = new Material(uiImage.material);
        uiImage.material = orbMaterial;

        currentValue = maxValue;
        targetValue = maxValue;
        timeSinceLastChangeForRegen = regenerationDelay + 1f;
        timeSinceDamageTakenForFlash = damageFlashDuration;
    }

    void Update()
    {
        if (!Mathf.Approximately(currentValue, targetValue))
        {
            currentValue = Mathf.MoveTowards(currentValue, targetValue, animationSpeed * Time.deltaTime);
        }

        orbMaterial.SetFloat(HealthProp, currentValue / maxValue);

        float lowHealthIntensity = GetLowHealthIntensity();
        float damageFlashIntensity = GetDamageFlashIntensity();
        orbMaterial.SetFloat(EffectIntensityProp, Mathf.Max(lowHealthIntensity, damageFlashIntensity));
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0) return;
        targetValue = Mathf.Max(0, targetValue - amount);
        timeSinceLastChangeForRegen = 0f;

        if (enableDamageFlashEffect)
        {
            timeSinceDamageTakenForFlash = 0f;
        }

        TriggerSplash(amount);
    }

    public void Heal(float amount, bool triggerSplash = true)
    {
        if (amount <= 0) return;
        targetValue = Mathf.Min(maxValue, targetValue + amount);
        if (triggerSplash)
        {
            TriggerSplash(amount);
        }
    }

    private void TriggerSplash(float changeAmount)
    {
        float magnitude = Mathf.Clamp01(changeAmount / maxValue);
        orbMaterial.SetFloat(InitialSplashMagnitudeProp, magnitude);
        orbMaterial.SetFloat(TimeSinceLastHPChangeProp, Time.time);
    }

    private float GetLowHealthIntensity()
    {
        if (!enableLowHealthEffect) return 0f;
        if (currentValue / maxValue < lowHealthThreshold)
        {
            float pulse = (Mathf.Sin(Time.time * lowHealthPulseSpeed) + 1f) / 2f;
            return pulse;
        }
        return 0f;
    }

    public void SetValueImmediate(float value)
    {
        currentValue = Mathf.Clamp(value, 0, maxValue);
        targetValue = currentValue;
        orbMaterial.SetFloat(HealthProp, currentValue / maxValue);
    }

    private float GetDamageFlashIntensity()
    {
        if (!enableDamageFlashEffect || timeSinceDamageTakenForFlash >= damageFlashDuration)
        {
            return 0f;
        }

        timeSinceDamageTakenForFlash += Time.deltaTime;
        float progress = timeSinceDamageTakenForFlash / damageFlashDuration;
        float intensity = damageFlashCurve.Evaluate(progress) * this.damageFlashIntensity;
        return intensity;
    }

    [ContextMenu("Test: Take 20 Damage")]
    void TestDamage() => TakeDamage(20);

    [ContextMenu("Test: Heal 15")]
    void TestHeal() => Heal(15);
}
