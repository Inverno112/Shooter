using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent))]

public class Follower : MonoBehaviour, IDamagable
{
    [Header("References")]
        public Transform target;
        public Vector3 targetPosition;
    private Animator animator;
    private bool canAttack;
    NavMeshAgent agent;
    AudioSource audio;
    [SerializeField] AudioClip[] clips = new AudioClip[3];


    [Header("Follower Stats")]
    byte maxLife;
    short life;
    byte attackPower;
    float attackRange;
    public bool isDead => life <= 0;
    float agentTimer;
    private bool canMove;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        if(audio == null) audio = GetComponent<AudioSource>();
        canAttack = true;
        life = (short)(maxLife * 100);
    }
    public void Dmg(byte Dmg, Transform source)
    {
        if(life-Dmg>0)
        {
            life -= Dmg;
        }
        else
        {
            life = 0;
            agent.isStopped = true;
            animator.SetTrigger("Die");
            
            Destroy(gameObject,50);
            enabled = false;
        }
    }
    


    public void Hunt(Transform enemy)
    {
        audio.PlayOneShot(clips[0]);
        target = enemy;  
        StopCoroutine("Wandering");      
        Attack();
    }

    internal void Attack()
    {
        if (target)
        {
            
            if(Vector3.Distance(transform.position, target.position) <= attackRange&&canAttack)
            {
                target.GetComponent<IDamagable>().Dmg(attackPower, transform);
                animator.SetTrigger("Attack");
                canAttack = false;
                if(!audio.isPlaying&&UnityEngine.Random.Range(0,100)<20)audio.PlayOneShot(clips[1]);
                StartCoroutine(AttackCooldown());
            }
            else
            {
                animator.SetInteger("action", 1);
                agent.SetDestination(target.position);
            }

        }
        else
        {
            target = null;
            animator.SetInteger("action", 0);
        }
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(1f); // Wait for 1 second before allowing the next attack
        canAttack = true; // Allow the next attack
    }

    IEnumerator IdleCooldown()
    {
        yield return new WaitForSeconds(1f); // Wait for 1 second before allowing the next attack
        canMove = true; // Allow the next attack
    }

    public void EvaluateMoveTo()
    {
        if(agent.pathPending || agent.remainingDistance > 0.1f&&Random.Range(0,100)<50)
        {           
            
            //transform movement to a position, used for wandering
            targetPosition = transform.position+UnityEngine.Random.insideUnitSphere * 10f; // Random direction within a sphere of radius 10
            NavMeshHit hit;
          if (NavMesh.SamplePosition(targetPosition, out hit, 10f, NavMesh.AllAreas))
          {
                agent.SetDestination(hit.position);
                animator.SetInteger("action", 1);
          }
        }else
        {
            animator.SetInteger("action", 0);
        }
    }


}
