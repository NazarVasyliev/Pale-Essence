using UnityEngine;
using UnityEngine.AI; 
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Detection parameters")]
    public float chaseRadius = 10f;
    public float detectionRadius = 5f; 
    public float attackRadius = 2f; 

    [Header("Attack parameters")]
    public float attackCooldown = 2f;
    private float lastAttackTime = 0f;

    private Transform player; 
    private NavMeshAgent agent; 
    private Animator animator;

    private enum AIState
    {
        Idle, 
        Chasing, 
        Attacking
    }
    private AIState currentState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        animator = GetComponent<Animator>();
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            this.enabled = false;
        }

        currentState = AIState.Idle;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case AIState.Idle:
                animator.SetFloat("Speed", 0);
                if (distanceToPlayer <= chaseRadius)
                {
                    currentState = AIState.Chasing;
                    agent.isStopped = false;
                }
                break;

            case AIState.Chasing:
                agent.SetDestination(player.position);
                animator.SetFloat("Speed", 1);
                if (distanceToPlayer <= attackRadius)
                {
                    currentState = AIState.Attacking;
                    agent.isStopped = true;
                }
                else if (distanceToPlayer > chaseRadius)
                {
                    currentState = AIState.Idle;
                    agent.isStopped = true;
                    agent.ResetPath();
                }
                break;

            case AIState.Attacking:
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    Attack();
                    lastAttackTime = Time.time;
                }

                if (distanceToPlayer > attackRadius)
                {
                    animator.SetBool("Attacking", false);
                    currentState = AIState.Chasing;
                    agent.isStopped = false; 
                }
                break;
        }
    }

    void Attack()
    {
        animator.SetBool("Attacking", true);
        transform.LookAt(player);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}