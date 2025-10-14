using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;

public class Melee : MonoBehaviour
{
    public string enemyTag = "Enemy";
    public int[] damageAmount;
    public float[] attackDuration; 
    public float[] attackSpeed;
    public float[] attackStamina;
    private InputAction attackInput;
    private Animator _animator;
    public int attackSeries = 0;

    public bool isAttacking = false;
    private HashSet<GameObject> damagedEnemiesInCurrentAttack = new HashSet<GameObject>(); 
    [SerializeField] private AnimationCurve[] _meleeDashSpeedCurve;
    private PlayerController cController; 

    [Header("Hit Stop & Effect")]
    [SerializeField] private float hitStopDuration = 0.2f;
    [SerializeField] private float timeScaleDuringHitStop = 0.1f;
    [SerializeField] private Material hitEffectMaterial;

    private List<Renderer> renderersWithHitEffect = new List<Renderer>();
    private bool hitStopActive = false;
    private float currentHitStopPauseEndTime;
    private PlayerStats playerStats;


    private void Start()
    {
        attackInput = InputSystem.actions.FindAction("LightAttack");
        _animator = GetComponentInParent<Animator>();
        cController = GetComponentInParent<PlayerController>();
        playerStats = cController.gameObject.GetComponent<PlayerStats>();
        if (GetComponent<CapsuleCollider>() == null)
        {
            Debug.LogError("Melee script on " + gameObject.name + " requires a CapsuleCollider component for OnTriggerEnter.", this);
        }
        else
        {
            GetComponent<CapsuleCollider>().isTrigger = true; 
            GetComponent<CapsuleCollider>().enabled = false;  
        }
    }

    private void Update()
    {
        if (attackInput.IsPressed() && !isAttacking && playerStats.TryUseStamina(attackStamina[attackSeries]))
        {
            PerformAttack();
        }
        if (cController != null && cController._dashing)
        {
            if (attackSeries < _meleeDashSpeedCurve.Length && attackSeries < attackSpeed.Length && attackSeries < attackDuration.Length)
            {
                cController.Dash(_meleeDashSpeedCurve[attackSeries], attackSpeed[attackSeries], attackDuration[attackSeries]);
            }
            else
            {
            }
        }
    }

    private void PerformAttack()
    {
        isAttacking = true;
        if (cController != null) cController.InitiateDash();
        _animator.SetTrigger("Attack");
    }

    public void StartAttack()
    {
        if (GetComponent<CapsuleCollider>() != null)
        {
            GetComponent<CapsuleCollider>().enabled = true;
        }
        damagedEnemiesInCurrentAttack.Clear();
    }

    public void EndAttack()
    {
        if (GetComponent<CapsuleCollider>() != null)
        {
            GetComponent<CapsuleCollider>().enabled = false;
        }
        isAttacking = false;

        if (attackSeries >= damageAmount.Length - 1 || attackSeries >= 2)
        {
            attackSeries = 0;
        }
        else
        {
            ++attackSeries;
        }
        _animator.SetInteger("AttackSeries", attackSeries);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isAttacking || !other.CompareTag(enemyTag) || hitEffectMaterial == null) return;

        GameObject enemyObject = other.gameObject;

        if (!damagedEnemiesInCurrentAttack.Contains(enemyObject))
        {
            var health = enemyObject.GetComponent<EnemyHealth>();
            if (health != null)
            {
                if (attackSeries < damageAmount.Length)
                {
                    health.TakeDamage(damageAmount[attackSeries]);
                }
                else
                {
                    if (damageAmount.Length > 0) health.TakeDamage(damageAmount[damageAmount.Length - 1]);
                }
            }

            Rigidbody enemyRigidbody = enemyObject.GetComponent<Rigidbody>();
            if (enemyRigidbody != null)
            {
                Vector3 forceDirection = (enemyObject.transform.position - transform.position).normalized;
                enemyRigidbody.AddForce(forceDirection * 10f + Vector3.up * 3f, ForceMode.Impulse);
            }

            var renderers = enemyObject.GetComponentsInChildren<Renderer>();
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
            damagedEnemiesInCurrentAttack.Add(enemyObject);

            if (!hitStopActive)
            {
                StartCoroutine(ExecuteHitStopSequence());
            }
        }
    }

    private IEnumerator ExecuteHitStopSequence()
    {
        hitStopActive = true;
        float originalTimeScale = Time.timeScale; 
        Time.timeScale = timeScaleDuringHitStop;

        while (Time.realtimeSinceStartup < currentHitStopPauseEndTime)
        {
            yield return null;
        }

        Time.timeScale = originalTimeScale;

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
