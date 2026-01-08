using UnityEngine;

public class SuperPill : MonoBehaviour
{

    private PlayerController playerControllerScript;
    void Start()
    {
        playerControllerScript = FindAnyObjectByType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playerControllerScript.isInvincible = true;
            Destroy(gameObject);
        }
    }
}

