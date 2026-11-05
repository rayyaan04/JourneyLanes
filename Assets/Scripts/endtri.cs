using UnityEngine;

public class endtri : MonoBehaviour
{
    public GameManager gameManager;
    private void OnTriggerEnter(Collider other) 
    { // Upon colliding with the end goal trigger, this function is activated
        gameManager.CompleteLevel(); // Calls the CompleteLevel function in the game manager
    }
}
