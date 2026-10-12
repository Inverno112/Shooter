using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Raged : MonoBehaviour,IDamagable
{
[Header("Action Attributes")]
    bool isFollowing = false; // Flag to indicate if the NPC is following the player    
    bool canAtt = true; // Flag to indicate if the NPC can attack

    private bool isDead = false;
    private WaitForSeconds wait02;
    private WaitForSeconds waitCD;
    private float updateSpeed = 0.2f; // Update speed for movement and attack checks
    private NavMeshAgent agent;
    private Transform target;
    [SerializeField]AudioSource EnvSound;
    [SerializeField]private LayerMask EnemyMask;
    
    [Header("Npc Stats")]
    private int life;  
    [SerializeField]private float detectionRange=15f;    
    [SerializeField]private byte attackPower= 10; // NPC's attack power
    private Animator animator;
    [SerializeField]private float attackRange=2f;
    [SerializeField] private int maxLife;
    private float attackCooldown=2f;
    
    [SerializeField]private float speed=4f;
    [SerializeField]private float runSpeed=6f;

    AudioSource audio,env;
    //Sound:idle,move,chase,attack,detect enemy Env: walking sound,run sound
    //Add description for editor
    [Header("Audio Clips")]
    [Tooltip("Audio clips for various actions: idle, move, chase, attack, detect enemy, environment walking sound, environment running sound")]
    [SerializeField] AudioClip[] clips = new AudioClip[7];


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        if(audio == null) audio = GetComponent<AudioSource>();
        if(env==null) env = EnvSound.GetComponent<AudioSource>();

        if (agent != null)
        {
            agent.speed = speed;
            agent.stoppingDistance = attackRange;
        }
           wait02 = new WaitForSeconds(updateSpeed); // Cache once, zero GC later
           waitCD=new WaitForSeconds(attackCooldown); // Cache once, zero GC later
           life = maxLife*100; // Initialize life to maximum life

           StartCoroutine(Vigil()); // Start the vigilance coroutine     
           StartCoroutine(Wonder()); // Start the wandering coroutine                                                                                                                                                                                                   
    }

    IEnumerator Vigil()
    {
        yield return new WaitForSeconds(Random.Range(1f, 5f)); // Initial delay before starting vigilance
        while (true)
        {
            //uses overlap sphere with layer enemy to detect enemies in range
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange, EnemyMask);
            if (hitColliders.Length > 0)
            {
                // If enemies are detected, change state to attack and set the target
               Sound(4,true); // Play enemy detected sound
                target = hitColliders[0].transform; // Get the first detected enemy
                agent.SetDestination(target.position); // Move towards the enemy
                target.GetComponent<IDamagable>().Dmg(attackPower,transform); // Start attacking the detected enemy
                StartCoroutine(Attack());
                StopCoroutine(Wonder()); // Stop wandering while attacking
                StopCoroutine(Vigil());
                
            }
            yield return new WaitForSeconds(Random.Range(1f, 5f)); // Check for enemy presence every second
        }
    }
    IEnumerator Attack()
    {
        agent.speed = runSpeed; // Increase speed while attacking
        while (target)
        {
            if(Vector3.Distance(transform.position, target.position) <= attackRange&&canAtt)
            {
                // Perform attack logic here
                animator.SetTrigger("Attack");
                StartCoroutine(CD()); // Start the attack cooldown
                Sound(3,true); // Play attack sound
                // Apply damage to the target (assuming the target has a method to receive damage)
                target.GetComponent<IDamagable>()?.Dmg(attackPower, transform);
                
            }
            else
            {
                // Move towards the target if not in range
                agent.SetDestination(target.position);
                animator.SetInteger("action", 1); // Set the animator to the moving state
                Sound(2,false); // Play chasing sound
                EnvironmentSound(6); // Play environment running sound
                yield return wait02; // Wait for a short duration before checking again
            }
        }
        target = null; // Reset the target when it's no longer valid
        agent.speed = speed; // Reset speed to normal
        StartCoroutine(Vigil()); // Resume vigilance after the target is gone
        StartCoroutine(Wonder()); // Start wandering after the target is gone
    }

    IEnumerator Wonder()
    {
        while (true)
        {
            // Implement wandering logic here
        // For example, you can use a random point within a certain radius to move to
             MoveTo(); // Move to a new random point after reaching the destination 
            while(agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                EnvironmentSound(5); // Play environment walking sound while moving
                yield return wait02; // Wait until the NPC reaches the destination
            }
            
            Sound(0,false); // Play idle sound when the NPC is idle
            EnvironmentSound(5,true); // Stop environment walking sound when idle
            animator.SetInteger("action", 0); // Set the animator to the idle state
            yield return new WaitForSeconds(Random.Range(2,15));
        }       
       
    }

    IEnumerator CD()
    {
        canAtt = false;
        yield return waitCD;
        canAtt = true;
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
            StopAllCoroutines();     
            Destroy(gameObject,50);
            enabled = false;
        }
    }
    

    void MoveTo()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 10f; // Random point within a radius of 10 units
        randomDirection += transform.position; // Offset by the NPC's current position
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position); // Move to the random point on the NavMesh
            animator.SetInteger("action", 1); // Set the animator to the moving state            
        }        
    }

    void Sound(byte clip,bool stopSound)
    {
        if(stopSound&&audio.clip!=clips[clip])
        {
            audio.Stop();
        }
        if(audio.isPlaying) return;
        audio.PlayOneShot(clips[clip]);
    }
    void EnvironmentSound(byte clip,bool stopSound=true)
    {
        if(stopSound)
        {
            env.Stop();
            if(env.clip==clips[clip])
            {
                return;
            }
        }
        
        if(env.isPlaying) return;
        env.PlayOneShot(clips[clip]);
    }
}
