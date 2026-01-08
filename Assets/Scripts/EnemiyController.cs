using UnityEngine;
using UnityEngine.AI;

public class EnemiyController : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent enemy;

    public Transform home;

    public PlayerController playerControllerScript;

    void Start()
    {
        enemy = GetComponent<NavMeshAgent>();
        playerControllerScript = FindAnyObjectByType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        EnemyMove();
    }

    void EnemyMove()
    {
        if (playerControllerScript.isInvincible)
        {
            enemy.destination = home.position;
         
        }
        else
        {
            enemy.destination = player.position;
        }
            
    }
}