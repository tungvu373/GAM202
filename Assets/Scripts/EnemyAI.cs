using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Targeting")]
    public string playerTag = "Player";
    
    [Header("Combat Settings")]
    public float attackRange = 2f;
    public float timeBetweenAttacks = 1.5f;

    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    private float nextAttackTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // Find player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"Player not found! Make sure the player GameObject has the tag '{playerTag}'.");
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            // In attack range: stop moving and attack
            agent.isStopped = true;
            
            // Rotate to face the player
            Vector3 direction = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + timeBetweenAttacks;
            }
            
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
            }
        }
        else
        {
            // Out of attack range: chase the player
            agent.isStopped = false;
            agent.SetDestination(player.position);

            if (animator != null)
            {
                animator.SetBool("IsMoving", true);
            }
        }
    }

    void Attack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        Debug.Log("Enemy attacked the player!");
        // TODO: Deal damage to the player here
    }
}
