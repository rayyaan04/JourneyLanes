using UnityEngine;

public class endtri : MonoBehaviour
{
    public GameManager gameManager;
    private void OnTriggerEnter(Collider other) 
    { // Upon colliding with the end goal trigger, this function is activated
        if (other.CompareTag("Player")) // stops an NPC car reaching the goal from completing the level
        {
            gameManager.CompleteLevel(); // Calls the CompleteLevel function in the game manager
        }
    }
}
