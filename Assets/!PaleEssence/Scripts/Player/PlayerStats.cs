using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private OrbController healthOrbController;
    private OrbController staminaOrbController;

    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Stamina")]
    public float maxStamina = 100f;
    [Tooltip("Stamina regeneration rate")]
    public float staminaRegenRate = 25f;
    [Tooltip("Stamina regeneration delay")]
    public float staminaRegenDelay = 1.5f;
    private float currentStamina;
    private float timeSinceStaminaUsed;

    public float CurrentHealth => currentHealth;
    public float CurrentStamina => currentStamina;

    private const string HEALTH_ORB_TAG = "Health";
    private const string STAMINA_ORB_TAG = "Stamina";

    void Start()
    {
        GameObject healthObject = GameObject.FindGameObjectWithTag(HEALTH_ORB_TAG);
        GameObject staminaObject = GameObject.FindGameObjectWithTag(STAMINA_ORB_TAG);

        if (healthObject != null)
        {
            healthOrbController = healthObject.GetComponent<OrbController>();
        }

        if (staminaObject != null)
        {
            staminaOrbController = staminaObject.GetComponent<OrbController>();
        }

        if (healthOrbController == null)
        {
            enabled = false;
            return;
        }

        if (staminaOrbController == null)
        {
            enabled = false;
            return;
        }

        currentHealth = maxHealth;
        healthOrbController.maxValue = maxHealth;
        healthOrbController.SetValueImmediate(currentHealth);

        currentStamina = maxStamina;
        staminaOrbController.maxValue = maxStamina;
        staminaOrbController.SetValueImmediate(currentStamina);

        timeSinceStaminaUsed = staminaRegenDelay;
    }

    void Update()
    {
        HandleStaminaRegeneration();
    }

    private void HandleStaminaRegeneration()
    {
        timeSinceStaminaUsed += Time.deltaTime;

        if (timeSinceStaminaUsed >= staminaRegenDelay && currentStamina < maxStamina)
        {
            float regenAmount = staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(maxStamina, currentStamina + regenAmount);

            staminaOrbController.Heal(regenAmount, false);
        }
    }


    public void TakeDamage(float amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        healthOrbController.TakeDamage(amount);

        if (currentHealth <= 0)
        {
            Die();
        }
    }


    public void Heal(float amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        healthOrbController.Heal(amount);
    }


    public bool TryUseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            staminaOrbController.TakeDamage(amount);
            timeSinceStaminaUsed = 0f; 
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Die()
    {
    }

    [ContextMenu("Test: Take 15 Damage")]
    private void TestDamage() => TakeDamage(15);

    [ContextMenu("Test: Heal 10")]
    private void TestHeal() => Heal(10);

    [ContextMenu("Test: Use 25 Stamina")]
    private void TestStamina() => TryUseStamina(25);
}