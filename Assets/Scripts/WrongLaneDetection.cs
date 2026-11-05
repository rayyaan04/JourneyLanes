using UnityEngine;
using TMPro;  // For TextMeshPro
using UnityEngine.SceneManagement; // For restarting the level

public class LaneDetection : MonoBehaviour
{
    private float wrongLaneTimer = 0; // timer to see how long user stays in trigger zone
    private bool isInWrongLane = false;
    public TMP_Text warningText; 

    void Start()
    {
        warningText.text = ""; // Makes the warning text start empty
    }
    private void OnTriggerEnter(Collider carCollider)
    {
        if (this.CompareTag("RightLane"))
        { // checks if the object with Player tag entered the trigger zone with RightLane tag
            if (carCollider.CompareTag("Player"))
            {
                isInWrongLane = true; // the car is now in the right lane
            }
        }
    }

    private void OnTriggerStay(Collider carCollider)
    {
        if (this.CompareTag("RightLane"))
        {
            if (carCollider.CompareTag("Player"))
            {
                if (isInWrongLane == true)
                {
                    wrongLaneTimer = wrongLaneTimer + Time.deltaTime; // Start counting the time the car stays in the wrong lane
                }
                if (wrongLaneTimer > 2 & wrongLaneTimer < 2.1) // warn after 2 seconds
                {
                    warningText.text = "Warning: Stay in the left lane"; // output warning to text
                    FindFirstObjectByType<PointsManager>().DecreasePoints(5, "Driving in wrong lane");
                }
                if (wrongLaneTimer >= 5)
                {
                    warningText.text = "Spent too much time in wrong lane. Restarting level"; // Show penalty message
                                                                                              // Restarts the level after 2 seconds
                    Invoke("RestartLevel", 2);
                }
            }
                
        }
    }
    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnTriggerExit(Collider carCollider)
    {
        if (this.CompareTag("RightLane"))
        {
            if (carCollider.CompareTag("Player"))
            {
                isInWrongLane = false;
                wrongLaneTimer = 0f; // Reset the timer when leaving the right lane
                warningText.text = ""; // Clear warning text when leaving the right lane
            }
        }
    }
}
