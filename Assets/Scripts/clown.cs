using UnityEngine;
using UnityEditor.AI;
using System.Collections;
using UnityEngine.AI;
public class clown : MonoBehaviour
{
    public Transform player;
    public Animator animator;
    public NavMeshAgent agent;
    public float atkRange;
    public float speedMove;
    public float atkCD;
    public float atkTimer;

    private void Update()
    {
        if (Vector3.Distance(transform.position, player.position) > atkRange)
        {
            animator.SetBool("run", true);
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            animator.SetBool("run", false);
            agent.isStopped = true;
        }
    }
}
