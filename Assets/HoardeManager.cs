using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class HoardeManager : MonoBehaviour,IDamagable
{
//Manage the not thinking monsters
[Header("Spawn Attributes")]
    [SerializeField]GameObject[] Prefabs;
    [SerializeField]Transform[] spawnPoints;
    [SerializeField]private LayerMask EnemyMask;
    [SerializeField]private int maxLife;

[Header("Control Attributes")]
    [SerializeField]private List<Follower> spawnedMonsters = new List<Follower>();
    [SerializeField]private int maxMonsters=10;
    private int currentMonsters=0;
    [SerializeField]private float spawnCooldown=5f;



    public void Dmg(byte Dmg, Transform source)
    {
        throw new System.NotImplementedException();
    }

    
    //Action Functions


    void MoveTo(Vector3 position)
    {
       
    }
    

    IEnumerator SpawnMonsters()
    {
        while (true)
        {
            if (currentMonsters < maxMonsters)
            {
                // Randomly select a prefab and spawn point
                GameObject prefabToSpawn = Prefabs[Random.Range(0, Prefabs.Length)];
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

                // Instantiate the monster and add it to the list
                GameObject spawnedMonster = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
                spawnedMonsters.Add(spawnedMonster.GetComponent<Follower>());
                currentMonsters++;
            }
            yield return new WaitForSeconds(spawnCooldown); // Wait for the cooldown before spawning the next monster
        }
    }

    IEnumerator MonsterBeh()
    {
        //Handles monster movement towards player and evaluates distance to player to attack or not
        //One overlap sphere and distance calculation per monster
        while (true)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 15f, EnemyMask);
            Transform[] targets = new Transform[hitColliders.Length];
            for (int i = 0; i < hitColliders.Length; i++)
            {
                targets[i] = hitColliders[i].transform;
            }
            foreach (Follower monster in spawnedMonsters)
            {
                if (monster)
                {                    
                    if(monster.isDead)
                    {
                        spawnedMonsters.Remove(monster);
                        currentMonsters--;
                        Destroy(monster.gameObject);
                        break;
                    }
                    if(monster.target==null){
                    // Check if the monster has a NavMeshAgent component
                        NavMeshAgent agent = monster.GetComponent<NavMeshAgent>();
                        if (agent != null)
                        {
                            // Find the closest enemy within the detection range
                            Transform closestEnemy = null;
                            float closestDistance = Mathf.Infinity;

                            foreach (Transform target in targets)
                            {
                                float distance =(target.position-transform.position).sqrMagnitude;
                                if (distance < closestDistance)
                                {
                                    closestDistance = distance;
                                    closestEnemy = target;
                                }
                            }

                            if (closestEnemy != null)
                            {
                                monster.Hunt(closestEnemy);
                            }
                            else
                            {
                                //monster wonders
                                monster.EvaluateMoveTo();
                            }
                        }
                    }
                    else
                    {
                        monster.Attack();
                    }
                }
            }
            yield return new WaitForSeconds(0.2f); // Wait for a short time before checking again
        }
    }


}/*
El manager solo debería decir:

"Estos son los enemigos visibles."

Y el Follower decidir:

ChooseClosest(targets);

Así mañana puedes hacer que un monstruo ataque al más débil, 
otro al más cercano y otro al que tenga menos vida sin tocar el manager.*/
