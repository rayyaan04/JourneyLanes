using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class StopSignTriggerDetection : MonoBehaviour
{
    private float stopSignTimer = 0f; // timer to track how long car waits at stop sign
    private bool passedCheck = false;
    private bool passedSignWithoutWaiting = false; // checks if user went past stop sign before 3 seconds pass
    public TMP_Text warningText;
    public GameObject stopSignSecondTrigger;
    public GameObject stopSignFirstTrigger;
    private float warningMessageTimer = 0f; // timer to see how long error message has been displayed for
    private bool showWarningMessage = false; // boolean to check whether error message shuold be displayed

    void Start()
    {
        warningText.text = ""; // makes the warning text start empty
    }

    // trigger for when the player enters the StopSignInitial trigger zone
    private void OnTriggerEnter(Collider carCollider)
    {

        if (carCollider.CompareTag("Player"))
        { // checks if the player is the collider, and if the collided object is the initial stop sign trigger
            if (this.CompareTag("StopSignInitial"))
            {
                stopSignTimer = 0f;  // resets the timer when entering the initial trigger zone
            }

            if (this.CompareTag("StopSignFinal"))
            {
                Debug.Log("Car entered the StopSignFinal trigger zone");
                // if the car enters the second trigger before the timer ends show the warning
                if (stopSignTimer < 3f)
                { // deducts points when the user passes stop sign without waiting
                    FindFirstObjectByType<PointsManager>().DecreasePoints(150, "Passed a stop sign without waiting.");
                    passedSignWithoutWaiting = true; 
                    warningText.text = "You didn't wait at the stop sign for 3 seconds. Points removed";
                    Debug.Log("car went past stop sign too early");
                    showWarningMessage = true; // triggers error message display
                    warningMessageTimer = 0f; // reset error message timer
                    stopSignSecondTrigger.SetActive(false); 
                    stopSignFirstTrigger.SetActive(false); // disables both triggers (line above is commented for htis too)
                }
            }
        }
    }

    // trigger for when the player stays inside the StopSignInitial trigger zone
    private void OnTriggerStay(Collider carCollider)
    {
        if (carCollider.CompareTag("Player"))
        { // checks if the player car is the one staying inside the initial trigger box 
            if (this.CompareTag("StopSignInitial"))
            {
                stopSignTimer = stopSignTimer + Time.deltaTime; // start timer for how long the car stays in the trigger zone
                Debug.Log("Stop sign timer: " + stopSignTimer);

                if (stopSignTimer >= 3f)
                {
                    passedCheck = true;
                    Debug.Log("player has waited 3 seconds");
                    stopSignSecondTrigger.SetActive(false); // deactivate the second trigger zone
                }
            }
        }
    }

    // Trigger for when the player exits either trigger zone
    private void OnTriggerExit(Collider carCollider)
    {
        if (carCollider.CompareTag("Player"))
        { // checks if the player is the collider, and if the collided object is the initial stop sign trigger
            if (this.CompareTag("StopSignInitial"))
            {  // reset all the states of all variables when exiting the trigger zone - 
                // good for when theres multiple stop signs in one level
                if (passedCheck == true) {
                    FindFirstObjectByType<PointsManager>().IncreasePoints(50, "Safely waited at a stop sign.");
                    passedCheck = false;
                    stopSignFirstTrigger.SetActive(false);
                }
                stopSignTimer = 0f; 
                warningText.text = "";
            }
            if (this.CompareTag("StopSignFinal") && passedSignWithoutWaiting)
            {
                // Error message is already shown, we just need to ensure it's displayed for 3 seconds
                warningMessageTimer += Time.deltaTime;
                if (warningMessageTimer >= 3f)
                {
                    warningText.text = ""; // Clear the error message after 3 seconds
                    showWarningMessage = false; // Stop showing the error message
                    warningMessageTimer = 0f; // Reset the timer
                }
            }
        }
    }

    void Update()
    {
        // starts a timer on the warning message if it is called to be true
        if (showWarningMessage)
        {
            warningMessageTimer += Time.deltaTime;

            // after 3 seconds clear the error message, hide the text and reset the state of variables
            if (warningMessageTimer >= 3f)
            {
                warningText.text = ""; // clear the error message
                showWarningMessage = false; // stop showing the error message
                warningMessageTimer = 0f; // reset the timer
            }
        }
    }
}
