using System.Collections;
using UnityEngine;

public class DamageOnContact : MonoBehaviour
{
    public float damageAmount = 10f;
    public float damageCooldown = 1f;
    private Collider spikesCollider;
    void Start()
    {
        spikesCollider = gameObject.GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!collision.CompareTag("Player")) return;
        PlayerStats playerHealth = collision.gameObject.GetComponent<PlayerStats>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
            spikesCollider.enabled = false;
            StartCoroutine(EnableCollider());

        }
    }

    IEnumerator EnableCollider()
    {
        yield return new WaitForSeconds(1f);
        spikesCollider.enabled = true;
    }
}
