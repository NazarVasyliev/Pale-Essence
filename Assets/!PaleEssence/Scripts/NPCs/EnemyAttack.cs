using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyDamageDealer : MonoBehaviour
{
    public float damageAmount = 10f;
    public float damageCooldown = 1f;
    [Header("Hit Stop & Effect")]
    [SerializeField] private float hitStopDuration = 0.2f;
    [SerializeField] private float timeScaleDuringHitStop = 0.1f;
    [SerializeField] private Material hitEffectMaterial;
    private List<Renderer> renderersWithHitEffect = new List<Renderer>();
    private bool hitStopActive = false;
    private HashSet<GameObject> damagedEnemiesInCurrentAttack = new HashSet<GameObject>();
    private float currentHitStopPauseEndTime;

    private void OnTriggerEnter(Collider collision)
    {
        if (!collision.CompareTag("Player")) return;

        PlayerStats playerHealth = collision.gameObject.GetComponent<PlayerStats>();


        if (playerHealth != null)
        {

            playerHealth.TakeDamage(damageAmount);
            gameObject.GetComponent<SphereCollider>().enabled = false;


        }
        var renderers = collision.gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;

            List<Material> currentMaterials = renderer.sharedMaterials.ToList();
            currentMaterials.Add(hitEffectMaterial);
            renderer.materials = currentMaterials.ToArray();


            if (!renderersWithHitEffect.Contains(renderer))
            {
                renderersWithHitEffect.Add(renderer);
            }
        }

        currentHitStopPauseEndTime = Time.realtimeSinceStartup + hitStopDuration;
        damagedEnemiesInCurrentAttack.Add(collision.gameObject);

        if (!hitStopActive)
        {
            StartCoroutine(ExecuteHitStopSequence());
        }

    }
    private IEnumerator ExecuteHitStopSequence()
    {
        hitStopActive = true;
        float originalTimeScale = Time.timeScale;


        while (Time.realtimeSinceStartup < currentHitStopPauseEndTime)
        {
            yield return null;
        }



        foreach (Renderer renderer in renderersWithHitEffect)
        {
            if (renderer != null)
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    Debug.Log(material);
                }
                List<Material> currentMaterials = renderer.sharedMaterials.ToList();
                currentMaterials.Remove(hitEffectMaterial);
                renderer.sharedMaterials = currentMaterials.ToArray();
            }
        }
        renderersWithHitEffect.Clear();

        hitStopActive = false;
    }
}