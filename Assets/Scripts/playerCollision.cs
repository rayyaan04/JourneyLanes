using UnityEngine;

public class playerCollision : MonoBehaviour
{
    public playermovement movement; // references the playermovement script to be used

    void OnCollisionEnter (Collision collisionInfo) // when a collision occurs, it gets the info from collision
    {
        if (collisionInfo.collider.tag == "Obstacle") // if the object hit has the tag "obstacle," this is ran
        {
            movement.enabled = false; // stops all movement to provide a visual black box response
            FindFirstObjectByType<GameManager>().EndGame(); // calls the EndGame() function if collision is detected

                
        }
    }
}
